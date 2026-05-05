namespace Praxis.LiteDb;

using System.Globalization;
using LiteDB;

/// <summary>
/// Options for configuring and using LiteDb.
/// </summary>
/// <remarks>
/// Used in conjunction with <see cref="Factory"/>.
/// </remarks>
public sealed class FactoryOption {

	/// <summary>
	/// Format used when serializing / deserializing <see cref="DateTime"/>/<see cref="DateTimeOffset"/> when
	/// <see cref="RegisterCustomDateTimeDateTimeOffsetMapping(BsonMapper)"/> is executed.
	/// </summary>
	public const string DATETIMEFORMAT = "yyyy-MM-ddTHH:mm:ss.fffffff";

	/// <summary>
	/// Collation that specifies binary culture (127) and CompareOptions.OrdinalIgnoreCase.
	/// </summary>
	/// <remarks>Use this as part of a connection string as follows: new ConnectionString { Collation = FactoryOption.BinaryCultureOrdinalIgnoreCase, Filename = fileName }</remarks>
	public static Collation BinaryCultureOrdinalIgnoreCase { get; } = new(Collation.Binary.LCID, CompareOptions.OrdinalIgnoreCase);


	/// <summary>
	/// Gets or inits a delegate that performs final configuration of BsonMapper before being stored / reused by a Factory.
	/// </summary>
	/// <remarks>
	/// This can be used to create a whole new BsonMapper to replace the one created by <see cref="CreateBsonMapperWithSensibleDefaults"/>
	/// when creating a <see cref="Factory"/>.
	/// </remarks>
	public Func<BsonMapper, BsonMapper> ConfigureBsonMapper { get; init; } = (b) => b;

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
	/// Default performs no action.
	/// </remarks>
	public Action<LiteRepository> EnsureIndexes { get; init; } = (l) => { };

	/// <summary>
	/// Gets or inits the full file path used as the storage location for LiteDb instances
	/// </summary>
	public required string FileFullName { get; init; }

	/// <summary>
	/// Action to call when the first initialization of the database is called, whether newly created or opened, after <see cref="EnsureIndexes"/>.
	/// </summary>
	/// <remarks>
	/// This should run once per lifetime of an application context.
	/// </remarks>
	public Action<LiteRepository, Factory> OnInitialized { get; init; } = (l, f) => { };



	/// <summary>
	/// Create a BsonMapper with some default values set differently than the original class defaults.
	/// </summary>
	/// <remarks>
	/// Changes / differences are as follows:
	/// <list type="bullet">
	/// <item><see cref="BsonMapper.EnumAsInteger"/> = true (must be true to support LINQ expressions)</item>
	/// <item><see cref="BsonMapper.EmptyStringToNull"/> = false (store what is there and do not alter it before storage)</item>
	/// <item><see cref="BsonMapper.SerializeNullValues"/> = true (store what is there and do not leave off properties because of null values) </item>
	/// <item><see cref="BsonMapper.TrimWhitespace"/> = false (store what is there and do not alter is before storage)</item>
	/// </list>
	/// </remarks>
	/// <returns><see cref="BsonMapper"/></returns>
	public static BsonMapper CreateBsonMapperWithSensibleDefaults() {
		return new() {
			EnumAsInteger = true,
			EmptyStringToNull = false,
			SerializeNullValues = true,
			TrimWhitespace = false
		};
	}

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
