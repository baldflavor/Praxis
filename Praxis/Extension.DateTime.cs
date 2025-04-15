namespace Praxis;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Formatting string for fixed digits using 12 hour format with AM/PM designation, does not include seconds
	/// </summary>
	private const string _FIXEDDIGIT12HOURNOSECONDSFORMAT = "yyyy/MM/dd hh:mm tt";


	/// <summary>
	/// Gets a string representation using the format in <see cref="_FIXEDDIGIT12HOURNOSECONDSFORMAT"/>
	/// </summary>
	/// <remarks>
	/// Example:
	/// <c>2025/04/03 08:01 AM</c>
	/// </remarks>
	/// <param name="arg"><see cref="DateTime"/></param>
	/// <returns><see cref="string"/></returns>
	public static string ToFixedDigit12HourNoSeconds(this DateTime arg) => arg.ToString(_FIXEDDIGIT12HOURNOSECONDSFORMAT);

	/// <summary>
	/// Gets a string representation using the format in <see cref="_FIXEDDIGIT12HOURNOSECONDSFORMAT"/>
	/// </summary>
	/// <remarks>
	/// Example:
	/// <c>2025/04/03 08:01 AM</c>
	/// </remarks>
	/// <param name="arg"><see cref="DateTime"/></param>
	/// <returns><see cref="string"/></returns>
	public static string ToFixedDigit12HourNoSeconds(this DateTimeOffset arg) => arg.ToString(_FIXEDDIGIT12HOURNOSECONDSFORMAT);

	/// <summary>
	/// Gets the round-trip (ISO 8601) compliant string representation of <paramref name="arg"/>
	/// </summary>
	/// <remarks>Parsing this will set DateTime.Kind properly</remarks>
	/// <param name="arg"><see cref="DateTimeOffset"/></param>
	/// <returns><see cref="string"/></returns>
	public static string ToStringRoundTrip(this DateTimeOffset arg) => arg.ToString("O", System.Globalization.CultureInfo.InvariantCulture);

	/// <summary>
	/// Returns a <see cref="DateTime"/> with a <see cref="DateTime.Kind"/> set to <see cref="DateTimeKind.Unspecified"/> if it is not
	/// already <see cref="DateTimeKind.Unspecified"/>
	/// </summary>
	/// <remarks>
	/// The resulting value of the time/ticks will be the same, this simply removes the mask that holds the kind behind the scenes which
	/// can cause serialization/deserialization issues depending.
	/// </remarks>
	/// <param name="dateTime">Value to target</param>
	/// <returns><see cref="DateTime"/></returns>
	public static DateTime ToUnspecifiedKind(this DateTime dateTime) {
		if (dateTime.Kind != DateTimeKind.Unspecified)
			return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
		else
			return dateTime;
	}
}