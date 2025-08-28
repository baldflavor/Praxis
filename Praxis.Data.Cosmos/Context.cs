namespace Praxis.Data.Cosmos;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

/// <summary>
/// Class used as a "helper" data context for performing data operations against CosmosDB storage
/// </summary>
public sealed class Context {

	/// <summary>
	/// Holds a reference to a context used for accessing cosmos data
	/// </summary>
	private readonly CosmosClient _client;

	/// <summary>
	/// The Id of a database being arged or worked with
	/// </summary>
	private readonly string _databaseId;

	/// <summary>
	/// Holds object types by their container name
	/// </summary>
	private readonly ConcurrentDictionary<Type, string> _typeContainerId = new();


	/// <summary>
	/// Gets the <see cref="CosmosClient"/> that was created during <see cref="Initialize(string, string, string, CosmosPropertyNamingPolicy)"/>
	/// <para>*ONLY* use the client directly if any of the rest of the context methods are not sufficient</para>
	/// </summary>
	public CosmosClient Client { get => _client; }

	/// <summary>
	/// Prevents a default instance of the <see cref="Context" /> class from being created
	/// </summary>
	/// <param name="client">Client that will be used for all further data operations</param>
	/// <param name="databaseId">Id of the database being used in cosmos</param>
	private Context(CosmosClient client, string databaseId) {
		_client = client;
		_databaseId = databaseId;
	}

	/// <summary>
	/// Initializes a new <see cref="Context"/> instance and prepares it for use regarding data operations
	/// </summary>
	/// <param name="endpoint">Account endpoint to use for data access</param>
	/// <param name="auth">Auth key to use for data access</param>
	/// <param name="databaseId">Database Id to use for data access</param>
	/// <param name="namingPolicy">Naming policy to use.</param>
	/// <returns>A <see cref="Context"/> ready for use</returns>
	public static Context Initialize(string endpoint, string auth, string databaseId, CosmosPropertyNamingPolicy namingPolicy = CosmosPropertyNamingPolicy.CamelCase) {
		return new Context(
			new CosmosClient(
				endpoint,
				auth,
				new CosmosClientOptions {
					EnableContentResponseOnWrite = false,
					SerializerOptions = new CosmosSerializationOptions {
						PropertyNamingPolicy = namingPolicy
					}
				}),
			databaseId);
	}

	/// <summary>
	/// Creates the given <paramref name="model"/> in backing storage. Validated before transmission.
	/// </summary>
	/// <typeparam name="T">The type of model being created</typeparam>
	/// <param name="model">The model to commit to storage</param>
	/// <returns>An ItemResponse *WITHOUT* the created .Resource property set (i.e. headers only)</returns>
	public async Task<ItemResponse<T>> Create<T>(T model) where T : BaseModel {
		Assert.IsValid(model);
		return await GetContainerByType(model).CreateItemAsync(model).ConfigureAwait(false);
	}

	/// <summary>
	/// Deletes a data model from the data store
	/// </summary>
	/// <typeparam name="T">Type of model to delete</typeparam>
	/// <param name="id">Unique Id of the model to delete</param>
	/// <param name="pkey">The partition key to use for locating the corresponding <paramref name="id"/></param>
	/// <returns>An ItemResponse *WITHOUT* the created .Resource property set (i.e. headers only)</returns>
	public async Task<ItemResponse<T>> Delete<T>(string id, PartitionKey pkey) where T : BaseModel {
		return await GetContainerByType<T>().DeleteItemAsync<T>(id, pkey).ConfigureAwait(false);
	}


	/// <summary>
	/// Returns a single model given an id and partition key.
	/// </summary>
	/// <typeparam name="T">Type of data model to return</typeparam>
	/// <param name="id">Unique identifier to retrieve data by</param>
	/// <param name="pkey">Partition key to use for search -- if null, then the id is used as the partition key</param>
	/// <returns>A <typeparamref name="T"/> object</returns>
	public async Task<T> Get<T>(string id, PartitionKey pkey) where T : BaseModel => (await GetContainerByType<T>().ReadItemAsync<T>(id, pkey).ConfigureAwait(false)).Resource;

	/// <summary>
	/// Performs a remote search for data models that are of a given type and with the passed predicate. Cross partition queries are enabled by default, but it is recommended
	/// that if possible, you include the partition key in your search
	/// </summary>
	/// <typeparam name="T">Type of model data to search for and return</typeparam>
	/// <param name="predicate">Predicate used to determine which data should be returned from the remote server. Cosmos restrictions apply.</param>
	/// <returns>A <see cref="List{T}"/> of results. May be empty but will never be null</returns>
	public async Task<List<T>> Get<T>(Expression<Func<T, bool>> predicate) where T : BaseModel { // PASTA Make 'select' version to allow for partial objects
		using var fIterator =
			GetContainerByType<T>()
			.GetItemLinqQueryable<T>()
			.Where(predicate)
			.ToFeedIterator();

		List<T> toReturn = [];

		//Asynchronous query execution
		while (fIterator.HasMoreResults) {
			foreach (T? item in await fIterator.ReadNextAsync().ConfigureAwait(false)) {
				toReturn.Add(item);
			}
		}

		return toReturn;
	}

	/// <summary>
	/// Performs a remote search for data models that are of a given type and with the passed predicate. Cross partition queries are enabled by default, but it is recommended
	/// that if possible, you include the partition key in your search.
	/// Of the objects returned, select to given <typeparamref name="S"/> type during deserialization of results (Cosmos restrictions apply)
	/// </summary>
	/// <typeparam name="T">Type of model data to search for and return</typeparam>
	/// <typeparam name="S">An function / expression to use for the returned data. Cosmos restrictions apply</typeparam>
	/// <param name="predicate">Predicate used to determine which data should be returned from the remote server. Cosmos restrictions apply.</param>
	/// <param name="selector">The selector to use for objects that match <paramref name="predicate"/>. Cosmos restrictions apply.</param>
	/// <returns>A <see cref="List{S}"/> of results. May be empty but will never be null</returns>
	public async Task<List<S>> Get<T, S>(Expression<Func<T, bool>> predicate, Expression<Func<T, S>> selector) where T : BaseModel {
		using var fIterator =
			GetContainerByType<T>()
			.GetItemLinqQueryable<T>()
			.Where(predicate)
			.Select(selector)
			.ToFeedIterator();

		List<S> toReturn = [];

		//Asynchronous query execution
		while (fIterator.HasMoreResults) {
			foreach (S? item in await fIterator.ReadNextAsync().ConfigureAwait(false)) {
				toReturn.Add(item);
			}
		}

		return toReturn;
	}

	/// <summary>
	/// Performs a remote search for data models that are of a given type and with the passed predicate. Cross partition queries are enabled by default, but it is recommended
	/// that if possible, you include the partition key in your search.
	/// Of the objects returned, select to given <typeparamref name="S"/> type during deserialization of results (Cosmos restrictions apply)
	/// A final projection is run when iterating through results from the feed using <paramref name="projector"/>
	/// </summary>
	/// <typeparam name="T">Type of model data to search for and return</typeparam>
	/// <typeparam name="S">An function / expression to use for the returned data. Cosmos restrictions apply</typeparam>
	/// <typeparam name="P">The type to project selected objects to post feed iteration</typeparam>
	/// <param name="predicate">Predicate used to determine which data should be returned from the remote server. Cosmos restrictions apply.</param>
	/// <param name="selector">The selector to use for objects that match <paramref name="predicate"/>. Cosmos restrictions apply.</param>
	/// <param name="projector">Function to use for projection of selected data to a final type</param>
	/// <returns>A <see cref="List{P}"/> of results. May be empty but will never be null</returns>
	public async Task<List<P>> Get<T, S, P>(Expression<Func<T, bool>> predicate, Expression<Func<T, S>> selector, Func<S, P> projector) where T : BaseModel {
		using var fIterator =
			GetContainerByType<T>()
			.GetItemLinqQueryable<T>()
			.Where(predicate)
			.Select(selector)
			.ToFeedIterator();

		List<P> toReturn = [];

		//Asynchronous query execution
		while (fIterator.HasMoreResults) {
			foreach (S? item in await fIterator.ReadNextAsync().ConfigureAwait(false)) {
				toReturn.Add(projector(item));
			}
		}

		return toReturn;
	}


	/// <summary>
	/// For a given type, the CollectionId is assumed to be the name of <typeparamref name="T"/> 's actual type
	/// </summary>
	/// <typeparam name="T">Type parameter to use for retrieving a collection name</typeparam>
	/// <returns>A string which is typically typeof(T).Name</returns>
	public Container GetContainerByType<T>(T? arg = null) where T : BaseModel {
		Type type = typeof(T);

		if (!_typeContainerId.ContainsKey(type))
			_typeContainerId.TryAdd(type, (arg ?? Activator.CreateInstance<T>()).GetContainerId());

		return _client.GetContainer(_databaseId, _typeContainerId[type]);
	}

	/// <summary>
	/// Commits an update of a model to Cosmos storage. The <paramref name="model"/> is validated before storage.
	/// <para>Make sure the model is in the correct state regarding its partition key before updating</para>
	/// </summary>
	/// <typeparam name="T">The type of model being updated</typeparam>
	/// <param name="model">The model to update in Cosmos storage</param>
	/// <returns>An ItemResponse *WITHOUT* the created .Resource property set (i.e. headers only)</returns>
	public async Task<ItemResponse<T>> Update<T>(T model) where T : BaseModel {
		Assert.IsValid(model);

		return await GetContainerByType(model).ReplaceItemAsync(model, model.Id).ConfigureAwait(false);
	}
}
