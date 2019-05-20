using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Stylus
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassMemberOrderAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "StylusClassMemberOrder";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Class member order";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(CheckClassMemberOrder, SyntaxKind.ClassDeclaration);
        }

        private void CheckClassMemberOrder(SyntaxNodeAnalysisContext context)
        {
            var @class = context.Node as ClassDeclarationSyntax;
            var members = @class.ChildNodes().ToList();
            for (int i = 0; (i + 1) < members.Count; i++)
            {
                for (int j = (i + 1); j < members.Count; j++)
                {
                    var kindA = members[i].Kind();
                    var kindB = members[j].Kind();
                    Util.TryGetAccessModifier(members[i], out Accessibility modifierA);
                    Util.TryGetAccessModifier(members[j],out Accessibility modifierB);
                    var keyA = Util._memberOrder.Where(p => p.Value == (kindA, modifierA)).Select(p => p.Key).FirstOrDefault();
                    var keyB = Util._memberOrder.Where(p => p.Value == (kindB, modifierB)).Select(p => p.Key).FirstOrDefault();
                    if (keyB - keyA < 0)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, @class.GetLocation(), @class.Identifier.ToString()));
                        return;
                    }
                }
            }
        }
    }
}
