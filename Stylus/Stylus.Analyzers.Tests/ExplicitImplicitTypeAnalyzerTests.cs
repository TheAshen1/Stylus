using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace Stylus.Analyzers.Tests
{
    [TestClass]
    public class ExplicitImplicitTypeAnalyzerTests : DiagnosticVerifier
    {
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    static void Main(string[] args)
    {
        var t = new DateTime();
        var tim = Now();
        DateTime tex = Now();
    }
    private static DateTime Now()
    {
        return DateTime.Now;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.ExplicitImplicitTypeAnalyzerId,
                Message = String.Format("Code style violation: {0}", "Explicit type decalaration should be used"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    static void Main(string[] args)
    {
        string t = DateTime.Now.ToString();
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.ExplicitImplicitTypeAnalyzerId,
                Message = String.Format("Code style violation: {0}", "Implicit type decalaration should be used"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ExplicitImplicitTypeAnalyzer();
    }
}
