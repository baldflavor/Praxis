namespace Praxis.Test;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PAssert = Praxis.Assert;


[TestClass]
public class Assertion {

	[TestMethod]
	public void Affirm() {
		var f = new Food() { Ounces = 4 };
		var f2 = PAssert.Affirm(f, f => {
			if (f.Ounces > 4)
				throw new Exception("Not in range");
		});

		var ounces = f2.Ounces;

		var flavor = PAssert.Affirm(f.Flavor, _FlavorCheck);

		f.Flavor = "rich";
		flavor = PAssert.Affirm(f.Flavor, _FlavorCheck);

		f.Flavor = "spicy";
		try {
			flavor = PAssert.Affirm(f.Flavor, _FlavorCheck);
		}
		catch (Exception ex) {
			ex.AddData(f.Flavor);
		}


		static bool _FlavorCheck(string? arg) {
			return arg is null || arg.ContainsOIC("rich");
		}
	}


	private class Food {
		public string? Flavor { get; set; }

		public int Ounces { get; set; }
	}
}
