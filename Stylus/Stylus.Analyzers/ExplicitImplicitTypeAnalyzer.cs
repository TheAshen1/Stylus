using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
            VariableDeclarationSyntax variableDeclaration = (context.Node as LocalDeclarationStatementSyntax).Declaration;
            bool isTypeApparent = IsTypeApparentInDeclaration(variableDeclaration, context.SemanticModel, context.CancellationToken);
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
        }

        private static bool IsTypeApparentInDeclaration(VariableDeclarationSyntax variableDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (variableDeclaration.Variables.Count != 1)
            {
                return false;
            }
            EqualsValueClauseSyntax initializer = variableDeclaration.Variables[0].Initializer;
            if (initializer == null)
            {
                return false;
            }

            ExpressionSyntax initializerExpression = variableDeclaration.Variables.Single().Initializer?.Value;
            TypeSyntax variableDeclarationType = variableDeclaration.Type is RefTypeSyntax refType ? refType.Type : variableDeclaration.Type;
            ITypeSymbol declaredTypeSymbol = semanticModel.GetTypeInfo(variableDeclarationType, cancellationToken).Type;
            return IsTypeApparentInAssignmentExpression(initializerExpression, semanticModel, declaredTypeSymbol, cancellationToken);
        }

        public static bool IsTypeApparentInAssignmentExpression(
           ExpressionSyntax initializerExpression,
           SemanticModel semanticModel,
           ITypeSymbol typeInDeclaration,
           CancellationToken cancellationToken)
        {
            if (initializerExpression.IsKind(SyntaxKind.TupleExpression))
            {
                var tuple = (TupleExpressionSyntax)initializerExpression;
                if (typeInDeclaration == null || !typeInDeclaration.IsTupleType)
                {
                    return false;
                }

                var tupleType = (INamedTypeSymbol)typeInDeclaration;
                if (tupleType.TupleElements.Length != tuple.Arguments.Count)
                {
                    return false;
                }

                for (int i = 0, n = tuple.Arguments.Count; i < n; i++)
                {
                    ArgumentSyntax argument = tuple.Arguments[i];
                    ITypeSymbol tupleElementType = tupleType.TupleElements[i].Type;

                    if (!IsTypeApparentInAssignmentExpression(argument.Expression, semanticModel, tupleElementType, cancellationToken))
                    {
                        return false;
                    }
                }
                return true;
            }

            if (initializerExpression.IsKind(SyntaxKind.DefaultExpression))
            {
                return true;
            }

            if (initializerExpression.IsKind(SyntaxKind.CharacterLiteralExpression) ||
                initializerExpression.IsKind(SyntaxKind.DefaultLiteralExpression) ||
                initializerExpression.IsKind(SyntaxKind.FalseLiteralExpression) ||
                initializerExpression.IsKind(SyntaxKind.TrueLiteralExpression) ||
                initializerExpression.IsKind(SyntaxKind.NumericLiteralExpression) ||
                initializerExpression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return true;
            }

            if (initializerExpression.IsKind(SyntaxKind.ObjectCreationExpression) &&
                !initializerExpression.IsKind(SyntaxKind.AnonymousObjectCreationExpression))
            {
                return true;
            }

            if (initializerExpression.IsKind(SyntaxKind.CastExpression) ||
                initializerExpression.IsKind(SyntaxKind.IsExpression) ||
                initializerExpression.IsKind(SyntaxKind.AsExpression))
            {
                return true;
            }

            var memberName = GetRightmostInvocationExpression(initializerExpression).GetRightmostName();
            if (memberName == null)
            {
                return false;
            }

            var methodSymbol = semanticModel.GetSymbolInfo(memberName, cancellationToken).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return false;
            }

            if (memberName.IsRightSideOfDot())
            {
                var containingTypeName = memberName.GetLeftSideOfDot();
                return IsPossibleCreationOrConversionMethod(methodSymbol, typeInDeclaration, semanticModel, containingTypeName, cancellationToken);
            }

            return false;
        }

    }
}
