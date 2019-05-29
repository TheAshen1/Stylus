using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace Stylus.Analyzers.Tests
{
    [TestClass]
    public class EnumMemberAnalyzerTests : DiagnosticVerifier
    {
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    public enum Type
    {
        A = 0
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    public enum Type
    {
        A
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.EnumMemberAnalyzerId,
                Message = String.Format("Code style violation: {0}", "Enum members should have explicit value"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new EnumMemberAnalyzer();
    }
}
