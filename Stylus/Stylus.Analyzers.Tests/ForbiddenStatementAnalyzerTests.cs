using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace Stylus.Analyzers.Tests
{
    [TestClass]
    public class ForbiddenStatementAnalyzerTests : DiagnosticVerifier
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
        var a = true;
        if(a)
        {
            Console.WriteLine(""Ok"");
        }
        else
        {
            Console.WriteLine(""Not Ok"");
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.ForbiddenStatementAnalyzerId,
                Message = String.Format("{0} statement should be avoided", "Else"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 9)
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
        do
        {
            Console.WriteLine(""do"");
        }
        while (true);
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.ForbiddenStatementAnalyzerId,
                Message = String.Format("{0} statement should be avoided", "Do-While"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ForbiddenStatementAnalyzer();
    }
}
