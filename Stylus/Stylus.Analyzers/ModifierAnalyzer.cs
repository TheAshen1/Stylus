using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stylus.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ModifierAnalyzer : DiagnosticAnalyzer
    {
        public const string _diagnosticId = StylusManifest.ModifierAnalyzerId;
        internal static readonly LocalizableString _title = "Access modifier rule violation";
        internal static readonly LocalizableString _messageFormat = "Access modifiers should always be specified";
        internal const string _category = StylusManifest.Category;

        internal static DiagnosticDescriptor _rule = new DiagnosticDescriptor(_diagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzerModifier,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DelegateDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.EventDeclaration,
                SyntaxKind.EventFieldDeclaration,
                SyntaxKind.FieldDeclaration,
                SyntaxKind.IndexerDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.StructDeclaration);
        }

        private void AnalyzerModifier(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.LocalDeclarationStatement) 
                || context.Node.Parent.IsKind(SyntaxKind.InterfaceDeclaration))
            {
                return;
            }

            SyntaxTokenList? modifiers = null;
            
            modifiers = (context.Node as BaseTypeDeclarationSyntax)?.Modifiers ??
                        (context.Node as DelegateDeclarationSyntax)?.Modifiers ??
                        (context.Node as BaseFieldDeclarationSyntax)?.Modifiers ??
                        (context.Node as BasePropertyDeclarationSyntax)?.Modifiers ??
                        (context.Node as BaseMethodDeclarationSyntax)?.Modifiers;
            if (!modifiers.HasValue)
            {
                return;
            }

            if (!modifiers.Value.Where(m => IsAccessModifier(m)).Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, context.Node.GetLocation()));
            }
        }

        private bool IsAccessModifier(SyntaxToken modifier)
        {
            bool isAccessModifier = modifier.IsKind(SyntaxKind.PublicKeyword) 
                                    || modifier.IsKind(SyntaxKind.PrivateKeyword)
                                    || modifier.IsKind(SyntaxKind.ProtectedKeyword)
                                    || modifier.IsKind(SyntaxKind.InternalKeyword);
            return isAccessModifier;
        }
    }
}
