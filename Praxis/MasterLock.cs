namespace Praxis;

using System.Collections.Concurrent;

/// <summary>
/// Class used for providing rapid access to a battery of simple objects that can be used in locking scenarios
/// </summary>
public sealed class MasterLock<T> where T : notnull {
	/// <summary>
	/// Provides static access to a lazily instantiated MasterLock object
	/// </summary>
	private static readonly Lazy<MasterLock<T>> _instance = new();


	/// <summary>
	/// Holds a dictionary of locks by string key
	/// </summary>
	private readonly ConcurrentDictionary<T, LockKey<T>> _lockDict = [];

	/// <summary>
	/// Holds a dictionary of SemaphoreSlim by string key
	/// </summary>
	private readonly ConcurrentDictionary<T, SemaphoreSlim> _lockDictSlim = [];

	/// <summary>
	/// Gets a static instance of the MasterLock class
	/// </summary>
	public static MasterLock<T> Instance => _instance.Value;


	/// <summary>
	/// Gets a lock for use against the passed key
	/// </summary>
	/// <param name="key">An object belonging to the passed key</param>
	/// <returns>An object</returns>
	public LockKey<T> this[T key] => _lockDict.GetOrAdd(key, () => new LockKey<T>(key));

	/// <summary>
	/// Locks while an action is being performed
	/// </summary>
	/// <param name="key">The key to use while locking</param>
	/// <param name="action">The action to perform</param>
	/// <param name="masterLock">Masterlock instance to use. When <c>null</c> uses <see cref="MasterLock{T}.Instance"/>.</param>
	public static void LockWhile(T key, Action action, MasterLock<T>? masterLock = null) {
		lock ((masterLock ?? MasterLock<T>.Instance)[key]) {
			action();
		}
	}

	/// <summary>
	/// Locks while an action is being performed
	/// </summary>
	/// <param name="key">The key to use while locking</param>
	/// <param name="func">The function to perform and result returned</param>
	/// <param name="masterLock">Masterlock instance to use. When <c>null</c> uses <see cref="MasterLock{T}.Instance"/>.</param>
	public static R LockWhile<R>(T key, Func<R> func, MasterLock<T>? masterLock = null) {
		lock ((masterLock ?? MasterLock<T>.Instance)[key]) {
			return func();
		}
	}

	/// <summary>
	/// Locks while an action is being performed
	/// </summary>
	/// <param name="key">The key to use while locking</param>
	/// <param name="action">The action to perform</param>
	/// <param name="masterLock">Masterlock instance to use. When <c>null</c> uses <see cref="MasterLock{T}.Instance"/>.</param>
	/// <returns>An awaitable task</returns>
	public static async Task LockWhileAsync(T key, Func<Task> action, MasterLock<T>? masterLock = null) {
		SemaphoreSlim semSlimLock = (masterLock ?? MasterLock<T>.Instance).Slim(key);
		try {
			await semSlimLock.WaitAsync().ConfigureAwait(false);
			await action().ConfigureAwait(false);
		}
		finally {
			semSlimLock.Release();
		}
	}

	/// <summary>
	/// Locks while an action is being performed
	/// </summary>
	/// <param name="key">The key to use while locking</param>
	/// <param name="func">The function to perform and result returned</param>
	/// <param name="masterLock">Masterlock instance to use. When <c>null</c> uses <see cref="MasterLock{T}.Instance"/>.</param>
	/// <returns>An awaitable task</returns>
	public static async Task<R> LockWhileAsync<R>(T key, Func<Task<R>> func, MasterLock<T>? masterLock = null) {
		SemaphoreSlim semSlimLock = (masterLock ?? MasterLock<T>.Instance).Slim(key);
		try {
			await semSlimLock.WaitAsync().ConfigureAwait(false);
			return await func().ConfigureAwait(false);
		}
		finally {
			semSlimLock.Release();
		}
	}

	/// <summary>
	/// Returns a single entrant SemaphoreSlim.
	/// </summary>
	/// <remarks>
	/// Returned instance is: <c>SemaphoreSlim(1,1)</c>.
	/// </remarks>
	/// <param name="key">A SemaphoreSlim configured for single access belonging to the passed key.</param>
	/// <returns>SemaphoreSlim</returns>
	public SemaphoreSlim Slim(T key) => _lockDictSlim.GetOrAdd(key, () => new(1, 1));
}


/// <summary>
/// Simple object that can be used both to "lock" upon and also to hold a key that can be used for
/// semaphore or related operations 
/// </summary>
public record class LockKey<T>(T Key);
