namespace Praxis;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Static class used for working with Json serialization
/// </summary>
/// <remarks>
/// Provides default serialization options, helpers, etc
/// </remarks>
public static partial class Json {

	/// <summary>
	/// Gets a static (thread safe) instance of <see cref="JsonSerializerOptions"/> which has been configured to provide a mix of
	/// strictness and flexibility
	/// </summary>
	/// <remarks>
	/// <para><b>Immutable</b></para>
	/// Note that this differs from the <see cref="JsonSerializerDefaults.Web"/> in the following ways:
	/// <list type="table">
	/// <listheader>
	///     <term>Property</term>
	///     <description>Value</description>
	/// </listheader>
	/// <item>
	///     <term><see cref="JsonSerializerOptions.DefaultIgnoreCondition"/></term>
	///     <description><see cref="JsonIgnoreCondition.WhenWritingNull"/></description>
	/// </item>
	/// <item>
	///     <term><see cref="JsonSerializerOptions.NumberHandling"/></term>
	///     <description><see cref="JsonNumberHandling.Strict"/></description>
	/// </item>
	/// <item>
	///     <term><see cref="JsonSerializerOptions.PreferredObjectCreationHandling"/></term>
	///     <description><see cref="JsonObjectCreationHandling.Populate"/></description>
	/// </item>
	/// <item>
	///     <term><see cref="JsonSerializerOptions.PropertyNameCaseInsensitive"/></term>
	///     <description><see langword="false"/></description>
	/// </item>
	/// <item>
	///     <term><see cref="JsonSerializerOptions.ReadCommentHandling"/></term>
	///     <description><see cref="JsonCommentHandling.Skip"/></description>
	/// </item>
	/// <item>
	///     <term><see cref="JsonSerializerOptions.UnknownTypeHandling"/></term>
	///     <description><see cref="JsonUnknownTypeHandling.JsonNode"/></description>
	/// </item>
	/// <item>
	///     <term><see cref="JsonSerializerOptions.UnmappedMemberHandling"/></term>
	///     <description><see cref="JsonUnmappedMemberHandling.Disallow"/></description>
	/// </item>
	/// </list>
	/// It also includes several extra converters:
	/// <list type="bullet">
	/// <item><see cref="IPAddressJsonConverter"/></item>
	/// <item><see cref="IPEndpointJsonConverter"/></item>
	/// <item><see cref="JsonStringEnumConverter"/>(namingPolicy: <see cref="JsonNamingPolicy.CamelCase"/>, allowIntegerValues: <see langword="true"/>)]</item>
	/// </list>
	/// </remarks>
	private static readonly JsonSerializerOptions _praxisDefaultOptions = new() {
		Converters = {
			new IPAddressJsonConverter(),
			new IPEndpointJsonConverter(),
			new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
		},
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
		ReadCommentHandling = JsonCommentHandling.Skip,
		UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
	};

	/// <summary>
	/// Holds a reference to <see cref="JsonSerializerOptions"/> which is accessed mainly through <see cref="Options"/> and may be overriden through <see cref="SetOptions"/>
	/// </summary>
	/// <remarks>Initially set to <see cref="_praxisDefaultOptions"/></remarks>
	private static JsonSerializerOptions _options = _praxisDefaultOptions;


	/// <summary>
	/// Gets a static (thread safe) instance of <see cref="JsonSerializerOptions"/>.
	/// </summary>
	/// <remarks>
	/// <b>Immutable</b>
	/// <para>Used internally through extension methods and other code in the Praxis library</para>
	/// </remarks>
	public static JsonSerializerOptions Options => _options;

	/// <summary>
	/// Statically initializes the <see cref="Json"/> class
	/// </summary>
	static Json() {
		JsonSerializer.Serialize(true, _praxisDefaultOptions);
	}

	/// <summary>
	/// Gets an immutable instance of <see cref="JsonSerializerOptions"/>.
	/// </summary>
	/// <param name="praxisDefault">Whether to return the initial <see cref="_praxisDefaultOptions"/> or the current instance set to <see cref="_options"/> which may
	/// have been set by external code</param>
	/// <returns><see cref="JsonSerializerOptions"/></returns>
	public static JsonSerializerOptions GetOptions(bool praxisDefault) => praxisDefault ? _praxisDefaultOptions : _options;

	/// <summary>
	/// Sets the <see cref="_options"/> field to the passed argument, which will <i>thus result in the <see cref="Options"/> property returning
	/// this instance</i>.
	/// <para>When calling this method, <paramref name="options"/> will <b>become immutable</b></para>
	/// </summary>
	/// <param name="options">The options to set</param>
	public static void SetOptionsImmutable(JsonSerializerOptions options) {
		JsonSerializer.Serialize(true, options);
		Interlocked.Exchange(ref _options, options);
	}
}