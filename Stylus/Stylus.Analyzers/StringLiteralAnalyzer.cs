using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stylus.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringLiteralAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = StylusManifest.StringLiteralAnalyzerId;
        internal static readonly LocalizableString Title = "String literal analyzer";
        internal static readonly LocalizableString MessageFormat = "Code style violation: {0}";
        internal const string Category = StylusManifest.Category;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

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
                context.ReportDiagnostic(Diagnostic.Create(Rule, expression.GetLocation(), "String.IsNullOrEmpty should be used instead"));
            }
        }

        private void AnalyzeReturnLiteral(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.ReturnStatement))
            {
                var returnExpression = (context.Node as ReturnStatementSyntax).Expression;
                if (returnExpression.IsKind(SyntaxKind.StringLiteralExpression) && String.IsNullOrEmpty((returnExpression as LiteralExpressionSyntax).Token.ValueText))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "String.Empty should be used instead"));
                }
            }
            if (context.Node.IsKind(SyntaxKind.EqualsValueClause))
            {
                var value = (context.Node as EqualsValueClauseSyntax).Value;
                if (value.IsKind(SyntaxKind.StringLiteralExpression) && String.IsNullOrEmpty((value as LiteralExpressionSyntax).Token.ValueText))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "String.Empty should be used instead"));

                }
            }
        }
    }
}
