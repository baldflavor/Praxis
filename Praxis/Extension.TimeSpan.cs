namespace Praxis;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {
	/// <summary>
	/// Constant string format for a negative <see cref="TimeSpan"/> that includes the number of days
	/// </summary>
	public const string TIMESPAN_FORMAT_DAYNEGATIVE = "\\-dd\\:hh\\:mm\\:ss\\.ff";

	/// <summary>
	/// Constant string format for a positive <see cref="TimeSpan"/> that includes the number of days
	/// </summary>
	public const string TIMESPAN_FORMAT_DAYPOSITIVE = "dd\\:hh\\:mm\\:ss\\.ff";

	/// <summary>
	/// Constant string format for a negative <see cref="TimeSpan"/>
	/// </summary>
	public const string TIMESPAN_FORMAT_NEGATIVE = "\\-hh\\:mm\\:ss\\.ff";

	/// <summary>
	/// Constant string format for a positive <see cref="TimeSpan"/>
	/// </summary>
	public const string TIMESPAN_FORMAT_POSITIVE = "hh\\:mm\\:ss\\.ff";

	/// <summary>
	/// Returns a timespan formatted with two digits for: hours, minutes, seconds, milliseconds
	/// <para>A leading `<c>-</c>` is added if the <see cref="TimeSpan"/> is less than <see cref="TimeSpan.Zero"/></para>
	/// </summary>
	/// <remarks>
	/// Negative two days, twelve hours, forty five minutes, ten seconds, thirty milliseconds would be formatted as:
	/// <code>-12:45:10.03</code>
	/// </remarks>
	/// <param name="arg">TimeSpan to display as a string</param>
	/// <returns>A string</returns>
	public static string ToStringFixed(this TimeSpan arg) => arg.ToString(arg < TimeSpan.Zero ? TIMESPAN_FORMAT_NEGATIVE : TIMESPAN_FORMAT_POSITIVE);

	/// <summary>
	/// Returns a timespan formatted with two digits for days and two digits for: hours, minutes, seconds -- and three digits for milliseconds
	/// <para>A leading `<c>-</c>` is added if the <see cref="TimeSpan"/> is less than <see cref="TimeSpan.Zero"/></para>
	/// </summary>
	/// <remarks>
	/// Negative two days, twelve hours, forty five minutes, ten seconds, thirty milliseconds would be formatted as:
	/// <code>-02:12:45:10.03</code>
	/// </remarks>
	/// <param name="arg">TimeSpan to display as a string</param>
	/// <returns>A string</returns>
	public static string ToStringFixedDays(this TimeSpan arg) => arg.ToString(arg < TimeSpan.Zero ? TIMESPAN_FORMAT_DAYNEGATIVE : TIMESPAN_FORMAT_DAYPOSITIVE);

	/// <summary>
	/// Returns a timespan formatted with (minimum) two digits for: minutes, and two digits for seconds and milliseconds
	/// <para>A leading `<c>-</c>` is added if the <see cref="TimeSpan"/> is less than <see cref="TimeSpan.Zero"/></para>
	/// </summary>
	/// <remarks>
	/// Negative two days, twelve hours, forty five minutes, ten seconds, thirty milliseconds would be formatted as:
	/// <code>-3645:10.03</code>
	/// </remarks>
	/// <param name="arg">TimeSpan to display as a string</param>
	/// <returns>A string</returns>
	public static string ToStringFixedMinutes(this TimeSpan arg) {
		const string PFORMAT = "{0:D2}:{1:D2}.{2:D2}";
		const string NFORMAT = "-" + PFORMAT;

		bool isNegative = arg.Ticks < 0;
		if (isNegative)
			arg = arg.Negate();

		return string.Format(
				isNegative ? NFORMAT : PFORMAT,
				(int)arg.TotalMinutes,
				arg.Seconds,
				arg.Milliseconds / 10);
	}
}