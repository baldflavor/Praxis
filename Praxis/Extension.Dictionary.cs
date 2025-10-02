namespace Praxis;

using System.Collections.Generic;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Enumerates through keys and values in an <c>IDictionary</c> where they are <c>OfType&lt;IDisposable&gt;</c> and calls
	/// <c>Dispose()</c> on each.<br />
	/// Then the dictionary is cleared.
	/// </summary>
	/// <typeparam name="K">Type of key in the dictionary.</typeparam>
	/// <typeparam name="V">Type of values in the dictionary.</typeparam>
	/// <param name="arg">Dictionary to operate upon.</param>
	public static void ClearDispose<K, V>(IDictionary<K, V> arg) {
		foreach (var iDisposableKey in arg.Keys.OfType<IDisposable>())
			iDisposableKey.Dispose();

		foreach (var iDisposableValue in arg.Values.OfType<IDisposable>())
			iDisposableValue.Dispose();

		arg.Clear();
	}

	/// <summary>
	/// Attempts to retrieve a key / value and supplies a value if it does not exist.
	/// </summary>
	/// <remarks>
	/// Course <c>lock</c> applied over <paramref name="arg"/> during execution.
	/// </remarks>
	/// <typeparam name="K">Type of Key</typeparam>
	/// <typeparam name="V">Type of Value</typeparam>
	/// <param name="arg">Target dictionary to check</param>
	/// <param name="key">Key to use for value lookup</param>
	/// <param name="value">Value to add / return when not found by key</param>
	/// <returns>A <typeparamref name="V"/> value</returns>
	public static V GetOrAdd<K, V>(this IDictionary<K, V> arg, K key, V value) {
		lock (arg) {
			if (arg.TryGetValue(key, out V? v))
				return v;

			arg[key] = value;
			return value;
		}
	}

	/// <summary>
	/// Attempts to retrieve a key / value and generates a value if it does not exist
	/// </summary>
	/// <remarks>
	/// Course <c>lock</c> applied over <paramref name="arg"/> during execution.
	/// </remarks>
	/// <typeparam name="K">Type of Key</typeparam>
	/// <typeparam name="V">Type of Value</typeparam>
	/// <param name="arg">Target dictionary to check</param>
	/// <param name="key">Key to use for value lookup</param>
	/// <param name="generator">Function used to generate a value when not found by key</param>
	/// <returns>A <typeparamref name="V"/> value</returns>
	public static V GetOrAdd<K, V>(this IDictionary<K, V> arg, K key, Func<V> generator) {
		lock (arg) {
			if (arg.TryGetValue(key, out V? v))
				return v;

			V toReturn = generator();
			arg[key] = toReturn;

			return toReturn;
		}
	}

	/// <summary>
	/// Attempts to retrieve a key / value and generates a value if it does not exist
	/// </summary>
	/// <remarks>
	/// If <paramref name="semSlimKey"/> is provided, then locked during execution.
	/// </remarks>
	/// <typeparam name="K">Type of Key</typeparam>
	/// <typeparam name="V">Type of Value</typeparam>
	/// <param name="arg">Target dictionary to check</param>
	/// <param name="key">Key to use for value lookup</param>
	/// <param name="generator">Function used to generate a value when not found by key</param>
	/// <param name="semSlimKey">Key to use for locking access to this dictionary in multithreaded scenarios when necessary</param>
	/// <returns>A <typeparamref name="V"/> value</returns>
	public static async Task<V> GetOrAdd<K, V>(this IDictionary<K, V> arg, K key, Func<Task<V>> generator, string? semSlimKey = default) {
		SemaphoreSlim? semSlim;
		if (semSlimKey is null)
			semSlim = null;
		else
			semSlim = MasterLock.Instance.Slim(semSlimKey);

		try {
			if (semSlim != null)
				await semSlim.WaitAsync().ConfigureAwait(false);

			if (arg.TryGetValue(key, out V? v))
				return v;

			V toReturn = await generator().ConfigureAwait(false);
			arg[key] = toReturn;

			return toReturn;
		}
		finally {
			semSlim?.Release();
		}
	}

	/// <summary>
	/// Adds or Sets (if it already exists) the passed KeyValuePair to an existing Dictionary
	/// </summary>
	/// <typeparam name="K">Type of key</typeparam>
	/// <typeparam name="V">Type of value</typeparam>
	/// <param name="arg">Dictionary being altered</param>
	/// <param name="toAdd">KeyValuePair to use for parameters</param>
	/// <returns>The <paramref name="arg"/> Dictionary</returns>
	public static Dictionary<K, V> SetKvp<K, V>(this Dictionary<K, V> arg, KeyValuePair<K, V> toAdd) where K : notnull {
		arg[toAdd.Key] = toAdd.Value;
		return arg;
	}
}
