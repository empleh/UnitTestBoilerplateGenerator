using System;
using System.Text;
using UnitTestBoilerplate.Model;

namespace UnitTestBoilerplate
{
	public class DefaultTemplateGenerator
	{
		private int indentLevel;

		private StringBuilder template;

		public string Get(TestFramework testFramework, MockFramework mockFramework)
		{
			this.indentLevel = 0;

			this.template = new StringBuilder();

			AddUsingStatements();
			AddNamespace();


			AddTestClassAttribute(testFramework);
			CreateTestClass(testFramework, mockFramework);

			AddTestClassStartCode(mockFramework);
			AddTestClassMockFields(mockFramework);
			AddTestInitialize(testFramework, mockFramework);

			AddTestCleanup(testFramework, mockFramework);

			AddTestMethod(testFramework, mockFramework);

			AddCreationHelperMethod(mockFramework);

			AddEndOfClassAndNamespace();

			return this.template.ToString();
		}

		private void AddTestInitialize(TestFramework testFramework, MockFramework mockFramework)
		{
			// Test initialize
			switch (testFramework.TestInitializeStyle)
			{
				case TestInitializeStyle.Constructor:
					this.AppendLineIndented("public $ClassName$Tests()");

					break;
				case TestInitializeStyle.AttributedMethod:
					this.AppendLineIndented($"[{testFramework.TestInitializeAttribute}]");
					this.AppendLineIndented($"public void {testFramework.TestInitializeAttribute}()");

					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(testFramework));
			}

			this.AppendLineIndented("{");
			this.indentLevel++;

			this.AppendLineIndented("$MockFieldInitializations$");

			AddInitializeStartCode(mockFramework);

			this.indentLevel--;
			this.AppendLineIndented("}");
			this.AppendLineIndented();
		}

		private void AddInitializeStartCode(MockFramework mockFramework)
		{
			if (string.IsNullOrEmpty(mockFramework.InitializeStartCode))
			{
				return;
			}

			this.AppendLineIndented();
			this.AppendLineIndented(mockFramework.InitializeStartCode);

		}

		private void AddCreationHelperMethod(MockFramework mockFramework)
		{
			if (mockFramework.TestedObjectCreationStyle != TestedObjectCreationStyle.HelperMethod)
			{
				return;
			}

			this.AppendLineIndented();
			this.AppendLineIndented("private $ClassName$ Create$ClassNameShort$()");
			this.AppendLineIndented("{");
			this.indentLevel++;
			this.AppendLineIndented("return $ExplicitConstructor$;");
			this.indentLevel--;
			this.AppendLineIndented("}");
		}

		private void AddTestMethod(TestFramework testFramework, MockFramework mockFramework)
		{
			this.AppendLineIndented($"[{testFramework.TestMethodAttribute}]");
			this.AppendLineIndented("public void TestMethod1()");
			this.AppendLineIndented("{");
			this.indentLevel++;

			this.AppendLineIndented("// Arrange");
			if (!string.IsNullOrEmpty(mockFramework.TestArrangeCode))
			{
				this.AppendLineIndented(mockFramework.TestArrangeCode);
			}

			this.AppendLineIndented(); // Blank line for users to put in their own arrange code
			this.AppendLineIndented(); // Separator

			this.AppendLineIndented("// Act");
			AddActCode(mockFramework);

			this.AppendLineIndented(); // Blank line for users to put in their own act code
			this.AppendLineIndented(); // Separator

			this.AppendLineIndented("// Assert");
			this.AppendLineIndented(); // Blank line for users to put in their own assert code

			this.indentLevel--;
			this.AppendLineIndented("}");
		}

		private void AddActCode(MockFramework mockFramework)
		{
			if (string.IsNullOrEmpty(mockFramework.TestedObjectCreationCode) == false)
			{
				this.AppendLineIndented(mockFramework.TestedObjectCreationCode);
				return;
			}

			switch (mockFramework.TestedObjectCreationStyle)
			{
				case TestedObjectCreationStyle.HelperMethod:
					this.AppendLineIndented("$ClassName$ $ClassNameShort.CamelCase$ = this.Create$ClassNameShort$();");

					break;
				case TestedObjectCreationStyle.DirectCode:
					this.AppendLineIndented(mockFramework.TestedObjectCreationCode);

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void AddEndOfClassAndNamespace()
		{
			this.indentLevel--;
			this.AppendLineIndented("}");
			this.indentLevel--;
			this.AppendLineIndented("}");
		}

		private void AddTestCleanup(TestFramework testFramework, MockFramework mockFramework)
		{
			if (mockFramework.HasTestCleanup == false)
			{
				return;
			}

			switch (testFramework.TestCleanupStyle)
			{
				case TestCleanupStyle.Disposable:
					this.AppendLineIndented("public void Dispose()");

					break;
				case TestCleanupStyle.AttributedMethod:
					this.AppendLineIndented($"[{testFramework.TestCleanupAttribute}]");
					this.AppendLineIndented($"public void {testFramework.TestCleanupAttribute}()");

					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(testFramework));
			}

			this.AppendLineIndented("{");
			this.indentLevel++;

			this.AppendLineIndented(mockFramework.TestCleanupCode);

			this.indentLevel--;
			this.AppendLineIndented("}");
			this.AppendLineIndented();

		}

		private void AddTestClassMockFields(MockFramework mockFramework)
		{
			if (mockFramework.HasMockFields == false)
			{
				return;
			}

			this.AppendLineIndented("$MockFieldDeclarations$");
			this.AppendLineIndented();
		}

		private void AddTestClassStartCode(MockFramework mockFramework)
		{
			if (string.IsNullOrEmpty(mockFramework.ClassStartCode))
			{
				return;
			}

			this.AppendLineIndented(mockFramework.ClassStartCode);
			this.AppendLineIndented();

		}

		private void CreateTestClass(TestFramework testFramework, MockFramework mockFramework)
		{
			this.AppendIndent();
			this.template.Append("public class $ClassName$Tests");
			if (mockFramework.HasTestCleanup && testFramework.TestCleanupStyle == TestCleanupStyle.Disposable)
			{
				this.template.Append(" : IDisposable");
			}

			this.template.AppendLine();
			this.AppendLineIndented("{");
			this.indentLevel++;
		}

		private void AddTestClassAttribute(TestFramework testFramework)
		{
			if (!string.IsNullOrEmpty(testFramework.TestClassAttribute))
			{
				this.AppendLineIndented($"[{testFramework.TestClassAttribute}]");
			}
		}

		private void AddNamespace()
		{
			this.AppendLineIndented("namespace $Namespace$");
			this.AppendLineIndented("{");
			this.indentLevel++;
		}

		private void AddUsingStatements()
		{
			this.AppendLineIndented("$UsingStatements$");
			this.AppendLineIndented();
		}

		public void AppendIndent()
		{
			for (int i = 0; i < this.indentLevel; i++)
			{
				this.template.Append('\t');
			}
		}

		public void AppendLineIndented(string line)
		{
			this.AppendIndent();
			this.template.AppendLine(line);
		}

		public void AppendLineIndented()
		{
			this.AppendIndent();
			this.template.AppendLine(string.Empty);
		}
	}
}
