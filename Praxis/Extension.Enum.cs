namespace Praxis;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Asserts that <paramref name="arg"/> is defined as member of its backing enumeration
	/// </summary>
	/// <typeparam name="T">Type of enumeration</typeparam>
	/// <param name="arg">Value to assert is defined</param>
	/// <returns><paramref name="arg"/></returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="arg"/> is not defined in the target enumeration</exception>
	public static T AssertDefined<T>(this T arg) where T : struct, Enum {
		if (!Enum.IsDefined(arg))
			throw new ArgumentException("Value is not defined as a member of the target enumeration").AddData(new { arg, typeof(T).Name });

		return arg;
	}

	/// <summary>
	/// Returns the display attribute name property for this enumeration value
	/// </summary>
	/// <param name="value">Enumeration value to return for</param>
	/// <returns>In null coalesced order: <see cref="DisplayAttribute.Name"/>, <see cref="DisplayAttribute.Description"/>, value.ToStrings()</returns>
	public static string DescriptionDisplayName(this Enum value) {
		string valString = value.ToString();
		DisplayAttribute? displayAttribute = value.Attribute<DisplayAttribute>(valString);
		return
				displayAttribute?.Name ??
				displayAttribute?.Description ??
				valString;
	}
}