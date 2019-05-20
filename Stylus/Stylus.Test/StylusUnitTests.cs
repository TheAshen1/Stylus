using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace Stylus.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [TestMethod]
        public void TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestMethod2()
        {
            var test = @"
    using System;

    namespace AnalyzerTest
    {
        class Program
        {
            private string privateString;
            public string publicString;
            protected string protectedString;
            static void Main(string[] args)
            {
                int i = 5;
                int j = 3;
                int k = i + j;
            }
            protected void ProtectedMethod()
            {
                Console.WriteLine(protectedString);
            }
            public void PublicMethod()
            {
                Console.WriteLine(publicString);
            }
            private void PrivateMethod()
            {
                Console.WriteLine(privateString);
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "Stylus",
                Message = String.Format("Class '{0}' members could be reordered", "Program"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;

    namespace AnalyzerTest
    {
        class Program
        {
            static void Main(string[] args)
            {
                int i = 5;
                int j = 3;
                int k = i + j;
            }
            private string privateString;
            protected string protectedString;
            public string publicString;
            public void PublicMethod()
            {
                Console.WriteLine(publicString);
            }
            protected void ProtectedMethod()
            {
                Console.WriteLine(protectedString);
            }
            private void PrivateMethod()
            {
                Console.WriteLine(privateString);
            }
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new ClassMemberOrderFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ClassMemberOrderAnalyzer();
        }
    }
}
