namespace Praxis.LiteDb;

using System.Globalization;
using LiteDB;
using LiteDB.Engine;

/// <summary>
/// Options for configuring and using LiteDb.
/// </summary>
/// <remarks>
/// Used in conjunction with <see cref="Factory"/>.
/// </remarks>
public class FactoryOption {

	/// <summary>
	/// Format that is used when serializing / deserializing <see cref="DateTime"/> when <see cref="RegisterCustomDateTimeDateTimeOffsetMapping(BsonMapper)"/>
	/// is executed.
	/// </summary>
	public const string DATETIMEFORMAT = "yyyy-MM-ddTHH:mm:ss.fffffff";

	/// <summary>
	/// Collation that specifies binary culture (127) and CompareOptions.OrdinalIgnoreCase.
	/// </summary>
	/// <remarks>Use this as part of a connection string as follows: new ConnectionString { Collation = FactoryOption.BinaryCultureOrdinalIgnoreCase, Filename = fileName }</remarks>
	public static Collation BinaryCultureOrdinalIgnoreCase { get; } = new(Collation.Binary.LCID, CompareOptions.OrdinalIgnoreCase);


	/// <summary>
	/// Gets or inits a delegate that performs extra bson mapping for objects that require additional configuration for storage and retrieval
	/// </summary>
	/// <remarks>
	/// Default performs no action
	/// </remarks>
	public Action<BsonMapper> BsonMapping { get; init; } = (b) => { };

	/// <summary>
	/// Gets or inits the checkpoint size to use on the database (when creating new)
	/// </summary>
	/// <remarks>
	/// Default is <c>500</c>
	/// </remarks>
	public int CheckpointSize { get; init; } = 500;

	/// <summary>
	/// Gets or inits the connection type used when accessing a LiteDb instance
	/// </summary>
	/// <remarks>
	/// Default is <see cref="ConnectionType.Shared"/>
	/// </remarks>
	public ConnectionType ConnectionType { get; init; } = ConnectionType.Shared;

	/// <summary>
	/// Gets or inits a delegate used to ensure that LiteDb has indexes beyond the default (auto-id) for various collections
	/// </summary>
	/// <remarks>
	/// Default to performs no action
	/// </remarks>
	public Action<LiteRepository> EnsureIndexes { get; init; } = (l) => { };

	/// <summary>
	/// Gets or inits a value indicating whether or not enums will be stored as integers in data
	/// </summary>
	/// <remarks>
	/// Default is <see langword="true"/>
	/// </remarks>
	public bool EnumAsInteger { get; init; } = true;

	/// <summary>
	/// Gets or inits the full file path used as the storage location for LiteDb instances
	/// </summary>
	public required string FileFullName { get; init; }

	/// <summary>
	/// Used for mapping objects to collection names that are different than their type name
	/// </summary>
	/// <returns><see cref="Dictionary{TKey, TValue}"/> of <see cref="Type"/> and <see cref="string"/></returns>
	public Dictionary<Type, string> MapTypesToCollectionNames { get; init; } = [];

	/// <summary>
	/// Action to call when the first initialization of the database is called, whether newly created or opened.
	/// </summary>
	/// <remarks>
	/// This should run once per lifetime of an application context.
	/// </remarks>
	public Action<LiteRepository>? OnInitialized { get; init; }

	/// <summary>
	/// Gets or inits a value indicating whether strings will have whitespace auto trimmed during storage
	/// </summary>
	/// <remarks>
	/// Default is <see langword="false"/>
	/// <para>Beware of changing this to <see langword="true"/>: while it may save some space, this can
	/// cause values with intentional spaces and their sizes not to match depending on your code</para>
	/// </remarks>
	public bool TrimWhitespace { get; init; } = false;



	/// <summary>
	/// Adds registration for <see cref="DateTime"/> and <see cref="DateTimeOffset"/> types
	/// </summary>
	/// <remarks>
	/// For <see cref="DateTime"/> there is no adjustment to UTC or Local performed on serialization. On deserialization, the resulting <see cref="DateTimeKind"/>
	/// will be set to <see cref="DateTimeKind.Unspecified"/>.
	/// <para>
	/// For <see cref="DateTimeOffset"/> on serialization the round trip specifier is used and will include any offset at the end of the string. On
	/// deserialization, the resulting value will have the time and offset set appropriately.
	/// </para>
	/// </remarks>
	/// <param name="bMapper">Mapper to add registration to</param>
	/// <returns><see cref="BsonMapper"/></returns>
	public static BsonMapper RegisterCustomDateTimeDateTimeOffsetMapping(BsonMapper bMapper) {
		bMapper.RegisterType<DateTime>(
			value => value.ToString(DATETIMEFORMAT),
			bson => DateTime.Parse(bson));

		bMapper.RegisterType<DateTimeOffset>(
				value => value.ToString("O"),
				bson => DateTimeOffset.ParseExact(bson, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

		return bMapper;
	}
}
