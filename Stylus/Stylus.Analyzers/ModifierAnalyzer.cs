using System;
using System.Collections.Generic;
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
    public class ModifierAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ModifierAnalyzer";
        internal static readonly LocalizableString Title = "ModifierAnalyzer Title";
        internal static readonly LocalizableString MessageFormat = "Access modifiers should always be specified";
        internal const string Category = StylusManifest.Category;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(CheckModifier,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DelegateDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.EventDeclaration,
                SyntaxKind.EventFieldDeclaration,
                SyntaxKind.FieldDeclaration,
                SyntaxKind.IndexerDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.StructDeclaration);
        }

        private void CheckModifier(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.IsKind(SyntaxKind.LocalDeclarationStatement))
            {
                return;
            }

            SyntaxTokenList? modifiers = null;

            modifiers = (context.Node as BaseTypeDeclarationSyntax)?.Modifiers ??
                        (context.Node as DelegateDeclarationSyntax)?.Modifiers ??
                        (context.Node as BaseFieldDeclarationSyntax)?.Modifiers ??
                        (context.Node as BasePropertyDeclarationSyntax)?.Modifiers ??
                        (context.Node as BaseMethodDeclarationSyntax)?.Modifiers;

            if (modifiers.HasValue && modifiers.Value.Count == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }
    }
}
