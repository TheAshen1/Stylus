using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace Stylus.Analyzers.Tests
{
    [TestClass]
    public class NamingAnalyzerTests : DiagnosticVerifier
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
    enum TestEnum
    {
        A,
        B
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.NamingAnalyzerId,
                Message = String.Format("{0} must {1}", "Enums", @"not containt suffix ""Enum"""),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
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
    enum test
    {
        A,
        B
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.NamingAnalyzerId,
                Message = String.Format("{0} must {1}", "Enums", "be in PascalCase"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        
        [TestMethod]
        public void TestMethod4()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    class test
    {
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.NamingAnalyzerId,
                Message = String.Format("{0} must {1}", "ClassDeclaration", "be in PascalCase"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
         
        [TestMethod]
        public void TestMethod5()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    class Test
    {
        public string str;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.NamingAnalyzerId,
                Message = String.Format("{0} must {1}", "Fields", "start with underscore"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
          
        [TestMethod]
        public void TestMethod6()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    class Test
    {
        public string _Str;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.NamingAnalyzerId,
                Message = String.Format("{0} must {1}", "Fields", "be in camelCase"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
         
        [TestMethod]
        public void TestMethod7()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    class Test
    {
        static void Main(string[] args)
        {
            var Local = 5;
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.NamingAnalyzerId,
                Message = String.Format("{0} must {1}", "Local variables", "be in camelCase"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new NamingAnalyzer();
    }
}
