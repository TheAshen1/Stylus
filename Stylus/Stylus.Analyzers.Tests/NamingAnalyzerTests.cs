using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        public void TestMethod01()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void TestMethod02()
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
                Message = String.Format("Naming rule violation: {0}", $@"{SyntaxKind.EnumDeclaration} should not contain suffix ""Enum"""),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestMethod03()
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
                Message = String.Format("Naming rule violation: {0}", $"{SyntaxKind.EnumDeclaration} should be in PascalCase"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestMethod04()
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
                Message = String.Format("Naming rule violation: {0}", $"{SyntaxKind.ClassDeclaration} should be in PascalCase"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestMethod05()
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
                Message = String.Format("Naming rule violation: {0}", $"{SyntaxKind.FieldDeclaration} should start with underscore"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestMethod06()
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
                Message = String.Format("Naming rule violation: {0}", $"{SyntaxKind.FieldDeclaration} should be in camelCase"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestMethod07()
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
                Message = String.Format("Naming rule violation: {0}", $"{SyntaxKind.LocalDeclarationStatement} should be in camelCase"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }


        [TestMethod]
        public void TestMethod08()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    class Test_Class
    {
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.NamingAnalyzerId,
                Message = String.Format("Naming rule violation: {0}", $"{SyntaxKind.ClassDeclaration} should not contain underscore"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestMethod09()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    internal class Test
    {
        public string _test_str;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.NamingAnalyzerId,
                Message = String.Format("Naming rule violation: {0}", $"{SyntaxKind.FieldDeclaration} should not contain underscore"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestMethod10()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    internal class Test
    {
        static void Main(string[] args)
        {
            var local_var = 5;
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.NamingAnalyzerId,
                Message = String.Format("Naming rule violation: {0}", $"{SyntaxKind.LocalDeclarationStatement} should contain only letters or digits"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 10, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void TestMethod11()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    internal interface Test
    {
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.NamingAnalyzerId,
                Message = String.Format("Naming rule violation: {0}", $"{SyntaxKind.InterfaceDeclaration} should start with I"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }
        
        [TestMethod]
        public void TestMethod12()
        {
            var test = @"
using System;

namespace AnalyzerTest
{
    internal interface Itest
    {
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.NamingAnalyzerId,
                Message = String.Format("Naming rule violation: {0}", $"{SyntaxKind.InterfaceDeclaration} should be in PascalCase"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new NamingAnalyzer();
    }
}
