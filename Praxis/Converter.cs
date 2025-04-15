namespace Praxis;

/// <summary>
/// Class used for converting data to / from disparate types
/// </summary>
public static class Converter {

	/// <summary>
	/// Attempts to parse the arg string into a date time offset.
	/// </summary>
	/// <param name="arg">The arg string to parse</param>
	/// <returns>The parsed DateTimeOffset or null</returns>
	public static DateTimeOffset? ToDateTimeOffset(string arg) {
		if (DateTimeOffset.TryParse(arg, out DateTimeOffset parsed))
			return parsed;
		else
			return null;
	}

	/// <summary>
	/// Attempts to parse the passed string out from arg
	/// <para>You can send in empty or null strings</para>
	/// </summary>
	/// <param name="arg">The string to parse</param>
	/// <returns>The parsed decimal; null if conversion fails</returns>
	public static decimal? ToDecimal(string arg) {
		if (string.IsNullOrEmpty(arg))
			return null;

		if (decimal.TryParse(arg, out decimal parsed))
			return parsed;
		else
			return null;
	}

	/// <summary>
	/// Attempts to parse the passed string out from arg
	/// <para>You can send in empty or null strings</para>
	/// </summary>
	/// <param name="arg">The string to parse</param>
	/// <returns>The parsed double; null if conversion fails</returns>
	public static double? ToDouble(string arg) {
		if (string.IsNullOrEmpty(arg))
			return null;

		if (double.TryParse(arg, out double parsed))
			return parsed;
		else
			return null;
	}

	/// <summary>
	/// Attempts to parse the passed string into a guid - if this is not successful for any reason null is returned
	/// </summary>
	/// <param name="arg">Target string to parse</param>
	/// <returns>A guid; null on failure to parse</returns>
	public static Guid? ToGuid(string arg) {
		if (!Guid.TryParse(arg, out Guid result))
			return null;
		else
			return result;
	}

	/// <summary>
	/// Attempts to parse the passed string out from arg
	/// <para>You can send in empty or null strings</para>
	/// </summary>
	/// <param name="arg">The string to parse</param>
	/// <returns>The parsed int; null if conversion fails</returns>
	public static int? ToInt(string arg) {
		if (string.IsNullOrEmpty(arg))
			return null;

		if (int.TryParse(arg, out int parsed))
			return parsed;
		else
			return null;
	}
}