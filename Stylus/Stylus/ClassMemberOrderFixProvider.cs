using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stylus
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClassMemberOrderFixProvider)), Shared]
    public class ClassMemberOrderFixProvider : CodeFixProvider
    {
        private const string title = "Sort class members";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ClassMemberOrderAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => SortClassMembersAsync(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> SortClassMembersAsync(Document document, ClassDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var classMembers = declaration.ChildNodes().ToList();
            classMembers.Sort(new ClassMemberComparer());
            var dictionary = new Dictionary<SyntaxNode, SyntaxNode>();
            var oldClassMembers = declaration.ChildNodes().ToList();
            for (int i = 0; i < oldClassMembers.Count; i++)
            {
                dictionary.Add(oldClassMembers[i], classMembers[i]);
            }
            var newClassDeclaration = declaration.ReplaceNodes(oldClassMembers, (n1,n2) => dictionary[n1]);
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(declaration, newClassDeclaration);
            var formattedRoot = Formatter.Format(newRoot, new AdhocWorkspace());
            return document.WithSyntaxRoot(newRoot);
        }


        private class ClassMemberComparer : Comparer<SyntaxNode>
        {
            public override int Compare(SyntaxNode a, SyntaxNode b)
            {
                var kindA = a.Kind();
                var kindB = b.Kind();
                Util.TryGetAccessModifier(a, out Accessibility modifierA);
                Util.TryGetAccessModifier(b, out Accessibility modifierB);
                var keyA = Util._memberOrder.Where(p => p.Value == (kindA, modifierA)).Select(p => p.Key).FirstOrDefault();
                var keyB = Util._memberOrder.Where(p => p.Value == (kindB, modifierB)).Select(p => p.Key).FirstOrDefault();
                return keyA - keyB;
            }
        }
    }
}
