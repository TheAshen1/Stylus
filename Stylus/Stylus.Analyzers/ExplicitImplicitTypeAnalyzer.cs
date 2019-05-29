using System;
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

            SimpleNameSyntax memberName = GetRightmostName(GetRightmostInvocationExpression(initializerExpression));
            if (memberName == null)
            {
                return false;
            }

            var methodSymbol = semanticModel.GetSymbolInfo(memberName, cancellationToken).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return false;
            }

            if (IsRightSideOfDot(memberName))
            {
                ExpressionSyntax containingTypeName = GetLeftSideOfDot(memberName);
                return IsPossibleCreationOrConversionMethod(methodSymbol, typeInDeclaration, semanticModel, containingTypeName, cancellationToken);
            }

            return false;
        }

        private static ExpressionSyntax GetRightmostInvocationExpression(ExpressionSyntax node)
        {
            if (node is AwaitExpressionSyntax awaitExpression && awaitExpression.Expression != null)
            {
                return GetRightmostInvocationExpression(awaitExpression.Expression);
            }

            if (node is InvocationExpressionSyntax invocationExpression && invocationExpression.Expression != null)
            {
                return GetRightmostInvocationExpression(invocationExpression.Expression);
            }

            if (node is ConditionalAccessExpressionSyntax conditional)
            {
                return GetRightmostInvocationExpression(conditional.WhenNotNull);
            }

            return node;
        }

        public static SimpleNameSyntax GetRightmostName(ExpressionSyntax node)
        {
            if (node is MemberAccessExpressionSyntax memberAccess && memberAccess.Name != null)
            {
                return memberAccess.Name;
            }

            if (node is QualifiedNameSyntax qualified && qualified.Right != null)
            {
                return qualified.Right;
            }

            if (node is SimpleNameSyntax simple)
            {
                return simple;
            }

            if (node is ConditionalAccessExpressionSyntax conditional)
            {
                return GetRightmostName(conditional.WhenNotNull);
            }

            if (node is MemberBindingExpressionSyntax memberBinding)
            {
                return memberBinding.Name;
            }

            if (node is AliasQualifiedNameSyntax aliasQualifiedName && aliasQualifiedName.Name != null)
            {
                return aliasQualifiedName.Name;
            }

            return null;
        }

        public static bool IsRightSideOfDot(ExpressionSyntax name)
        {
            return IsMemberAccessExpressionName(name) || IsRightSideOfQualifiedName(name) || IsQualifiedCrefName(name);
        }

        public static bool IsMemberAccessExpressionName(ExpressionSyntax expression)
        {
            return (IsParentKind(expression, SyntaxKind.SimpleMemberAccessExpression) && ((MemberAccessExpressionSyntax)expression.Parent).Name == expression) ||
                   (IsMemberBindingExpressionName(expression));
        }

        public static bool IsParentKind(SyntaxNode node, SyntaxKind kind)
        {
            return node != null && node.Parent.IsKind(kind);
        }

        private static bool IsMemberBindingExpressionName(ExpressionSyntax expression)
        {
            return IsParentKind(expression, SyntaxKind.MemberBindingExpression) &&
                ((MemberBindingExpressionSyntax)expression.Parent).Name == expression;
        }

        public static bool IsRightSideOfQualifiedName(ExpressionSyntax expression)
        {
            return IsParentKind(expression, SyntaxKind.QualifiedName) && ((QualifiedNameSyntax)expression.Parent).Right == expression;
        }

        public static bool IsQualifiedCrefName(ExpressionSyntax expression)
        {
            return IsParentKind(expression, SyntaxKind.NameMemberCref) && IsParentKind(expression.Parent, SyntaxKind.QualifiedCref);
        }

        public static ExpressionSyntax GetLeftSideOfDot(SimpleNameSyntax name)
        {
            //Debug.Assert(name.IsMemberAccessExpressionName() || name.IsRightSideOfQualifiedName() || name.IsParentKind(SyntaxKind.NameMemberCref));
            if (IsMemberAccessExpressionName(name))
            {
                ConditionalAccessExpressionSyntax conditionalAccess = GetParentConditionalAccessExpression(name);
                if (conditionalAccess is null)
                {
                    return ((MemberAccessExpressionSyntax)name.Parent).Expression;
                }
                return conditionalAccess.Expression;
            }
            if (IsRightSideOfQualifiedName(name))
            {
                return ((QualifiedNameSyntax)name.Parent).Left;
            }
            return ((QualifiedCrefSyntax)name.Parent.Parent).Container;
        }

        public static ConditionalAccessExpressionSyntax GetParentConditionalAccessExpression(SyntaxNode node)
        {
            SyntaxNode current = node;
            while (current?.Parent != null)
            {
                if (IsParentKind(current, SyntaxKind.ConditionalAccessExpression) &&
                    ((ConditionalAccessExpressionSyntax)current.Parent).WhenNotNull == current)
                {
                    return (ConditionalAccessExpressionSyntax)current.Parent;
                }

                current = current.Parent;
            }

            return null;
        }

        private static bool IsPossibleCreationOrConversionMethod(IMethodSymbol methodSymbol,
            ITypeSymbol typeInDeclaration,
            SemanticModel semanticModel,
            ExpressionSyntax containingTypeName,
            CancellationToken cancellationToken)
        {
            if (methodSymbol.ReturnsVoid)
            {
                return false;
            }

            ITypeSymbol containingType = semanticModel.GetTypeInfo(containingTypeName, cancellationToken).Type;

            return IsPossibleCreationMethod(methodSymbol, typeInDeclaration, containingType)
                || IsPossibleConversionMethod(methodSymbol, typeInDeclaration, containingType, semanticModel, cancellationToken);
        }

        private static bool IsPossibleCreationMethod(IMethodSymbol methodSymbol,
           ITypeSymbol typeInDeclaration,
           ITypeSymbol containingType)
        {
            if (!methodSymbol.IsStatic)
            {
                return false;
            }

            return IsContainerTypeEqualToReturnType(methodSymbol, typeInDeclaration, containingType);
        }

        private static bool IsContainerTypeEqualToReturnType(IMethodSymbol methodSymbol,
           ITypeSymbol typeInDeclaration,
           ITypeSymbol containingType)
        {
            ITypeSymbol returnType = UnwrapTupleType(methodSymbol.ReturnType);

            if (GetTypeArguments(UnwrapTupleType(typeInDeclaration)).Length > 0 ||
                GetTypeArguments(containingType).Length > 0)
            {
                return UnwrapTupleType(containingType).Name.Equals(returnType.Name);
            }
            return UnwrapTupleType(containingType).Equals(returnType);
        }

        public static ImmutableArray<ITypeSymbol> GetTypeArguments(ISymbol symbol)
        {
            switch (symbol)
            {
                case IMethodSymbol m: return m.TypeArguments;
                case INamedTypeSymbol nt: return nt.TypeArguments;
                default: return ImmutableArray.Create<ITypeSymbol>();
            }
        }

        private static bool IsPossibleConversionMethod(IMethodSymbol methodSymbol,
           ITypeSymbol typeInDeclaration,
           ITypeSymbol containingType,
           SemanticModel semanticModel,
           CancellationToken cancellationToken)
        {
            ITypeSymbol returnType = methodSymbol.ReturnType;
            string returnTypeName = IsNullable(returnType)
                ? GetTypeArguments(returnType).First().Name
                : returnType.Name;

            return methodSymbol.Name.Equals("To" + returnTypeName, StringComparison.Ordinal);
        }

        private static ITypeSymbol UnwrapTupleType(ITypeSymbol symbol)
        {
            if (symbol is null)
            {
                return null;
            }
            if (!(symbol is INamedTypeSymbol namedTypeSymbol))
            {
                return symbol;
            }
            return namedTypeSymbol.TupleUnderlyingType ?? symbol;
        }

        public static bool IsNullable(ITypeSymbol symbol)
            => symbol?.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }
}
