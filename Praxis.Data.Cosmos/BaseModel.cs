namespace Praxis.Data.Cosmos;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

/// <summary>
/// Class that serves as a base model for other entities used with CosmosDB
/// </summary>
public abstract partial class BaseModel {

	/// <summary>
	/// Constant field describing the string to use a delimiter between properties when representing an object as a string
	/// </summary>
	public const string DELIMITER = "\r\n¦";

	/// <summary>
	/// Regular expression string that corresponds to lowercase letters and 0-9
	/// </summary>
	public const string LCASEALPHA09 = "^[a-z0-9]*$";

	/// <summary>
	/// Regular expression string that corresponds to lowercase letters, 0-9 and underscore
	/// </summary>
	public const string LCASEALPHA09DOT = @"^[a-z0-9\.]*$";

	/// <summary>
	/// Describes an error message for violations of <see cref="LCASEALPHA09DOT"/>
	/// </summary>
	public const string LCASEALPHA09DOTERRMSG = "Lowercase letters, numbers and dot only";

	/// <summary>
	/// Describes an error message for violations of <see cref="LCASEALPHA09"/>
	/// </summary>
	public const string LCASEALPHA09ERRMSG = "Lowercase letters and numbers only";

	/// <summary>
	/// Regular expression string that corresponds to lowercase letters, 0-9 and underscore
	/// </summary>
	public const string LCASEALPHA09UND = "^[a-z0-9_]*$";

	/// <summary>
	/// Regular expression string that corresponds to lowercase letters, 0-9, underscore, dot
	/// </summary>
	public const string LCASEALPHA09UNDDOT = @"^[a-z0-9_\.]*$";

	/// <summary>
	/// Describes an error message for violations of <see cref="LCASEALPHA09UNDDOT"/>
	/// </summary>
	public const string LCASEALPHA09UNDDOTERRMSG = "Lowercase letters, numbers, underscore and dot only";

	/// <summary>
	/// Describes an error message for violations of <see cref="LCASEALPHA09UND"/>
	/// </summary>
	public const string LCASEALPHA09UNDERRMSG = "Lowercase letters, numbers and underscore only";

	/// <summary>
	/// Gets or sets a string value that is required to be present (and unique) for Cosmos entities within the same partition
	/// </summary>
	[JsonProperty("id")]
	[System.Text.Json.Serialization.JsonPropertyName("id")]
	[Required]
	[StringLength(255)]
	[CustomValidation(typeof(BaseModel), nameof(ValidateIdRestrictedCharacters))]
	public required string Id { get; set; }

	/// <summary>
	/// Backed by <see cref="Extension.ToStringProperties(object, List{string}, string, string)"/> with the delimiter specified as <see cref="DELIMITER"/>
	/// </summary>
	/// <param name="arg">Object to represent as a string</param>
	/// <param name="skipProperties">Properties to skip as part of string graph</param>
	/// <returns>A string</returns>
	public static string ToString(object arg, params string[] skipProperties) => arg.ToStringProperties(delimiter: DELIMITER, excludeProperties: skipProperties);

	/// <summary>
	/// Validates the Id of a cosmos base model to ensure it does not contain any characters restricted by Cosmos DB storage
	/// </summary>
	/// <param name="arg">Value to check for validity</param>
	/// <param name="context">Context of validation</param>
	/// <returns>A validation result</returns>
	public static ValidationResult? ValidateIdRestrictedCharacters(string arg, ValidationContext context) {
		if (_idAcceptableRegex().IsMatch(arg))
			return new ValidationResult($"{context.DisplayName} must not contain: \\, /, ?, #", context.MemberName is not null ? new string[] { context.MemberName } : null);

		return ValidationResult.Success;
	}

	/// <summary>
	/// Provides a string representation of this object
	/// </summary>
	/// <returns>A string</returns>
	public override string ToString() => this.ToStringProperties(delimiter: DELIMITER);

	/// <summary>
	/// Returns the Id of a container to use for Cosmos storage. This method is expensive and should be called minimially and with the
	/// returned value re-used for efficiency. This is due to the fact that a derived class that has the <see cref="ContainerIdAttribute"/>
	/// will have that attribute's value returned (requiring reflection) whereas those without will simply have their <see cref="Type"/> name returned
	/// </summary>
	/// <returns>A string</returns>
	internal string GetContainerId() {
		Type type = GetType();
		object[] attr = GetType().GetCustomAttributes(typeof(ContainerIdAttribute), true);
		if (attr.Length == 0)
			return type.Name;
		else
			return ((ContainerIdAttribute)attr[0]).Id;
	}

	/// <summary>
	/// Provides a regular expression that matches characters that are invalid for cosmos id values
	/// </summary>
	[GeneratedRegex("\\x2F|\\x5C|\\x3F|\\x23")]
	private static partial Regex _idAcceptableRegex();
}