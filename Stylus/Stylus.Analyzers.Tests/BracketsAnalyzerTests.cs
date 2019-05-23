using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace Stylus.Analyzers.Tests
{
    [TestClass]
    public class BracketsAnalyzerTests : DiagnosticVerifier
    {
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (true)
            {
                Console.WriteLine(""Ok"");
            }
        }
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
    internal class Program
    {
        static void Main(string[] args)
        {
            if (true)
                Console.WriteLine(""Wrong"");
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.BracketsAnalyzerId,
                Message = String.Format("{0} should {1}", "Braces", "be used"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 13)
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
    internal class Program
    {
        static void Main(string[] args)
        {
            if (true) { Console.WriteLine(""Wrong""); }
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.BracketsAnalyzerId,
                Message = String.Format("{0} should {1}", "Braces", "be placed on their own lines"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new BracketsAnalyzer();
    }

}
