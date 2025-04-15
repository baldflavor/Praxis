namespace Praxis.Cache;

using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Extensions for working with <see cref="IMemoryCache"/> instances
/// </summary>
public static class Cow {

	/// <summary>
	/// MasterLock class used for generating locks when accessing cached information
	/// </summary>
	private static readonly MasterLock _masterLock = new();

	/// <summary>
	/// Removes all items from the cache if the passed interface is of type <see cref="MemoryCache"/>. If it is not, then no clearing operation will be performed. Check the
	/// return value to determine whether or not the cache could be cleared
	/// </summary>
	/// <returns>True if the cache could be cleared, otherwise false</returns>
	public static bool Clear(this IMemoryCache cache) {
		if (cache is MemoryCache mc) {
			mc.Clear();
			return true;
		}

		return false;
	}

	/// <summary>
	/// Retrieves data that utilizes memory caching
	/// </summary>
	/// <typeparam name="T">Type of data that will be retrieved</typeparam>
	/// <param name="cache">Used for memory caching capability</param>
	/// <param name="key">The key to use for cache access</param>
	/// <param name="generator">Function used for generating data when not found in the cache</param>
	/// <param name="entryOptions"><see cref="MemoryCacheEntryOptions"/> used for controlling entries placed in the cache</param>
	/// <param name="postGenerate">Optional function ran after data generation and cache insertion when data was not found in the cache</param>
	/// <returns><typeparamref name="T"/></returns>
	public static T? Retrieve<T>(this IMemoryCache cache, string key, Func<T> generator, MemoryCacheEntryOptions entryOptions, Func<T, T>? postGenerate = null) {
		if (cache.TryGetValue<T>(key, out T? preCheckDataInCache))
			return preCheckDataInCache;

		lock (_masterLock[key]) {
			if (cache.TryGetValue<T>(key, out T? dataInCache))
				return dataInCache;

			T newData = generator();
			cache.Set<T>(key, newData, entryOptions);

			if (postGenerate == null)
				return newData;
			else
				return postGenerate(newData);
		}
	}

	/// <summary>
	/// Retrieves data that utilizes memory caching
	/// </summary>
	/// <typeparam name="T">Type of data that will be retrieved</typeparam>
	/// <param name="cache">Used for memory caching capability</param>
	/// <param name="key">The key to use for cache access</param>
	/// <param name="generator">Function used for generating data when not found in the cache</param>
	/// <param name="entryOptions"><see cref="MemoryCacheEntryOptions"/> used for controlling entries placed in the cache</param>
	/// <param name="postGenerate">Optional function ran after data generation and cache insertion when data was not found in the cache</param>
	/// <returns><see cref="Task{T}"/> of <typeparamref name="T"/></returns>
	public static async Task<T?> RetrieveAsync<T>(this IMemoryCache cache, string key, Func<Task<T>> generator, MemoryCacheEntryOptions entryOptions, Func<T, T>? postGenerate = null) {
		if (cache.TryGetValue<T>(key, out T? preCheckDataInCache))
			return preCheckDataInCache;

		SemaphoreSlim semSlimLock = _masterLock.Slim(key);
		try {
			await semSlimLock.WaitAsync().ConfigureAwait(false);

			if (cache.TryGetValue<T>(key, out T? dataInCache))
				return dataInCache;

			T newData = await generator().ConfigureAwait(false);
			cache.Set<T>(key, newData, entryOptions);

			if (postGenerate == null)
				return newData;
			else
				return postGenerate(newData);
		}
		finally {
			semSlimLock.Release();
		}
	}
}