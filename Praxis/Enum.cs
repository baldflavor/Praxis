namespace Praxis;

/// <summary>
/// Static class used for parsing out enumerated values from a target type and obtaining descriptions and values
/// </summary>
/// <typeparam name="T">The type of enumeration to work with</typeparam>
public static class Enum<T> where T : struct, Enum {
	/// <summary>
	/// Gets the individual flag values from a flag enumeration
	/// </summary>
	/// <param name="flags">Flags to use as source for inspecting present values</param>
	/// <returns>IEnumerable of the individual enumeration values present</returns>
	public static IEnumerable<T> GetFlags(T flags) {
		foreach (Enum value in Enum.GetValues<T>()) {
			if (flags.HasFlag(value))
				yield return (T)value;
		}
	}

	/// <summary>
	/// Parses the given enumerated value (either the string "name" or the numeric value into an Enum. Case insensitive.
	/// </summary>
	/// <param name="target">The string to use for parsing</param>
	/// <returns>Corresponding TData enum value</returns>
	/// <exception cref="ArgumentException">Thrown if parsing does not succeed</exception>
	public static T Parse(string target) {
		if (!Enum.TryParse<T>(target, true, out T result) || !Enum.IsDefined(result))
			throw new ArgumentException($"Could not parse [{target}] into an enumerated value of the given type", nameof(target));
		else
			return result;
	}

	/// <summary>
	/// Attempts to parse a given string to an enumeration with the option of providing a default if parsing fails
	/// </summary>
	/// <remarks>Prefer this over the built in <see cref="System.Enum.TryParse{TEnum}(string?, out TEnum)"/> as that method, when specifying a number as a string, will return true
	/// and provide a value that is not actually defined. (Thus the need for the check of <see cref="Enum.IsDefined(Type, object)"/> below)</remarks>
	/// <param name="target">String to attempt to parse to an enum</param>
	/// <param name="result">The result of the parsing attempt</param>
	/// <param name="defaultValue">Default enum value if parsing fails</param>
	/// <returns>Whether the parse operation succeeded or failed</returns>
	public static bool TryParse(string target, out T result, T defaultValue = default) {
		if (!Enum.TryParse<T>(target, true, out T attempt) || !Enum.IsDefined(attempt)) {
			result = defaultValue;
			return false;
		}
		else {
			result = attempt;
			return true;
		}
	}

	/// <summary>
	/// Returns the values and corresponding DisplayAttribute.Name(s), DescriptionAttribute.Description or the value.ToStrings() of
	/// <typeparamref name="T"/> enumeration
	/// </summary>
	/// <returns>Tuple of <typeparamref name="T"/> value and string</returns>
	/// <exception cref="Exception">Thrown on operation failure</exception>
	public static IEnumerable<(T value, string display)> ValuesDisplay() => System.Enum.GetValues<T>().Select(e => (e, e.DescriptionDisplayName()));
}