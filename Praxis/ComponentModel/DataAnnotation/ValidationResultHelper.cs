namespace Praxis.ComponentModel.DataAnnotation;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Static class used for creating common validation results
/// </summary>
public static class ValidationResultHelper {
	/// <summary>
	/// Creates a validation result indicating that a value must be greater than zero
	/// </summary>
	/// <param name="propertyNames"><see cref="string"/> array of property names to assign to <see cref="ValidationResult.MemberNames"/>. Must contain at least one element</param>
	/// <returns><see cref="ValidationResult"/></returns>
	/// <exception cref="Exception">Thrown if <paramref name="propertyNames"/> does not contain at least one element</exception>
	public static ValidationResult GreaterThanZero(params string[] propertyNames) => new("Must be greater than 0", _AssertHasElement(propertyNames));

	/// <summary>
	/// Creates a validation result indicating that a Guid must have a non-empty value
	/// </summary>
	/// <param name="propertyNames"><see cref="string"/> array of property names to assign to <see cref="ValidationResult.MemberNames"/>. Must contain at least one element</param>
	/// <returns><see cref="ValidationResult"/></returns>
	/// <exception cref="Exception">Thrown if <paramref name="propertyNames"/> does not contain at least one element</exception>
	public static ValidationResult GuidNotEmpty(params string[] propertyNames) => new("Guid must have a non-empty value", _AssertHasElement(propertyNames));

	/// <summary>
	/// Creates a validation result indicating that a value must match one in a set of values
	/// </summary>
	/// <typeparam name="T">Type of values in <paramref name="values"/></typeparam>
	/// <param name="values">Acceptable values. Will be combined into a string separated by a comma in the result message and ordered using comparison provided by <typeparamref name="T"/></param>
	/// <param name="propertyNames"><see cref="string"/> array of property names to assign to <see cref="ValidationResult.MemberNames"/>. Must contain at least one element</param>
	/// <returns><see cref="ValidationResult"/></returns>
	/// <exception cref="Exception">Thrown if <paramref name="propertyNames"/> does not contain at least one element</exception>
	public static ValidationResult MustBeOneOfValue<T>(IEnumerable<T> values, params string[] propertyNames) => new($"Must be one of the following values: [{string.Join(',', values.Order())}]", _AssertHasElement(propertyNames));

	/// <summary>
	/// Asserts that the passed array is not null and contains at least one element
	/// </summary>
	/// <param name="arg"><see cref="string"/> array</param>
	/// <returns><paramref name="arg"/></returns>
	/// <exception cref="Exception">Thrown if there is not at least one element in <paramref name="arg"/></exception>
	private static string[] _AssertHasElement(string[] arg) {
		if ((arg?.Length > 0) != true)
			throw new Exception("Must be non-null and contain at least one element");
		return arg;
	}
}