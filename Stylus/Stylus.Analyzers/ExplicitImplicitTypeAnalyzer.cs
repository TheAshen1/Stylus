using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stylus.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExplicitImplicitTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = StylusManifest.ExplicitImplicitTypeAnalyzerId;
        internal static readonly LocalizableString Title = "Type declaration rule violation";
        internal static readonly LocalizableString MessageFormat = "{0} type decalaration should be used";
        internal const string Category = StylusManifest.Category;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.LocalDeclarationStatement);
        }
        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var variableDeclaration = (context.Node as LocalDeclarationStatementSyntax).Declaration;
            var semanticModel = context.SemanticModel;
            var isTypeApparent = IsTypeApparentInDeclaration(variableDeclaration, semanticModel);
            if (isTypeApparent && !variableDeclaration.Type.IsVar)
            {
                context.ReportDiagnostic(
                   Diagnostic.Create(
                       Rule,
                       variableDeclaration.GetLocation(),
                       "Implicit"));
            }
            if (!isTypeApparent && variableDeclaration.Type.IsVar)
            {
                context.ReportDiagnostic(
                   Diagnostic.Create(
                       Rule,
                       variableDeclaration.GetLocation(),
                       "Explicit"));
            }

            //// other Conversion cases:
            ////      a. conversion with helpers like: int.Parse, TextSpan.From methods 
            ////      b. types that implement IConvertible and then invoking .ToType()
            ////      c. System.Convert.Totype()
            //var declaredTypeSymbol = semanticModel.GetTypeInfo(variableDeclaration.Type, cancellationToken).Type;
            //var expressionOnRightSide = initializerExpression.WalkDownParentheses();

            //var memberName = expressionOnRightSide.GetRightmostName();
            //if (memberName == null)
            //{
            //    return false;
            //}

            //var methodSymbol = semanticModel.GetSymbolInfo(memberName, cancellationToken).Symbol as IMethodSymbol;
            //if (methodSymbol == null)
            //{
            //    return false;
            //}

            //if (memberName.IsRightSideOfDot())
            //{
            //    var typeName = memberName.GetLeftSideOfDot();
            //    return IsPossibleCreationOrConversionMethod(methodSymbol, declaredTypeSymbol, semanticModel, typeName, cancellationToken);
            //}

            //return false;
        }

        private bool IsTypeApparentInDeclaration(VariableDeclarationSyntax variableDeclaration, SemanticModel semanticModel)
        {
            var initializer = variableDeclaration.Variables.Single().Initializer?.Value;
            if(initializer is null)
            {
                return false;
            }

            if(initializer.IsKind(SyntaxKind.CharacterLiteralExpression)||
                initializer.IsKind(SyntaxKind.DefaultLiteralExpression) ||
                initializer.IsKind(SyntaxKind.FalseLiteralExpression) ||
                initializer.IsKind(SyntaxKind.TrueLiteralExpression) ||
                initializer.IsKind(SyntaxKind.NumericLiteralExpression) ||
                initializer.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return true;
            }

            if (initializer.IsKind(SyntaxKind.ObjectCreationExpression) &&
                !initializer.IsKind(SyntaxKind.AnonymousObjectCreationExpression))
            {
                return true;
            }

            if (initializer.IsKind(SyntaxKind.CastExpression) ||
                initializer.IsKind(SyntaxKind.IsExpression) ||
                initializer.IsKind(SyntaxKind.AsExpression))
            {
                return true;
            }

            return false;
        }
    }
}
