using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using TestHelper;

namespace Stylus.Analyzers.Tests
{
    [TestClass]
    public class StringLiteralAnalyzerTests : DiagnosticVerifier
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
    public class Program
    {
        static void Main(string[] args)
        {
            var str = """";
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.StringLiteralAnalyzerId,
                Message = String.Format("Code style violation: {0}", "String.Empty should be used instead"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 21)
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
    public class Program
    {
        static void Main(string[] args)
        {
            var str = ""abc"";
            if (str == """")
            {
                Console.WriteLine(""string is empty"");
            }
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.StringLiteralAnalyzerId,
                Message = String.Format("Code style violation: {0}", "String.IsNullOrEmpty should be used instead"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 17)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new StringLiteralAnalyzer();
    }
}
