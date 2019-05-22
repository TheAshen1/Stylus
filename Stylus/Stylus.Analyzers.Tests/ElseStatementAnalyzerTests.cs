using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace Stylus.Analyzers.Tests
{
    [TestClass]
    public class ElseStatementAnalyzerTests : DiagnosticVerifier
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
                Id = StylusManifest.ElseStatementAnalyzerId,
                Message = String.Format("Else statement should be avoided"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ElseStatementAnalyzer();
    }
}
