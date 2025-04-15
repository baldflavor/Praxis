namespace Praxis;

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {
	/// <summary>
	/// Default delimter to use between each item in <see cref="ToStringSummary(IEnumerable{ValidationResult}, string?, string?, string?, bool)"/>
	/// </summary>
	public static string JoinDelimiter { get; set; } = Const.BROKENVERTBARSTRING;

	/// <summary>
	/// Inner delimiter (for full property) for each item in <see cref="ToStringEach(IEnumerable, bool, string?, string?, string?)"/>, <see cref="ToStringEach(IDictionary, bool, string?, string?, string?)"/>, <see cref="ToStringEach(NameValueCollection, string?, string?)"/>
	/// </summary>
	public static string ToStringsDelimiterInner { get; set; } = Const.BROKENVERTBARSTRING;

	/// <summary>
	/// Format to use for <see cref="ToStringEach(IEnumerable, bool, string?, string?, string?)"/>, <see cref="ToStringEach(IDictionary, bool, string?, string?, string?)"/>, <see cref="ToStringEach(NameValueCollection, string?, string?)"/>
	/// </summary>
	public static string ToStringsFormat { get; set; } = "{0}" + Const.RIGHTARROWHEAD + "{1}";

	/// <summary>
	/// Determines whether two lists are equal by comparing reference equality, then count, then value comparison
	/// </summary>
	/// <typeparam name="T">Type of elements contained in each list</typeparam>
	/// <param name="first">First list</param>
	/// <param name="second">Second list</param>
	/// <returns><see langword="true"/> if the lists are equal, otherwise false</returns>
	public static bool IsEqualTo<T>(this IReadOnlyList<T>? first, IReadOnlyList<T>? second) {
		if (ReferenceEquals(first, second))
			return true;

		if (first == null || second == null)
			return false;

		if (first.Count != second.Count)
			return false;

		for (int i = 0; i < first.Count; i++) {
			if (!EqualityComparer<T>.Default.Equals(first[i], second[i]))
				return false;
		}

		return true;
	}

	/// <summary>
	/// Returns a value that indicates if the passed <paramref name="arg"/> has any elements.
	/// <para>If the passed argument is null then false is returned</para>
	/// <para>If the passed argument has a <see cref="ICollection.Count"/> greater than <c>zero</c> true is returned</para>
	/// </summary>
	/// <param name="arg">Collection argument to check for existence of elements</param>
	/// <returns>True if <paramref name="arg"/> has elements, otherwise false</returns>
	public static bool HasElements(this ICollection? arg) => arg != null && arg.Count > 0;

	/// <summary>
	/// Joins a string array into a single string using an optional delimiter
	/// </summary>
	/// <param name="arg">Array to join together</param>
	/// <param name="delimiter">Optional delimiter. If <see langword="null"/> then <see cref="JoinDelimiter"/> is used</param>
	/// <returns><see cref="string"/></returns>
	public static string Join(this IEnumerable<string> arg, string? delimiter = null) => string.Join(delimiter ?? JoinDelimiter, arg);

	/// <summary>
	/// Gets the keys and values and projects them into a <see cref="KeyValuePair{TKey, TValue}"/> of <see cref="string"/>/<see cref="string"/>
	/// </summary>
	/// <param name="arg">Collection used as source data</param>
	/// <returns><see cref="KeyValuePair{TKey, TValue}"/> of <see cref="string"/>/<see cref="string"/></returns>
	public static List<KeyValuePair<string, string?>> KeysValues(this NameValueCollection arg) {
		List<KeyValuePair<string, string?>> toReturn = [];

		foreach (string? key in arg.AllKeys) {
			if (key == null)
				continue;

			toReturn.Add(KeyValuePair.Create(key, arg[key]));
		}

		return toReturn;
	}

	/// <summary>
	/// Collates all property names and uses their values as keys in a dictionary, where the value is an array of validation error messages
	/// for the property in question
	/// </summary>
	/// <remarks>
	/// Where a <see cref="ValidationResult"/> does not have any members names, they will be added to a single key, "General"
	/// <para>Where <see cref="ValidationResult.ErrorMessage"/> is <see langword="null"/> an empty string is substituted</para>
	/// </remarks>
	/// <param name="arg"><see cref="IEnumerable{T}"/> of <see cref="ValidationResult"/></param>
	/// <returns><see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> and <see cref="string"/> array</returns>
	public static Dictionary<string, string[]> ToPropertyErrorDictionary(this IEnumerable<ValidationResult> arg) {
		const string GENERAL = "General";

		Dictionary<string, string[]> toReturn =
				arg
				.SelectMany(v => v.MemberNames.Select(m => KeyValuePair.Create(m, v.ErrorMessage ?? "")))
				.GroupBy(g => g.Key)
				.ToDictionary(
						g => g.Key,
						v => v.Select(r => r.Value).ToArray());

		// Validation results without a member name will not be present in the dictionary above
		// so they all get placed in the
		var noMembName = arg.Where(v => !v.MemberNames.Any()).Select(v => v.ErrorMessage ?? "").ToArray();
		if (noMembName.Length > 0)
			toReturn[GENERAL] = noMembName;

		return toReturn;
	}

	/// <summary>
	/// Enumerates through each item in an IEnumerable and returns a string representation of each.
	/// <para>Nested IDictionary or IEnumerable values will be called recursively with a .ToStrings</para>
	/// <para>Values that are <see langword="null"/> return simply "null"</para>
	/// </summary>
	/// <param name="arg">The collection to run the code upon</param>
	/// <param name="useFullPropertyMode">Whether full/nested properties of each item should be returned</param>
	/// <param name="format">Format to use when displaying property name/value combinations when useFullPropertyMode is true</param>
	/// <returns><see cref="List{T}"/> of <see cref="string"/></returns>
	public static List<string> ToStrings(this IEnumerable arg, bool useFullPropertyMode = false, string? format = null, string? delimiterInner = null) {
		format ??= ToStringsFormat;
		delimiterInner ??= ToStringsDelimiterInner;

		int depth = 0;

		if (arg == null)
			return [Const.NULL];

		List<string> toReturn = [];

		foreach (object? t in arg)
			toReturn.Add(t.ToStringRich(ref depth, useFullPropertyMode, format, delimiterInner));

		return toReturn;
	}


	/// <summary>
	/// Returns validation results formatted with member names and error messages
	/// </summary>
	/// <param name="arg">Target validation results</param>
	/// <param name="delimiterMemberName">Used as a delimiter between the member names of a each validation result</param>
	/// <param name="delimiterMessage">Used as a delimiter between the member names and the error message of each validation result</param>
	/// <param name="omitMemberNames">Indicates whether to omit the names of members from each string</param>
	/// <returns><see cref="IEnumerable{T}"/> of <see cref="string"/></returns>
	public static IEnumerable<string> ToStrings(this IEnumerable<ValidationResult> arg, string? delimiterMemberName = null, string? delimiterMessage = null, bool omitMemberNames = default) {
		return arg.Select(v =>
				omitMemberNames ?
						v.ErrorMessage ?? Const.NULL :
						v.ToStringFull(delimiterMemberName, delimiterMessage));
	}
}