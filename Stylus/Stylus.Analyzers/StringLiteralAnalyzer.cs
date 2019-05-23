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
        internal static readonly LocalizableString MessageFormat = "String.Empty should be used instead";
        internal const string Category = StylusManifest.Category;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeStringLiteral, SyntaxKind.ReturnStatement, SyntaxKind.LocalDeclarationStatement, SyntaxKind.De);
        }

        private void AnalyzeStringLiteral(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.StringLiteralExpression) && String.IsNullOrEmpty((context.Node as LiteralExpressionSyntax).Token.ValueText))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }
    }
}
