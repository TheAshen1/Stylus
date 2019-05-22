using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stylus.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EnumNameAnalyzer : DiagnosticAnalyzer
    {
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.StylusEnumNameAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.StylusEnumNameAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.StylusEnumNameAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            StylusManifest.EnumNameAnalyzerId,
            Title,
            MessageFormat,
            StylusManifest.Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(CheckEnumName, SyntaxKind.EnumDeclaration);
        }

        private void CheckEnumName(SyntaxNodeAnalysisContext context)
        {
            var @enum = context.Node as EnumDeclarationSyntax;
            var forbiddenSuffix = "Enum";
            var enumName = @enum.Identifier.ToString();
            if (enumName.IndexOf(forbiddenSuffix, StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), enumName, forbiddenSuffix));
            }
        }
    }
}
