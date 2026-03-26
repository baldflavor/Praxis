namespace Praxis;

public static partial class Extension {

	/// <summary>
	/// Formatting string for fixed digits using 12 hour format with AM/PM designation, does not include seconds.
	/// </summary>
	public const string FIXEDDIGIT12HOURNOSECONDSFORMAT = "yyyy/MM/dd hh:mm tt";

	/// <summary>
	/// Formatting string for fixed digits using 12 hour format with AM/PM designation, includes seconds.
	/// </summary>
	public const string FIXEDDIGIT12HOURSECONDSFORMAT = "yyyy/MM/dd hh:mm:ss tt";


	/// <summary>
	/// Format string for date and time suitable for use in a filename.
	/// </summary>
	public const string FILENAMEFORMAT = "yyyy-MM-dd_HH-mm-ss";

	/// <summary>
	/// Format string for date and time suitable for use in a filename including milliseconds.
	/// </summary>
	public const string FILENAMEFORMATMS = "yyyy-MM-dd_HH-mm-ss-fff";

	/// <summary>
	/// Returns the string representation of a date and time value suitable for use as part of a file name.
	/// </summary>
	/// <param name="dt">Chronological value to use as source of the string.</param>
	/// <param name="includeMilliseconds">Whether to incude milliseconds in the resultant string.</param>
	/// <returns>String suitable for use as part of a filename.</returns>
	public static string ToStringFilename(this DateTime dt, bool includeMilliseconds = false) => dt.ToString(includeMilliseconds ? FILENAMEFORMATMS : FILENAMEFORMAT);

	/// <summary>
	/// Returns the string representation of a date and time value suitable for use as part of a file name.
	/// </summary>
	/// <param name="dt">Chronological value to use as source of the string.</param>
	/// <param name="includeMilliseconds">Whether to incude milliseconds in the resultant string.</param>
	/// <returns>String suitable for use as part of a filename.</returns>
	public static string ToStringFilename(this DateTimeOffset dt, bool includeMilliseconds = false) => dt.ToString(includeMilliseconds ? FILENAMEFORMATMS : FILENAMEFORMAT);

	/// <summary>
	/// Gets a string representation using a constant, custom format.
	/// </summary>
	/// <remarks>
	/// Utilizes:
	/// <para><see cref="FIXEDDIGIT12HOURSECONDSFORMAT"/>, <see cref="FIXEDDIGIT12HOURNOSECONDSFORMAT"/></para>
	/// Example:
	/// <para><c>2025/04/03 08:01 AM</c></para>
	/// <para><c>2025/04/03 08:01:58 AM</c></para>
	/// </remarks>
	/// <param name="arg">Value to format</param>
	/// <param name="includeSeconds">Indicates whether or not to include seconds in the result string.</param>
	/// <returns><c>string</c></returns>
	public static string ToStringFixedDigit12Hour(this DateTime arg, bool includeSeconds) => arg.ToString(includeSeconds ? FIXEDDIGIT12HOURSECONDSFORMAT : FIXEDDIGIT12HOURNOSECONDSFORMAT);

	/// <summary>
	/// Gets a string representation using a constant, custom format.
	/// </summary>
	/// <remarks>
	/// Utilizes:
	/// <para><see cref="FIXEDDIGIT12HOURSECONDSFORMAT"/>, <see cref="FIXEDDIGIT12HOURNOSECONDSFORMAT"/></para>
	/// Example:
	/// <para><c>2025/04/03 08:01 AM</c></para>
	/// <para><c>2025/04/03 08:01:58 AM</c></para>
	/// </remarks>
	/// <param name="arg">Value to format</param>
	/// <param name="includeSeconds">Indicates whether or not to include seconds in the result string.</param>
	/// <returns><c>string</c></returns>
	public static string ToStringFixedDigit12Hour(this DateTimeOffset arg, bool includeSeconds) => arg.ToString(includeSeconds ? FIXEDDIGIT12HOURSECONDSFORMAT : FIXEDDIGIT12HOURNOSECONDSFORMAT);

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
