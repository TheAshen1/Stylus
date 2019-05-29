using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stylus.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BracketsAnalyzer : DiagnosticAnalyzer
    {
        public const string _diagnosticId = StylusManifest.BracketsAnalyzerId;
        internal static readonly LocalizableString _title = "Brackets analyzer";
        internal static readonly LocalizableString _messageFormat = "Code style violation: {0}";
        internal const string _category = StylusManifest.Category;

        internal static DiagnosticDescriptor _rule = new DiagnosticDescriptor(_diagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeParentheses, SyntaxKind.IfStatement);
        }

        private void AnalyzeParentheses(SyntaxNodeAnalysisContext context)
        {
            IEnumerable<SyntaxNode> children = context.Node.ChildNodes();
            SyntaxNode block = children.Where(n => n.IsKind(SyntaxKind.Block)).FirstOrDefault();
            if (block is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, context.Node.GetLocation(), "Braces should be used"));
                return;
            }
            IEnumerable<SyntaxTrivia> whiteSpaceTrivia = block.GetLeadingTrivia().Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
            if(!whiteSpaceTrivia.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, context.Node.GetLocation(), "Braces should be placed on their own lines"));
            }
        }
    }
}
