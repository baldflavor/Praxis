namespace Praxis.Test;

using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Praxis.Knowledge;

[TestClass]
public class ExceptionExtension {

	[TestMethod]
	public void AddData_Object_ByNewInline() {
		int testNum = Random.Shared.Next();
		string testString = Word.Random();

		try {
			throw new Exception("Thrown for testing");
		}
		catch (Exception ex) {
			bool found = false;
			ex.AddData(new { testNum = -3, testString = "NADA" }).AddData(new { testNum, testString });

			foreach (DictionaryEntry de in ex.Data) {
				if (de.Value == null)
					continue;

				dynamic dVal = de.Value;
				if (dVal.testNum == testNum && dVal.testString == testString) {
					found = true;
					break;
				}
			}

			Assert.IsTrue(found);
		}
	}

	[TestMethod]
	public void AddData_Object_ByVariable() {
		var edObj = new { testNum = Random.Shared.Next(), testString = Word.Random() };

		try {
			throw new Exception("Thrown for testing");
		}
		catch (Exception ex) {
			bool found = false;
			ex.AddData(edObj);

			foreach (DictionaryEntry de in ex.Data) {
				if (de.Value == edObj) {
					found = true;
					break;
				}
			}

			Assert.IsTrue(found);
		}
	}

	[TestMethod]
	public void AddData_Primitive_ByVariable() {
		int testNum = Random.Shared.Next();

		try {
			throw new Exception("Thrown for testing");
		}
		catch (Exception ex) {
			bool found = false;
			ex.AddData(testNum).AddData(false);

			foreach (DictionaryEntry de in ex.Data) {
				if (de.Value?.Equals(testNum) == true) {
					found = true;
					break;
				}
			}

			Assert.IsTrue(found);
		}

		try {
			throw new Exception("Thrown for testing");
		}
		catch (Exception ex) {
			bool found = false;
			ex.AddData(testNum);

			foreach (DictionaryEntry de in ex.Data) {
				if (de.Value?.Equals(testNum) == true) {
					found = true;
					break;
				}
			}

			Assert.IsTrue(found);
		}
	}

	[TestMethod]
	public void AddData_Primitive_Inline() {
		int testNum = 1234567;

		try {
			throw new Exception("Thrown for testing");
		}
		catch (Exception ex) {
			bool found = false;
			ex.AddData(333).AddData(1234567);

			foreach (DictionaryEntry de in ex.Data) {
				if (de.Value?.Equals(testNum) == true) {
					found = true;
					break;
				}
			}

			Assert.IsTrue(found);
		}
	}
}