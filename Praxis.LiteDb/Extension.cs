namespace Praxis.LiteDb;

using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using LiteDB;
using LiteDB.Engine;

/// <summary>
/// Extensions for <see cref="LiteDb"/> objects.
/// </summary>
public static class Extension {

	/// <summary>
	/// Gets a collection by type
	/// </summary>
	/// <typeparam name="T">Type to use for retrieving a collection</typeparam>
	/// <param name="lr"><see cref="LiteRepository"/> instance</param>
	/// <returns>An <see cref="ILiteCollection{T}"/></returns>
	public static ILiteCollection<T> Collection<T>(this ILiteRepository lr) => lr.Database.GetCollection<T>();

	/// <summary>
	/// Downloads a file
	/// </summary>
	/// <param name="lr"><see cref="ILiteDatabase"/> used for storage</param>
	/// <param name="id">ID of the file to retrieve</param>
	/// <param name="filename">The full file path where the file should be written to</param>
	/// <param name="filesCollection">The files collection used within the database</param>
	/// <param name="chunksCollection">The chunks collection used within the database</param>
	/// <param name="overwrite">Whether to overwrite an existing file</param>
	public static void DownloadFile(this ILiteRepository lr, string id, string filename, string filesCollection = "_files", string chunksCollection = "_chunks", bool overwrite = true) => lr.Database.GetStorage<string>(filesCollection, chunksCollection).Download(id, filename, overwrite);

	/// <summary>
	/// Drops a collection
	/// </summary>
	/// <typeparam name="T">Type to use for removing a collection</typeparam>
	/// <param name="lr"><see cref="LiteRepository"/> instance</param>
	/// <returns>True if the collection was dropped otherwise false</returns>
	public static bool DropCollection<T>(this ILiteRepository lr) => lr.Database.DropCollection(lr.Collection<T>().Name);

	/// <summary>
	/// Queries data using expression and optional limit / skip values
	/// </summary>
	/// <typeparam name="T">Type to use for collection being queried</typeparam>
	/// <param name="lr"><see cref="ILiteRepository"/> to use for query</param>
	/// <param name="predicate">Expression used to locate matching data</param>
	/// <param name="skip">Number to skip before returning results</param>
	/// <param name="limit">Limit of total results to return</param>
	/// <returns></returns>
	public static IEnumerable<T> Find<T>(this ILiteRepository lr, Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue) => lr.Database.GetCollection<T>().Find(predicate, skip, limit);

	/// <summary>
	/// Inserts a value into the database after asserting that <see cref="Validator.TryValidateObject(object, ValidationContext, ICollection{ValidationResult}?, bool)"/> returns <c>true</c>.
	/// </summary>
	/// <typeparam name="T">Type of data to insert</typeparam>
	/// <param name="db">Database used for data insertion</param>
	/// <param name="data">Data to insert. (Note that for auto incremented ID values it should be set post insert)</param>
	/// <returns><see cref="BsonValue"/> of the document id post insert</returns>
	public static BsonValue InsertValidated<T>(this ILiteDatabase db, T data) where T : class {
		return db.GetCollection<T>().Insert(_AssertValid(data));

		static K _AssertValid<K>(K data) where K : class {
			List<ValidationResult> validationResults = [];

			if(!Validator.TryValidateObject(data, new ValidationContext(data), validationResults, true))
				throw new AggregateException("Validation failed on arg object", [.. validationResults.Select(v => new ValidationException(v, null, null))]);

			return data;
		}
	}

	/// <summary>
	/// Initiates a rebuild on the database.
	/// </summary>
	/// <remarks>Will cause a backup file to be created until closed.</remarks>
	/// <param name="lr"><see cref="ILiteRepository"/> to use for database instance access</param>
	/// <param name="rebuildOptions">Options specifying the rebuild operation. When null, the collation used is <see cref="FactoryOption.BinaryCultureOrdinalIgnoreCase"/>  </param>
	[Obsolete("This currently does not function due to a bug in the underlying LiteDb engine", true)]
	public static void Rebuild(this ILiteRepository lr, RebuildOptions? rebuildOptions) => lr.Database.Rebuild(rebuildOptions ?? new RebuildOptions { Collation = FactoryOption.BinaryCultureOrdinalIgnoreCase });

	/// <summary>
	/// EXPERIMENTAL: Copy all collections and documents from a source repository to a new one.
	/// </summary>
	/// <param name="sourceLr">Source ILiteRepository.</param>
	/// <param name="collation">Collation to use for the new database.</param>
	/// <param name="destinationFullName">The fullname of a file where the new database should be written.</param>
	/// <param name="ensureIndexes">Action to use for ensuring indexes before data is copied.</param>
	/// <param name="password">Password to set on the new database.</param>
	public static void RebuildCopy(this ILiteRepository sourceLr, Collation collation, string destinationFullName, Action<LiteRepository>? ensureIndexes = null, string? password = null) {
		var conStr = new ConnectionString { Connection = ConnectionType.Direct, Collation = collation, Filename = destinationFullName };
		if (password is not null)
			conStr.Password = password;

		using var destLr = new LiteRepository(conStr);
		destLr.Database.CheckpointSize = sourceLr.Database.CheckpointSize;
		ensureIndexes?.Invoke(destLr);

		foreach (var colName in sourceLr.Database.GetCollectionNames()) {
			var sCol = sourceLr.Database.GetCollection(colName);
			var dCol = destLr.Database.GetCollection(colName, sCol.AutoId);
			dCol.Insert(sCol.FindAll());
		}
	}

	/// <summary>
	/// Uploads a file
	/// </summary>
	/// <param name="lr"><see cref="ILiteDatabase"/> used for storage</param>
	/// <param name="id">ID of the file to insert</param>
	/// <param name="fileFullName">The full file path to use for insertion data</param>
	/// <param name="filesCollection">The files collection used within the database</param>
	/// <param name="chunksCollection">The chunks collection used within the database</param>
	public static void UploadFile(this ILiteRepository lr, string id, string fileFullName, string filesCollection = "_files", string chunksCollection = "_chunks") => lr.Database.GetStorage<string>(filesCollection, chunksCollection).Upload(id, fileFullName);
}
