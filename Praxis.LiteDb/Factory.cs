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
	/// EXPERIMENTAL: Copy all collections and documents from a source repository to a new one.
	/// </summary>
	/// <remarks>
	/// Make sure to Dispose / Close the returned LiteRepository.
	/// </remarks>
	/// <param name="destinationFullName">The fullname of a file where the new database should be written.</param>
	/// <exception cref="ArgumentException">Thrown if <paramref name="destinationFullName"/> already exists.</exception>
	/// <exception cref="Exception">Thrown if the database cannot be rebuilt into a copy.</exception>
	public LiteRepository RebuildCopy(string destinationFullName) {
		try {
			if (File.Exists(destinationFullName))
				throw new ArgumentException("Destination filename already exists", nameof(destinationFullName));

			using var sourceLr = new LiteRepository(new ConnectionString { Connection = _option.ConnectionType, Filename = _option.FileFullName }, _bsonMapper);

			var destLr = new LiteRepository(
				new ConnectionString {
					Collation = FactoryOption.BinaryCultureOrdinalIgnoreCase,
					Connection = _option.ConnectionType,
					Filename = destinationFullName
				},
				_bsonMapper);

			destLr.Database.CheckpointSize = _option.CheckpointSize;
			_option.EnsureIndexes(destLr);

			foreach (var colName in sourceLr.Database.GetCollectionNames()) {
				var sCol = sourceLr.Database.GetCollection(colName);
				var dCol = destLr.Database.GetCollection(colName, sCol.AutoId);
				dCol.Insert(sCol.FindAll());
			}

			return destLr;
		}
		catch (Exception ex) {
			ex.Data[nameof(destinationFullName)] = destinationFullName;
			throw;
		}
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
	private bool _InitializeLiteDb() {
		if (_initialized == 1 || Interlocked.Exchange(ref _initialized, 1) == 1)
			return false;

		// Ensure indexes - used in case of existing edition upgrades with new versions added
		using var lr = _CreateIfNonExistent() ?? GetLiteRepository();
		_option.EnsureIndexes(lr);
		return true;

		/* ----------------------------------------------------------------------------------------------------------
		 * Creates a new LiteDb file if it does not already exist on disk */
		LiteRepository? _CreateIfNonExistent() {
			if (File.Exists(_option.FileFullName))
				return null;

			var conString = new ConnectionString {
				Collation = FactoryOption.BinaryCultureOrdinalIgnoreCase,
				Connection = _option.ConnectionType,
				Filename = _option.FileFullName
			};

			var lr = new LiteRepository(conString, _bsonMapper);
			lr.Database.CheckpointSize = _option.CheckpointSize;

			// Do *NOT* set `UtcDate` on the database as it causes unpredictable/alteration of values behavior -- see the tests in this solution for more information
			//lr.Database.UtcDate = utc;

			return lr;
		}
	}
}
