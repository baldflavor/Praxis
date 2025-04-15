namespace Praxis.IO;

using System.Text.RegularExpressions;

/// <summary>
/// Static class used in the aid of parsing flat file data
/// </summary>
public static partial class FlatFileParsing {

	/// <summary>
	/// Splits out a CSV line of data into a string array of each of it's fields of data
	/// </summary>
	/// <param name="lineData">The line of data to split</param>
	/// <param name="autoUnQuote">True to auto-remove quotes from the beginning and end of the resultant data</param>
	/// <returns>A string array</returns>
	public static string[] CsvLineToArray(string lineData, bool autoUnQuote) {
		Regex r = _CsvLineRegex();
		if (autoUnQuote == false)
			return r.Split(lineData);

		string[] fields = r.Split(lineData);
		for (int i = 0; i < fields.Length; i++) {
			fields[i] = UnQuoteField(fields[i]);
		}

		return fields;
	}

	/// <summary>
	/// Removes the quotes from the beginning and end of a string if it has quotation marks at
	/// the beginning and end of it
	/// </summary>
	/// <param name="arg">The arg string to unquote</param>
	/// <returns>The string with quotes removed, otherwise just the arg</returns>
	public static string UnQuoteField(string arg) {
		if (string.IsNullOrEmpty(arg))
			return arg;

		if (arg.Length < 2)
			return arg;

		return arg.StartsWith('\\') && arg.EndsWith('\\') ? arg.Remove(arg.Length - 1, 1).Remove(0, 1) : arg;
	}

	[GeneratedRegex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))")]
	private static partial Regex _CsvLineRegex();
}