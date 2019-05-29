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
    public class EnumMemberAnalyzer : DiagnosticAnalyzer
    {
        public const string _diagnosticId = StylusManifest.EnumMemberAnalyzerId;
        internal static readonly LocalizableString _title = "Enum member analyzer";
        internal static readonly LocalizableString _messageFormat = "Code style violation: {0}";
        internal const string _category = StylusManifest.Category;

        internal static DiagnosticDescriptor _rule = new DiagnosticDescriptor(_diagnosticId, _title, _messageFormat, _category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeEnumMember, SyntaxKind.EnumMemberDeclaration);
        }

        private void AnalyzeEnumMember(SyntaxNodeAnalysisContext context)
        {
            var member = context.Node as EnumMemberDeclarationSyntax;
            if (member.EqualsValue is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(_rule, context.Node.GetLocation(), "Enum members should have explicit value"));
            }
        }
    }
}
