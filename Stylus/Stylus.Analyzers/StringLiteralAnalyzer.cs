using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stylus.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringLiteralAnalyzer : DiagnosticAnalyzer
    {
        public const string _diagnosticId = StylusManifest.StringLiteralAnalyzerId;
        internal static readonly LocalizableString _title = "String literal analyzer";
        internal static readonly LocalizableString _messageFormat = "Code style violation: {0}";
        internal const string _category = StylusManifest.Category;

        internal static DiagnosticDescriptor _rule = new DiagnosticDescriptor(_diagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeReturnLiteral, SyntaxKind.ReturnStatement, SyntaxKind.EqualsValueClause);
            context.RegisterOperationAction(AnalyzerBinaryOperation, OperationKind.Binary);
        }

        private void AnalyzerBinaryOperation(OperationAnalysisContext context)
        {
            var expression = context.Operation.Syntax as BinaryExpressionSyntax;
            if ((expression.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken) || expression.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
                && expression.Right.IsKind(SyntaxKind.StringLiteralExpression)
                && String.IsNullOrEmpty((expression.Right as LiteralExpressionSyntax).Token.ValueText))
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, expression.GetLocation(), "String.IsNullOrEmpty should be used instead"));
            }
        }

        private void AnalyzeReturnLiteral(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.ReturnStatement))
            {
                ExpressionSyntax returnExpression = (context.Node as ReturnStatementSyntax).Expression;
                if (returnExpression.IsKind(SyntaxKind.StringLiteralExpression) && String.IsNullOrEmpty((returnExpression as LiteralExpressionSyntax).Token.ValueText))
                {
                    context.ReportDiagnostic(Diagnostic.Create(_rule, context.Node.GetLocation(), "String.Empty should be used instead"));
                }
            }
            if (context.Node.IsKind(SyntaxKind.EqualsValueClause))
            {
                ExpressionSyntax value = (context.Node as EqualsValueClauseSyntax).Value;
                if (value.IsKind(SyntaxKind.StringLiteralExpression) && String.IsNullOrEmpty((value as LiteralExpressionSyntax).Token.ValueText))
                {
                    context.ReportDiagnostic(Diagnostic.Create(_rule, context.Node.GetLocation(), "String.Empty should be used instead"));

                }
            }
        }
    }
}
