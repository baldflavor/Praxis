namespace Praxis.ComponentModel.DataAnnotation;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Provides generic validation results
/// </summary>
public static class CustomValidation {
	/// <summary>
	/// Constant string for when a member name is unknown.
	/// </summary>
	public const string MEMBERUNKNOWN = "MemberUnknown";

	/// <summary>
	/// Performs validation to ensure that the field does not contain characteres specified in the Strings.BracketsDelimeters regular expression
	/// </summary>
	/// <param name="arg">Target string value to validate</param>
	/// <param name="context">Validation context</param>
	/// <returns>A validation result</returns>
	public static ValidationResult? CannotContainBracketsDelimiters(string arg, ValidationContext context) {
		if (!string.IsNullOrWhiteSpace(arg) && Validation.BracketsDelimiters().IsMatch(arg))
			return new ValidationResult($"{context.DisplayName} cannot contain {Validation.BRACKETSDELIMITERSDESCRIPTION}", [context.MemberName ?? MEMBERUNKNOWN]);

		return ValidationResult.Success;
	}

	/// <summary>
	/// Performs boolean validation that specifies a value must be true
	/// </summary>
	/// <param name="arg">Target value to validate</param>
	/// <param name="context">Context of validation</param>
	/// <returns>A ValidationResult</returns>
	public static ValidationResult? EnforceTrue(bool arg, ValidationContext context) {
		if (!arg)
			return new ValidationResult($"{context.DisplayName} must be true", [context.MemberName ?? MEMBERUNKNOWN]);
		else
			return ValidationResult.Success;
	}

	/// <summary>
	/// Validates that a arg guid is not an empty (default) value
	/// </summary>
	/// <param name="arg">Target Guid to validate</param>
	/// <param name="context">Validation context to use</param>
	/// <returns>A validation result</returns>
	public static ValidationResult? GuidNonEmpty(Guid arg, ValidationContext context) {
		if (arg == Guid.Empty)
			return new ValidationResult("Guid must have a non-empty value", [context.MemberName ?? MEMBERUNKNOWN]);
		else
			return ValidationResult.Success;
	}

	/// <summary>
	/// Performs validation to ensure that a string composes a valid (12 character) E.164 phone number
	/// </summary>
	/// <param name="arg">Target string value to validate</param>
	/// <param name="context">Validation context</param>
	/// <returns>A validation result</returns>
	public static ValidationResult? IsValidE164(string arg, ValidationContext context) {
		if (!string.IsNullOrWhiteSpace(arg) && !arg.IsValidE164Number())
			return new ValidationResult("Must be 12 character E.164 valid phone number", [context.MemberName ?? MEMBERUNKNOWN]);

		return ValidationResult.Success;
	}

	/// <summary>
	/// Validates that the given property cannot end with a forward slash /
	/// </summary>
	/// <param name="arg">Target string to validate</param>
	/// <param name="context">Validation context to use</param>
	/// <returns>A validation result</returns>
	public static ValidationResult? NoTrailingSlash(string arg, ValidationContext context) {
		if (!string.IsNullOrWhiteSpace(arg) && arg.EndsWith("/", StringComparison.OrdinalIgnoreCase))
			return new ValidationResult($"{context.DisplayName} cannot end with a /", [context.MemberName ?? MEMBERUNKNOWN]);

		return ValidationResult.Success;
	}
}
