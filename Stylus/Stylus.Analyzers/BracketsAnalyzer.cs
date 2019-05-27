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
        public const string DiagnosticId = StylusManifest.BracketsAnalyzerId;
        internal static readonly LocalizableString Title = "Brackets rule violation";
        internal static readonly LocalizableString MessageFormat = "{0} should {1}";
        internal const string Category = StylusManifest.Category;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeParentheses, SyntaxKind.IfStatement);
        }

        private void AnalyzeParentheses(SyntaxNodeAnalysisContext context)
        {
            var children = context.Node.ChildNodes();
            var block = children.Where(n => n.IsKind(SyntaxKind.Block)).FirstOrDefault();
            if (block is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "Braces", "be used"));
                return;
            }
            var whiteSpaceTrivia = block.GetLeadingTrivia().Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
            if(!whiteSpaceTrivia.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "Braces", "be placed on their own lines"));
            }
        }
    }
}
