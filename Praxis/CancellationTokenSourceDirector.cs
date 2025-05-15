namespace Praxis;

using System.Collections.Concurrent;

/// <summary>
/// Class used for handling cancellation [token] sources and addition, removal and disposal of those sources
/// </summary>
/// <remarks>
/// Utilizes a <see cref="ConcurrentDictionary{TKey, TValue}"/> for internal storage
/// <para>Methods available to this class are thread safe</para>
/// </remarks>
/// <typeparam name="T">Type used as key in the internal <see cref="ConcurrentDictionary{TKey, TValue}"/></typeparam>
public sealed class CancellationTokenSourceDirector<T> where T : notnull {
	/// <summary>
	/// Used for holding identifiers against cancellation tokens and sources
	/// </summary>
	private readonly ConcurrentDictionary<T, CancellationTokenSource> _ctsDict = [];

	/// <summary>
	/// Gets a token by a source if it exists, otherwise a new source is created and its token returned
	/// </summary>
	/// <param name="id">ID used for addition/retrieval</param>
	/// <returns><see cref="CancellationToken"/></returns>
	public CancellationToken GetAdd(T id) => _ctsDict.GetOrAdd(id, new CancellationTokenSource()).Token;


	/// <summary>
	/// Removes all current entries and disposes them, optionally calling cancel
	/// </summary>
	/// <param name="cancel">Indicates to call cancel on the sources</param>
	public void RemoveDispose(bool cancel) {
		foreach (var key in _ctsDict.Keys)
			TryRemoveDispose(key, cancel);
	}

	/// <summary>
	/// Attempts to removes the <see cref="CancellationTokenSource"/> from the internal dictionary, and if it exists, disposes of it
	/// </summary>
	/// <param name="id">Identifier to use for looking up a <see cref="CancellationTokenSource"/></param>
	/// <param name="cancel">Whether to call cancel on the <see cref="CancellationTokenSource"/> before disposal</param>
	/// <returns><see langword="true"/> if an entry was found and the operations performed, otherwise false</returns>
	public bool TryRemoveDispose(T id, bool cancel) {
		if (_ctsDict.TryRemove(id, out var cts)) {
			using (cts) {
				if (cancel)
					cts.Cancel();

				return true;
			}
		}

		return false;
	}
}