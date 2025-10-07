namespace Praxis;

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {
	/// <summary>
	/// Default delimter to use between when joining items / strings.
	/// </summary>
	public static string JoinDelimiter { get; set; } = Const.BROKENVERTBARSTRING;

	/// <summary>
	/// Inner delimiter (for full property) for items in <see cref="ToStrings(IEnumerable, bool, string?, string?)"/> or <see cref="ToStrings(IEnumerable{ValidationResult}, string?, string?, bool)"/>.
	/// </summary>
	public static string ToStringsDelimiterInner { get; set; } = Const.BROKENVERTBARSTRING;

	/// <summary>
	/// Format to use for <see cref="ToStrings(IEnumerable, bool, string?, string?)"/> or <see cref="ToStrings(IEnumerable{ValidationResult}, string?, string?, bool)"/>.
	/// </summary>
	public static string ToStringsFormat { get; set; } = "{0}" + Const.VERTICALELLIPSIS + "{1}";


	/// <summary>
	/// Enumerates through elements in a collection where they are <c>OfType&lt;IDisposable&gt;</c> and calls
	/// <c>Dispose()</c> on each.
	/// </summary>
	/// <remarks>
	/// Exceptions encountered during <c>Dispose</c> calls are aggregated and thrown before the method exits, but will not break
	/// the loop of calling <c>Dispose</c> on subsequent elements.
	/// </remarks>
	/// <typeparam name="T">Collection type holding objects to dispose of.</typeparam>
	/// <param name="arg">Collection to operate upon.</param>
	/// <returns><paramref name="arg"/></returns>
	/// <exception cref="AggregateException">Thrown if exceptions are encountered as result of the dispose call.</exception>
	public static T DisposeAny<T>(this T arg) where T : IEnumerable {
		List<Exception> exceptions = [];

		foreach (var item in arg) {
			if (item is IDisposable iDisp) {
				try {
					iDisp.Dispose();
				}
				catch (Exception ex) {
					exceptions.Add(ex);
				}
			}
		}

		if (exceptions.Count > 0)
			throw new AggregateException("Exceptions encountered during disposal", exceptions);

		return arg;
	}

	/// <summary>
	/// Returns a value that indicates if the passed <paramref name="arg"/> has any elements.
	/// <para>If the passed argument is <c>null</c> then <c>false</c> is returned.</para>
	/// <para>If the passed argument has a <c>Count</c> greater than <b>0</b> <c>true</c> is returned.</para>
	/// </summary>
	/// <param name="arg">Target collection.</param>
	/// <returns><c>true</c> if <paramref name="arg"/> has elements otherwise <c>false</c>.</returns>
	public static bool HasElements(this ICollection? arg) => arg?.Count > 0;

	/// <summary>
	/// Joins a string array into a single string using an optional delimiter.
	/// </summary>
	/// <param name="arg">Array to join together.</param>
	/// <param name="delimiter">Optional delimiter. If <see langword="null"/> then <see cref="JoinDelimiter"/> is used</param>
	/// <returns><see cref="string"/></returns>
	public static string Join(this IEnumerable<string> arg, string? delimiter = null) => string.Join(delimiter ?? JoinDelimiter, arg);

	/// <summary>
	/// Gets the keys and values and projects them into a <see cref="KeyValuePair{TKey, TValue}"/> of <see cref="string"/>/<see cref="string"/>
	/// </summary>
	/// <param name="arg">Collection used as source data</param>
	/// <returns><see cref="KeyValuePair{TKey, TValue}"/> of <see cref="string"/>/<see cref="string"/></returns>
	public static KeyValuePair<string, string?>[] KeysValues(this NameValueCollection arg) {
		return [.. _GetKvps(arg)];

		static IEnumerable<KeyValuePair<string, string?>> _GetKvps(NameValueCollection arg) {
			foreach (string? key in arg.AllKeys) {
				if (key is not null)
					yield return KeyValuePair.Create(key, arg[key]);
			}
		}
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
	/// <param name="delimiterInner">Delimiter to use for nested calls to string/value formatting.</param>
	/// <returns><see cref="List{T}"/> of <see cref="string"/></returns>
	public static List<string> ToStrings(this IEnumerable arg, bool useFullPropertyMode = false, string? format = null, string? delimiterInner = null) {
		format ??= ToStringsFormat;
		delimiterInner ??= ToStringsDelimiterInner;

		int depth = 0;

		if (arg is null)
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
