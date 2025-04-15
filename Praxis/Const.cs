namespace Praxis;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

/// <summary>
/// Class used for holding various constant values
/// </summary>
public static class Const {

	/// <summary>
	/// Broken vertical bar
	/// </summary>
	/// <remarks>
	/// Unic Number: U+00A6
	///<para>HTML Code: <c>&amp;brvbar;</c> or <c>&#166;</c> </para>
	/// </remarks>
	public const char BROKENVERTBAR = '¦';

	/// <summary>
	/// Broken vertical bar
	/// </summary>
	/// <remarks>
	/// Unic Number: U+00A6
	///<para>HTML Code: <c>&amp;brvbar;</c> or <c>&#166;</c> </para>
	/// </remarks>
	public const string BROKENVERTBARSTRING = "¦";

	/// <summary>
	/// Carriage return / newline
	/// </summary>
	public const string CRLF = "\r\n";

	/// <summary>
	/// Represents the numeric equivalent of a <see langword="false"/> value.
	/// </summary>
	public const int FALSEINT = 0;

	/// <summary>
	/// Number of milliseconds in one minute
	/// </summary>
	public const int MILLISECONDSONEMINUTE = 60_000;

	/// <summary>
	/// Number of milliseconds in ten minutes
	/// </summary>
	public const int MILLISECONDTENMINUTES = 600_000;

	/// <summary>
	/// A string indicating that a value was <see langword="null"/>
	/// </summary>
	public const string NULL = "null";

	/// <summary>
	/// Right arrowhead unicode character
	/// </summary>
	/// <remarks>
	/// Unic Number: <c>U+02C3</c>
	///<para>HTML Code: <c>&#707;</c></para>
	///<para>CSS Code: <c>\02C3</c></para>
	///<para>Plane: 0 [Basic Multilingual Plane]</para>
	///<para>Unic Block: Spacing Modifier Letters</para>
	///<para>Unic Subblock: Miscellaneous phonetic modifiers</para>
	///<para>Unic Version: 1.1 (1993)</para>
	/// </remarks>
	public const char RIGHTARROWHEAD = '›';

	/// <summary>
	/// Right arrowhead unicode character
	/// </summary>
	/// <remarks>
	/// Unic Number: <c>U+02C3</c>
	///<para>HTML Code: <c>&#707;</c></para>
	///<para>CSS Code: <c>\02C3</c></para>
	///<para>Plane: 0 [Basic Multilingual Plane]</para>
	///<para>Unic Block: Spacing Modifier Letters</para>
	///<para>Unic Subblock: Miscellaneous phonetic modifiers</para>
	///<para>Unic Version: 1.1 (1993)</para>
	/// </remarks>
	public const string RIGHTARROWHEADSTRING = "›";

	/// <summary>
	/// Represents the numeric equivalent of a <see langword="true"/> value.
	/// </summary>
	public const int TRUEINT = 1;

	/// <summary>
	/// Binding Flags that describes, effectively, "all":
	/// <para>BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic</para>
	/// </summary>
	/// <remarks>You may wish to consider adding: BindingFlags.DeclaredOnly -- can be included to remove inherited members</remarks>
	public static BindingFlags BindingFlagsAll => BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
}

/// <summary>
/// Class used for asserting the values of constants and getting lists of constant values and field names
/// </summary>
/// <remarks>
/// When making a constants class it is not recommended to have multiple fields with the same value
/// </remarks>
/// <typeparam name="T">The <see cref="Type"/> of class that contains constants to assert against</typeparam>
public static class Const<T> where T : class {

	/// <summary>
	/// Perform value assertion. If the value passes assertion, it is returned, otherwise an <see cref="ArgumentException"/> is thrown
	/// </summary>
	/// <param name="arg">The value to assert is a member of a class containing constants</param>
	/// <exception cref="ArgumentException">Thrown if the passed value does not match a similarly defined constant</exception>
	/// <returns>The value passed into the method</returns>
	public static K Assert<K>(K arg) {
		if (!_HasMatch(arg))
			throw new ArgumentException($"The given value [{arg}] is not in the list of acceptable constants for {typeof(T).Name}");

		return arg;
	}

	/// <summary>
	/// Perform assertion that the passed string is a <see cref="StringComparison.OrdinalIgnoreCase"/> match against a value in the <typeparamref name="T"/> constant class.
	/// </summary>
	/// <remarks>If asserted, then the value that was stored <b>FOR</b> the constant is returned, otherwise an <see cref="ArgumentException"/> is thrown</remarks>
	/// <param name="arg">The value to assert is a member of a class containing constants</param>
	/// <exception cref="ArgumentException">Thrown if the passed value does not match a similarly defined constant</exception>
	/// <returns>The <strong>constant</strong> value matched by <paramref name="arg"/></returns>
	public static string AssertCoerce(string arg) {
		Type stringType = typeof(string);
		foreach (var field in _GetFields().Where(k => k.FieldType == stringType)) {
			string? value = (string?)field.GetValue(null);
			if (arg.Equals(value, StringComparison.OrdinalIgnoreCase))
				return value;
		}

		throw new ArgumentException($"The given value [{arg}] is not in the list of acceptable constants for {typeof(T).Name}");
	}

	/// <summary>
	/// Gets names, descriptions and values for a constants class
	/// </summary>
	/// <remarks>Uses the <see cref="DisplayAttribute"/> to look for an optional name and description. If <see cref="DisplayAttribute.Name"/> is not present
	/// then the name of the field is used</remarks>
	/// <returns>Tuple with name, description and value</returns>
	public static IEnumerable<(string name, string? description, object? value)> NameDescriptionValues() {
		foreach (var field in _GetFields()) {
			var dispAttribute = field.GetCustomAttribute<DisplayAttribute>();
			yield return (
							dispAttribute?.Name ?? field.Name,
							dispAttribute?.Description,
							field.GetValue(null));
		}
	}

	/// <summary>
	/// Gets names, descriptions and values for a constants class filtered to those where the type of value matches <typeparamref name="K"/>
	/// </summary>
	/// <remarks>Uses the <see cref="DisplayAttribute"/> to look for an optional name and description. If <see cref="DisplayAttribute.Name"/> is not present
	/// <typeparam name="K">Type to use for filtering values</typeparam>
	/// <returns>Tuple with name, description and value</returns>
	public static IEnumerable<(string name, string? description, K value)> NameDescriptionValues<K>() {
		Type kType = typeof(K);
		foreach (var field in _GetFields().Where(f => f.FieldType == kType)) {
			var dispAttribute = field.GetCustomAttribute<DisplayAttribute>();
			yield return (
							dispAttribute?.Name ?? field.Name,
							dispAttribute?.Description,
							(K)field.GetValue(null)!);
		}
	}

	/// <summary>
	/// Validates a passed value against the list of constant values in a class
	/// </summary>
	/// <param name="arg">The value to check as being valid in the class containing the constants</param>
	/// <param name="valRes"><see cref="ValidationResult"/> containing either failure information or <see cref="ValidationResult.Success"/></param>
	/// <param name="memberNames">The members / properties that may be associated with the validation check</param>
	/// <returns>True if the value is valid in the the constants class, otherwise false</returns>
	public static bool Validate<K>(K arg, out ValidationResult? valRes, params string[] memberNames) {
		if (!_HasMatch(arg)) {
			valRes = new ValidationResult($"The given value [{arg}] is not in the list of acceptable constants for {typeof(T).Name}", memberNames);
			return false;
		}

		valRes = ValidationResult.Success;
		return true;
	}

	/// <summary>
	/// Returns all of the values for a given constants class as objects
	/// </summary>
	public static object?[] Values() => _GetFields().Select(f => f.GetValue(null)).ToArray();

	/// <summary>
	/// Returns all of the <typeparamref name="K"/> type values for a given constants class
	/// </summary>
	/// <typeparam name="K">Type to use as a filter when retrieving values</typeparam>
	public static K[] Values<K>() where K : notnull {
		Type kType = typeof(K);
		return _GetFields().Where(f => f.FieldType == kType).Select(f => (K)f.GetValue(null)!).ToArray();
	}

	/// <summary>
	/// Gets the fields for this type that are suitable for constant assertion / lookup
	/// </summary>
	/// <returns><see cref="IEnumerable{T}"/> of <see cref="FieldInfo"/></returns>
	private static IEnumerable<FieldInfo> _GetFields() => typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public).Where(f => f.IsLiteral);

	/// <summary>
	/// Determines whether the value in question has a match against a value stored in a field in <typeparamref name="T"/>
	/// </summary>
	/// <typeparam name="K">Type specified for <paramref name="arg"/></typeparam>
	/// <param name="arg">Value to check for a match</param>
	/// <returns><see langword="true"/> if there is a match for the value, otherwise <see langword="false"/></returns>
	private static bool _HasMatch<K>(K arg) {
		Type kType = typeof(K);
		return _GetFields().Where(f => f.FieldType == kType).Any(f => EqualityComparer<K>.Equals(arg, f.GetValue(null)));
	}
}