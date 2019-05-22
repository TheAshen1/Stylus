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
    public class NamingAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = StylusManifest.NamingAnalyzerId;
        internal static readonly LocalizableString Title = "Naming must follow codestyle";
        internal static readonly LocalizableString MessageFormat = "{0} must {1}";
        internal const string Category = StylusManifest.Category;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(TypeAndFunctionNameCheck, SyntaxKind.MethodDeclaration, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeAction(LocalVariableNameCheck, SyntaxKind.LocalDeclarationStatement);
            context.RegisterSyntaxNodeAction(GlobaVariableNameCheck, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(EnumNameCheck, SyntaxKind.EnumDeclaration);
        }

        private void EnumNameCheck(SyntaxNodeAnalysisContext context)
        {
            var identifier = (context.Node as EnumDeclarationSyntax).Identifier.ToString();
            if (String.IsNullOrEmpty(identifier))
            {
                return;
            }
            if (!Char.IsUpper(identifier[0]))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "Enums", "be in PascalCase"));
            }
            if (identifier.IndexOf("Enum", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "Enums", @"not containt suffix ""Enum"""));
            }
        }

        private void TypeAndFunctionNameCheck(SyntaxNodeAnalysisContext context)
        {
            var identifier = String.Empty;
            var kind = SyntaxKind.None;

            if (context.Node.IsKind(SyntaxKind.MethodDeclaration))
            {
                identifier = (context.Node as MethodDeclarationSyntax).Identifier.ToString();
                kind = SyntaxKind.MethodDeclaration;
            }
            if (context.Node.IsKind(SyntaxKind.ClassDeclaration))
            {
                identifier = (context.Node as ClassDeclarationSyntax).Identifier.ToString();
                kind = SyntaxKind.ClassDeclaration;
            }
            if (context.Node.IsKind(SyntaxKind.StructDeclaration))
            {
                identifier = (context.Node as StructDeclarationSyntax).Identifier.ToString();
                kind = SyntaxKind.StructDeclaration;
            }
            if (context.Node.IsKind(SyntaxKind.PropertyDeclaration))
            {
                identifier = (context.Node as PropertyDeclarationSyntax).Identifier.ToString();
                kind = SyntaxKind.PropertyDeclaration;
            }
            if (context.Node.IsKind(SyntaxKind.DelegateDeclaration))
            {
                identifier = (context.Node as DelegateDeclarationSyntax).Identifier.ToString();
                kind = SyntaxKind.DelegateDeclaration;
            }

            if (String.IsNullOrEmpty(identifier))
            {
                return;
            }
            if (!Char.IsUpper(identifier[0]))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), kind.ToString(), "be in PascalCase"));
            }
        }

        private void LocalVariableNameCheck(SyntaxNodeAnalysisContext context)
        {
            var variable = context.Node as LocalDeclarationStatementSyntax;
            var identifier = variable.Declaration.Variables[0].Identifier.ToString();
            if (String.IsNullOrEmpty(identifier))
            {
                return;
            }
            if (!Char.IsLower(identifier[0]))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "Local variables", "be in camelCase"));
            }
        }

        private void GlobaVariableNameCheck(SyntaxNodeAnalysisContext context)
        {
            var field = context.Node as FieldDeclarationSyntax;
            var identifier = field.Declaration.Variables[0].Identifier.ToString();
            if (String.IsNullOrEmpty(identifier))
            {
                return;
            }
            if (!identifier.StartsWith("_"))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "Fields", "start with underscore"));
                return;
            }
            if (!Char.IsLower(identifier[1]))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), "Fields", "be in camelCase"));
            }
        }
    }
}
