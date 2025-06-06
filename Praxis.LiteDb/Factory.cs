namespace Praxis.LiteDb;

using LiteDB;

/// <summary>
/// Helper class used for working with LiteDb
/// </summary>
public sealed class Factory {

	/// <summary>
	/// Holds a reference to a <see cref="BsonMapper"/> used for translation of entities and data
	/// </summary>
	private readonly BsonMapper _bsonMapper;

	/// <summary>
	/// Holds a reference to options that are used for both configuring and returning LiteDb related
	/// connections, repositories, etc
	/// </summary>
	private readonly FactoryOption _option;


	/// <summary>
	/// Used for tracking single initialization of lite db
	/// </summary>
	private int _initialized = 0;


	/// <summary>
	/// Creates an instance of the <see cref="Factory"/> class
	/// </summary>
	/// <param name="option">Used for configurating and creating LiteDb related constructs</param>
	public Factory(FactoryOption option) {
		_option = option;
		_bsonMapper = _CreateBsonMapper();
	}


	/// <summary>
	/// Used to ensure that indexes for LiteDb are configured and set as needed
	/// </summary>
	public LiteRepository GetLiteRepository() {
		_InitializeLiteDb();

		if (!File.Exists(_option.FileFullName))
			throw new FileNotFoundException(null, _option.FileFullName);

		return new LiteRepository(new ConnectionString { Connection = _option.ConnectionType, Filename = _option.FileFullName }, _bsonMapper);
	}


	/// <summary>
	/// Creates a <see cref="BsonMapper"/> with registered types and options for this domain
	/// </summary>
	/// <returns><see cref="BsonMapper"/></returns>
	private BsonMapper _CreateBsonMapper() {
		// There are several options available on the global mapper, such as auto trimming, empty strings being set to null
		// and not serializing null false. Make sure to adjust these per your domain
		BsonMapper bMapper = new() {
			EnumAsInteger = _option.EnumAsInteger,
			ResolveCollectionName = (t) => _option.MapTypesToCollectionNames.TryGetValue(t, out var name) ? name : t.Name,
			TrimWhitespace = _option.TrimWhitespace
		};

		_option.BsonMapping(FactoryOption.RegisterCustomDateTimeDateTimeOffsetMapping(bMapper));

		return bMapper;
	}

	/// <summary>
	/// Initializes a LiteDb file with appropriate collation, checkpoint sizes and indexes
	/// </summary>
	private void _InitializeLiteDb() {
		if (_initialized == 1 || Interlocked.Exchange(ref _initialized, 1) == 1)
			return;

		// Ensure indexes - used in case of existing edition upgrades with new versions added
		using var lr = _CreateIfNonExistent() ?? GetLiteRepository();
		_option.EnsureIndexes(lr);

		/* ----------------------------------------------------------------------------------------------------------
		 * Creates a new LiteDb file if it does not already exist on disk */
		LiteRepository? _CreateIfNonExistent() {
			if (File.Exists(_option.FileFullName))
				return null;

			var conString = new ConnectionString {
				Collation = FactoryOption.InvariantIgnoreCaseCollation,
				Connection = ConnectionType.Direct,
				Filename = _option.FileFullName
			};

			var lr = new LiteRepository(conString, _bsonMapper);
			lr.Database.CheckpointSize = _option.CheckpointSize;

			// Do *NOT* set the database as below as it causes unpredictable behavior -- see the tests in this solution for more information
			//lr.Database.UtcDate = utc;

			return lr;
		}
	}
}
