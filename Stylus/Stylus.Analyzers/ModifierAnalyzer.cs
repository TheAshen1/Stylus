using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stylus.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ModifierAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = StylusManifest.ModifierAnalyzerId;
        internal static readonly LocalizableString Title = "Access modifier rule violation";
        internal static readonly LocalizableString MessageFormat = "Access modifiers should always be specified";
        internal const string Category = StylusManifest.Category;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzerModifier,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DelegateDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.EventDeclaration,
                SyntaxKind.EventFieldDeclaration,
                SyntaxKind.FieldDeclaration,
                SyntaxKind.IndexerDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.StructDeclaration);
        }

        private void AnalyzerModifier(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.LocalDeclarationStatement))
            {
                return;
            }

            SyntaxTokenList? modifiers = null;

            modifiers = (context.Node as BaseTypeDeclarationSyntax)?.Modifiers ??
                        (context.Node as DelegateDeclarationSyntax)?.Modifiers ??
                        (context.Node as BaseFieldDeclarationSyntax)?.Modifiers ??
                        (context.Node as BasePropertyDeclarationSyntax)?.Modifiers ??
                        (context.Node as BaseMethodDeclarationSyntax)?.Modifiers;

            if (modifiers.HasValue && modifiers.Value.Count == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }
    }
}
