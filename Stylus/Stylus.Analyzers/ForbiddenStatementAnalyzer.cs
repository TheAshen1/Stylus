using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stylus.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ForbiddenStatementAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = StylusManifest.ForbiddenStatementAnalyzerId;
        internal static readonly LocalizableString Title = "This statement should be avoided";
        internal static readonly LocalizableString MessageFormat = "{0} statement should be avoided";
        internal const string Category = StylusManifest.Category;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzerAction, SyntaxKind.ElseClause);
            context.RegisterSyntaxNodeAction(AnalyzerAction, SyntaxKind.DoStatement);
        }

        private void AnalyzerAction(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.ElseClause))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "Else"));
            }
            if (context.Node.IsKind(SyntaxKind.DoStatement))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "Do-While"));
            }
        }
    }
}
