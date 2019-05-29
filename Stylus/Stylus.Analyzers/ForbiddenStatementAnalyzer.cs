using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stylus.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ForbiddenStatementAnalyzer : DiagnosticAnalyzer
    {
        public const string _diagnosticId = StylusManifest.ForbiddenStatementAnalyzerId;
        internal static readonly LocalizableString _title = "Forbidden statement analyzer";
        internal static readonly LocalizableString _messageFormat = "Code style violation: {0}";
        internal const string _category = StylusManifest.Category;

        internal static DiagnosticDescriptor _rule = new DiagnosticDescriptor(_diagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeForbiddenStatements, SyntaxKind.ElseClause, SyntaxKind.DoStatement);
        }

        private void AnalyzeForbiddenStatements(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.ElseClause))
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, context.Node.GetLocation(), "Else statement should be avoided"));
            }
            if (context.Node.IsKind(SyntaxKind.DoStatement))
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, context.Node.GetLocation(), "Do-While statement should be avoided"));
            }
        }
    }
}
