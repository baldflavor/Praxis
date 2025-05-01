namespace AdViewPlayer.Api.Helper;

using System.Globalization;
using LiteDB;

/// <summary>
/// Helper class used for working with LiteDb
/// </summary>
public abstract class InstanceHelper {

	/// <summary>
	/// Holds a reference to a <see cref="BsonMapper"/> used for translation of entities and data
	/// </summary>
	private readonly BsonMapper _bsonMapper;

	/// <summary>
	/// Used for tracking single initialization of lite db
	/// </summary>
	private int _initialized = 0;

	/// <summary>
	/// Dictionary that holds types and custom names -- those not defined will simply have their type name used
	/// </summary>
	/// <remarks>
	/// See <see cref="_CreateBsonMapper"/> -> <c>bMapper.ResolveCollectionName</c>
	/// </remarks>
	private readonly Dictionary<Type, string> _typeCollectionNames;


	/// <summary>
	/// Gets the checkpoint size to use on the database (when creating new)
	/// </summary>
	protected virtual int CheckpointSize => 500;

	/// <summary>
	/// Creates an instance of the <see cref="InstanceHelper"/> class
	/// </summary>
	public InstanceHelper() {
		_typeCollectionNames = MapTypesToCollectionNames();
		_bsonMapper = _CreateBsonMapper();
	}


	/// <summary>
	/// Used to ensure that indexes for LiteDb are configured and set as needed
	/// </summary>
	public LiteRepository GetLiteRepository(string fileFullName) {
		_InitializeLiteDb(fileFullName);

		if (!File.Exists(fileFullName))
			throw new FileNotFoundException(null, fileFullName);

		return new LiteRepository(new ConnectionString { Connection = ConnectionType.Shared, Filename = fileFullName }, _bsonMapper);
	}


	/// <summary>
	/// Provides Bson mapping for objects stored that require additional configuration
	/// </summary>
	/// <param name="bMapper"><see cref="BsonMapper"/></param>
	protected virtual void BsonMapping(BsonMapper bMapper) { }

	/// <summary>
	/// Used to ensure that LiteDb has indexes beyond the default (auto-id) for various collections
	/// </summary>
	/// <param name="lr"><see cref="LiteRepository"/></param>
	protected virtual void EnsureIndexes(LiteRepository lr) { }

	/// <summary>
	/// Used for mapping objects to collection names that are different than their type name
	/// </summary>
	/// <returns><see cref="Dictionary{TKey, TValue}"/> of <see cref="Type"/> and <see cref="string"/></returns>
	protected virtual Dictionary<Type, string> MapTypesToCollectionNames() => [];


	/// <summary>
	/// Creates a <see cref="BsonMapper"/> with registered types and options for this domain
	/// </summary>
	/// <returns><see cref="BsonMapper"/></returns>
	private BsonMapper _CreateBsonMapper() {
		const string DATETIMEFORMAT = "yyyy-MM-ddTHH:mm:ss.fffffff";

		// There are several options available on the global mapper, such as auto trimming, empty strings being set to null
		// and not serializing null false. Make sure to adjust these per your domain
		BsonMapper bMapper = new() {
			TrimWhitespace = false,
			EnumAsInteger = true
		};

		bMapper.RegisterType<DateTime>(
				value => value.ToString(DATETIMEFORMAT),
				bson => DateTime.Parse(bson));

		bMapper.RegisterType<DateTimeOffset>(
						value => value.ToString("O"),
						bson => DateTimeOffset.ParseExact(bson, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

		BsonMapping(bMapper);

		bMapper.ResolveCollectionName = (t) => _typeCollectionNames.TryGetValue(t, out var name) ? name : t.Name;
		return bMapper;
	}

	/// <summary>
	/// Initializes a LiteDb file with appropriate collation, checkpoint sizes and indexes
	/// </summary>
	/// <param name="fileFullName">The full file path of where the database should reside</param>
	private void _InitializeLiteDb(string fileFullName) {
		if (_initialized == 1 || Interlocked.Exchange(ref _initialized, 1) == 1)
			return;

		// Ensure indexes - used in case of existing edition upgrades with new versions added
		using var lr = _CreateIfNonExistent() ?? GetLiteRepository(fileFullName);
		EnsureIndexes(lr);

		/* ----------------------------------------------------------------------------------------------------------
		 * Creates a new LiteDb file if it does not already exist on disk */
		LiteRepository? _CreateIfNonExistent() {
			if (File.Exists(fileFullName))
				return null;

			var conString = new ConnectionString {
				Collation = new Collation(127, CompareOptions.OrdinalIgnoreCase),
				Connection = ConnectionType.Shared,
				Filename = fileFullName
			};

			var lr = new LiteRepository(conString, _bsonMapper);
			lr.Database.CheckpointSize = this.CheckpointSize;
			return lr;
		}
	}
}