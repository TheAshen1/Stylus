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
        public const string DiagnosticId = StylusManifest.StringLiteralAnalyzerId;
        internal static readonly LocalizableString Title = "String literal analyzer";
        internal static readonly LocalizableString MessageFormat = "Code style violation: {0}";
        internal const string Category = StylusManifest.Category;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeStringLiteral, SyntaxKind.ReturnStatement, SyntaxKind.EqualsValueClause);
            //context.RegisterSyntaxNodeAction(AnalyzeStringComparison, SyntaxKind.EqualsEqualsToken, SyntaxKind.ExclamationEqualsToken);
        }

        //private void AnalyzeStringComparison(SyntaxNodeAnalysisContext context)
        //{
        //    if (context.Node.IsKind(SyntaxKind.EqualsEqualsToken))
        //    {
        //        context.Node.
        //    }
        //}

        private void AnalyzeStringLiteral(SyntaxNodeAnalysisContext context)
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
