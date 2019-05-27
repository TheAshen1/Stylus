using System;
using System.Collections.Immutable;
using System.Linq;
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
        internal static readonly LocalizableString Title = "Naming rule violation";
        internal static readonly LocalizableString MessageFormat = "Naming rule violation: {0}";
        internal const string Category = StylusManifest.Category;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeTypeAndFunctionName, SyntaxKind.MethodDeclaration, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.PropertyDeclaration, SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzerLocalVariableName, SyntaxKind.LocalDeclarationStatement);
            context.RegisterSyntaxNodeAction(AnalyzeGlobaVariableName, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeEnumName, SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeInterfaceName, SyntaxKind.InterfaceDeclaration);
        }

        private void AnalyzeInterfaceName(SyntaxNodeAnalysisContext context)
        {
            string identifier = (context.Node as InterfaceDeclarationSyntax).Identifier.ToString();
            if (String.IsNullOrWhiteSpace(identifier))
            {
                return;
            }
            if (identifier[0] != 'I')
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{SyntaxKind.InterfaceDeclaration} should start with I"));
                return;
            }
            if (identifier.Length > 1 && !Char.IsUpper(identifier[1]))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{SyntaxKind.InterfaceDeclaration} should be in PascalCase"));
                return;
            }
        }

        private void AnalyzeEnumName(SyntaxNodeAnalysisContext context)
        {
            string identifier = (context.Node as EnumDeclarationSyntax).Identifier.ToString();
            if (String.IsNullOrWhiteSpace(identifier))
            {
                return;
            }
            if (!Char.IsUpper(identifier[0]))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{SyntaxKind.EnumDeclaration} should be in PascalCase"));
                return;
            }
            if (identifier.IndexOf("Enum", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $@"{SyntaxKind.EnumDeclaration} should not contain suffix ""Enum"""));
                return;
            }
            if (identifier.IndexOf("_") >= 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{SyntaxKind.EnumDeclaration} should not contain underscore"));
            }
        }

        private void AnalyzeTypeAndFunctionName(SyntaxNodeAnalysisContext context)
        {
            string identifier = String.Empty;
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
            if (String.IsNullOrWhiteSpace(identifier))
            {
                return;
            }
            if (!Char.IsUpper(identifier[0]))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{kind} should be in PascalCase"));
                return;
            }
            if (identifier.IndexOf("_") >= 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{kind} should not contain underscore"));
            }
        }

        private void AnalyzerLocalVariableName(SyntaxNodeAnalysisContext context)
        {
            var variable = context.Node as LocalDeclarationStatementSyntax;
            string identifier = variable.Declaration.Variables[0].Identifier.ToString();
            if (String.IsNullOrWhiteSpace(identifier))
            {
                return;
            }
            if (identifier[0] == '@')
            {
                if (identifier.Length < 2)
                {
                    return;
                }
                identifier = identifier.Remove(0, 1);
            }
            if (!identifier.All(Char.IsLetterOrDigit))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{SyntaxKind.LocalDeclarationStatement} should contain only letters or digits"));
                return;
            }
            if (!Char.IsLower(identifier[0]))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{SyntaxKind.LocalDeclarationStatement} should be in camelCase"));
                return;
            }
        }

        private void AnalyzeGlobaVariableName(SyntaxNodeAnalysisContext context)
        {
            var field = context.Node as FieldDeclarationSyntax;
            string identifier = field.Declaration.Variables[0].Identifier.ToString();
            if (String.IsNullOrWhiteSpace(identifier))
            {
                return;
            }
            if (!identifier.StartsWith("_"))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{SyntaxKind.FieldDeclaration} should start with underscore"));
                return;
            }
            if (identifier.Length > 1 && !Char.IsLower(identifier[1]))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{SyntaxKind.FieldDeclaration} should be in camelCase"));
                return;
            }
            if (identifier.LastIndexOf("_") > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), $"{SyntaxKind.FieldDeclaration} should not contain underscore"));
            }
        }
    }
}
