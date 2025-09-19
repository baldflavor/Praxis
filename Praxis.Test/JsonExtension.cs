namespace Praxis.Test;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class JsonExtension {

	/// <summary>
	/// Tests dates and times from object to json and back and verifies that they are equivalent
	/// </summary>
	/// <remarks>
	/// In general the recommendation is <b>DO NOT</b> use "DateTime.Now" (ever, lol) without stripping the kind -- as some libraries
	/// will then attempt either to local or to utc before or after, which can make reading the serialized data difficult and produce unexpected results
	/// </remarks>
	[TestMethod]
	public void DateTimeRoundTrip() {
		var objOg =
			new DateTimeRT(
				DateTime.Now,
				DateTimeOffset.Now,

				DateTime.UtcNow,
				DateTimeOffset.UtcNow,

				DateTime.Now.ToUnspecifiedKind(),
				DateTime.UtcNow.ToUnspecifiedKind());

		var joA = Json.Options;



		string json = objOg.ToJson();

		var objDes = json.Deserialize<DateTimeRT>();
		Assert.IsNotNull(objDes);
		Assert.AreEqual(objOg, objDes);
	}



	/// <summary>
	/// Class used for testing round trips of dates times and offsets
	/// </summary>
	/// <param name="Local">Local "now"</param>
	/// <param name="LocalOfs">Local "now"</param>
	/// <param name="Utc">Utc "now"</param>
	/// <param name="UtcOfs">Utc "now"</param>
	/// <param name="LocalUsk">Local "now" with unspecified kind</param>
	/// <param name="UtcUsk">Utc "now" with unspecified kind</param>
	private record class DateTimeRT(
		DateTime Local,
		DateTimeOffset LocalOfs,

		DateTime Utc,
		DateTimeOffset UtcOfs,

		DateTime LocalUsk,
		DateTime UtcUsk);
}
