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
	/// Default Json serialization options.
	/// </summary>
	private static readonly JsonSerializerOptions _defaultOptions = new() {
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
	/// Json serialization options.
	/// </summary>
	private static JsonSerializerOptions _options = _defaultOptions;


	/// <summary>
	/// Statically initializes the <see cref="Json"/> class.
	/// </summary>
	static Json() {
		// Perform serialization with options so they become immutable
		JsonSerializer.Serialize(true, _defaultOptions);
	}


	/// <summary>
	/// Gets a static (thread safe) instance of <c>JsonSerializerOptions</c> which has been configured to provide a mix of
	/// strictness and flexibility.
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
	public static JsonSerializerOptions DefaultOptions => _defaultOptions;

	/// <summary>
	/// Gets or sets a static (thread safe) instance of <see cref="JsonSerializerOptions"/>.
	/// </summary>
	/// <remarks>
	/// Setting a <c>value</c> to this property will make it <b>immutable</b>.
	/// <para>Used internally through extension methods and other code in the <c>Praxis</c> namespace.</para>
	/// <para>As such, setting this property will be an effective <b>global change</b> to the way serialization is handled in <c>Praxis</c>.</para>
	/// <para>A reference to the original options can be obtained from <see cref="DefaultOptions"/>.</para>
	/// </remarks>
	public static JsonSerializerOptions Options {
		get => _options;
		set {
			JsonSerializer.Serialize(true, value);
			_options = value;
		}
	}
}
