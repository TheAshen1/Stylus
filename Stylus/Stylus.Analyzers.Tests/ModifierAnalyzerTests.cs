using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace Stylus.Analyzers.Tests
{
    [TestClass]
    public class ModifierAnalyzerTests : DiagnosticVerifier
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
    class Program
    {
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.ModifierAnalyzerId,
                Message = String.Format("Access modifiers should always be specified"),
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
    internal class Program
    {
        string _str;
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.ModifierAnalyzerId,
                Message = String.Format("Access modifiers should always be specified"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 9)
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
    internal class Program
    {
        static void Main(string[] args)
        {
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.ModifierAnalyzerId,
                Message = String.Format("Access modifiers should always be specified"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 8, 9)
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
    interface Program
    {       
    }
}";
            var expected = new DiagnosticResult
            {
                Id = StylusManifest.ModifierAnalyzerId,
                Message = String.Format("Access modifiers should always be specified"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 6, 5)
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
    public interface Program
    {
        void DoStuff();
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ModifierAnalyzer();
    }
}
