namespace Praxis.IO;

using System.Text.RegularExpressions;

/// <summary>
/// Static class used in the aid of parsing flat file data
/// </summary>
public static partial class FlatFileParsing {

	/// <summary>
	/// Splits a CSV line of data into string fields of data.
	/// </summary>
	/// <param name="line">The line of data to split</param>
	/// <param name="autoUnQuote">True to remove leading and trailing quotes from each resultant field.</param>
	/// <returns><c>string[]</c></returns>
	public static string[] CsvLineToArray(string line, bool autoUnQuote) {
		Regex r = _CsvLineRegex();

		string[] fields = r.Split(line);
		if (autoUnQuote == false) {
			for (int i = 0; i < fields.Length; i++) {
				fields[i] = UnQuote(fields[i]);
			}
		}

		return fields;
	}

	/// <summary>
	/// Removes <c>"</c> from the beginning and end of a string when it leads and trails with <c>"</c>.
	/// </summary>
	/// <param name="arg">Target string</param>
	/// <returns><c>string</c></returns>
	public static string UnQuote(string arg) {
		if (arg.StartsWith('"') && arg.EndsWith('"')) {
			return new string(arg.AsSpan()[1..][..^1]);
		}
		else {
			return arg;
		}
	}

	[GeneratedRegex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))")]
	private static partial Regex _CsvLineRegex();
}
