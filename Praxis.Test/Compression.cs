namespace Praxis.Test;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Comp = Praxis.IO.Compression;

[TestClass]
public class Compression {

	[TestMethod]
	public void RoundTrip() {

		string source = string.Join(Const.BROKENVERTBAR, Knowledge.Word.Random(10_000));
		var sourceBytes = source.ToUTF8Bytes();

		var compBytes = Comp.Compress(sourceBytes);

		Assert.IsLessThan(sourceBytes.Length, compBytes.Length);

		string decomp = Comp.Decompress(compBytes).ToUTF8String();

		Assert.AreEqual(source, decomp);
	}
}
