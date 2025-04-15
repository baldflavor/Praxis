namespace Praxis.LiteDb;

using System.Globalization;
using LiteDB;
using LiteDB.Engine;

/// <summary>
/// Options for configuring and using LiteDb
/// </summary>
public static class Option {

	/// <summary>
	/// Format that is used when serializing / deserializing <see cref="DateTime"/> when <see cref="UseCustomDateTimeDateTimeOffsetMapping(BsonMapper)"/>
	/// is used.
	/// </summary>
	public const string DATETIMEFORMAT = "yyyy-MM-ddTHH:mm:ss.fffffff";

	/// <summary>
	/// Returns a BsonMapper with some preset options. Cache instances for better performance when not persisting a database / repository
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item><see cref="BsonMapper.TrimWhitespace"/> = true</item>
	/// <item><see cref="BsonMapper.EnumAsInteger"/> = true</item>
	/// </list>
	/// </remarks>
	/// <returns><see cref="BsonMapper"/></returns>
	public static BsonMapper GetBsonMapperNoTrimWhitespaceEnumAsInteger() => new() { TrimWhitespace = false, EnumAsInteger = true };

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
	public static BsonMapper UseCustomDateTimeDateTimeOffsetMapping(this BsonMapper bMapper) {
		bMapper.RegisterType<DateTime>(
			value => value.ToString(DATETIMEFORMAT),
			bson => DateTime.Parse(bson));

		bMapper.RegisterType<DateTimeOffset>(
				value => value.ToString("O"),
				bson => DateTimeOffset.ParseExact(bson, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

		return bMapper;
	}


	/// <summary>
	/// Rebuild options that contain collation for invariant culture and with comparison for ordinal ignore case on strings
	/// </summary>
	public static RebuildOptions DefaultRebuildOptions { get; } = new() { Collation = InvariantIgnoreCaseCollation };

	/// <summary>
	/// Collation that specified an invariant culture (127) and CompareOptions.OrdinalIgnoreCase
	/// </summary>
	/// <remarks>Use this as part of a connection string as follows: new ConnectionString { Collation = Option.InvariantIgnoreCaseCollation, Filename = fileName }</remarks>
	public static Collation InvariantIgnoreCaseCollation { get; } = new(127, CompareOptions.OrdinalIgnoreCase);
}