using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Runtime.CompilerServices;
using TestHelper;

namespace Mntone.NotifyGenerator.Test
{
	[TestClass]
	public sealed class UnitTest : CodeFixVerifier
	{
		private static readonly string BaseDir = @"..\..\TestSamples\";
		private static readonly string TestBaseText;
		private static readonly string FixTestBaseText;

		static UnitTest()
		{
			TestBaseText = LoadFile(BaseDir + "TestBase.txt");
			FixTestBaseText = LoadFile(BaseDir + "FixTestBase.txt");
		}


		[TestMethod]
		public void EmptyTest()
		{
			var test = string.Empty;
			this.VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void SimpleOnePropertyTest() => this.Test();

		[TestMethod]
		public void SimpleTwoPropertiesTest() => this.Test();

		[TestMethod]
		public void SimpleWithUngenerateTest() => this.Test();

		[TestMethod]
		public void NoPrivateTest() => this.Test();

		[TestMethod]
		public void NoThisTest() => this.Test();

		[TestMethod]
		public void FieldVisibleTest() => this.Test();

		[TestMethod]
		public void NoThreadSafeTest() => this.Test();

		[TestMethod]
		public void NoThreadSafeAndNoThisTest() => this.Test();

		[TestMethod]
		public void CompareMethodNoneTest() => this.Test();

		[TestMethod]
		public void CompareMethodEqualsTest() => this.Test();

		[TestMethod]
		public void CompareMethodReferenceEqualsTest() => this.Test();

		[TestMethod]
		public void NameConversionMethodLeadingUnderscoreTest() => this.Test();

		[TestMethod]
		public void NameConversionMethodLeadingLowerCaseLettersTest() => this.Test();

		[TestMethod]
		public void NameConversionMethodHungarianTest() => this.Test();

		[TestMethod]
		public void NameConversionMethodHungarianLikeCPlusPlusTest() => this.Test();

		[TestMethod]
		public void AddNewPropertyTest() => this.Test();

		private void Test([CallerMemberName] string testName = "")
		{
			if (string.IsNullOrEmpty(testName)) return;

			var fileName = testName.Substring(0, testName.Length - 4);
			var test = LoadFile(BaseDir + fileName + "Test.txt");
			var fixtest = LoadFile(BaseDir + fileName + "FixTest.txt");
			this.Test(test, fixtest);
		}

		private void Test(string test, string fixtest, DiagnosticResult? expected = null)
		{
			test = TestBaseText + test;
			fixtest = FixTestBaseText + fixtest;

			if (expected == null)
			{
				expected = new DiagnosticResult
				{
					Id = NotifyGeneratorDiagnosticAnalyzer.DiagnosticId,
					Message = Resources.AnalyzerMessage,
					Severity = DiagnosticSeverity.Info,
					Locations = new[] { new DiagnosticResultLocation("Test0.cs", 29, 5) }
				};
			}

			this.VerifyCSharpDiagnostic(test, expected.Value);
			this.VerifyCSharpFix(test, fixtest);
		}

		private static string LoadFile(string filename)
		{
			using (var fs = new FileStream(filename, FileMode.Open))
			using (var reader = new StreamReader(fs))
			{
				return reader.ReadToEnd();
			}
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new NotifyGeneratorCodeFixProvider();
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new NotifyGeneratorDiagnosticAnalyzer();
	}
}