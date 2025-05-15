namespace Praxis;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {
	/// <summary>
	/// Default delimter to use between each member name in <see cref="ToStringFull(ValidationResult, string?, string?)"/>
	/// </summary>
	public static string ValidationResultDelimiterMemberName { get; set; } = ",";

	/// <summary>
	/// Default delimter to use between member names and the error message message <see cref="ToStringFull(ValidationResult, string?, string?)"/>
	/// </summary>
	public static string ValidationResultDelimiterMessage { get; set; } = ": ";

	/// <summary>
	/// Returns both the member names and error message together using passed (optional) delimeters
	/// </summary>
	/// <param name="arg">Target validation results</param>
	/// <param name="delimiterMemberName">Used as a delimiter between the member names of a each validation result</param>
	/// <param name="delimiterMessage">Used as a delimiter between the member names and the error message of each validation result</param>
	/// <returns><see cref="string"/></returns>
	public static string ToStringFull(this ValidationResult arg, string? delimiterMemberName = default, string? delimiterMessage = default) => $"{string.Join(delimiterMemberName ?? ValidationResultDelimiterMemberName, arg.MemberNames)}{delimiterMessage ?? ValidationResultDelimiterMessage}{arg.ErrorMessage}";
}