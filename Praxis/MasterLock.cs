namespace Praxis;

/// <summary>
/// Class used for providing rapid access to a battery of simple objects that can be used in locking scenarios
/// </summary>
public sealed class MasterLock {
	/// <summary>
	/// Provides static access to a lazily instantiated MasterLock object
	/// </summary>
	private static readonly Lazy<MasterLock> _instance = new();


	/// <summary>
	/// Holds a dictionary of locks by string key
	/// </summary>
	private readonly Dictionary<string, object> _lockDict = [];

	/// <summary>
	/// Lock for the lock dictionary
	/// </summary>
	private readonly object _lockDict_Lock = new();

	/// <summary>
	/// Holds a dictionary of SemaphoreSlim by string key
	/// </summary>
	private readonly Dictionary<string, SemaphoreSlim> _lockDictSlim = [];

	/// <summary>
	/// Gets a static instance of the MasterLock class
	/// </summary>
	public static MasterLock Instance => _instance.Value;


	/// <summary>
	/// Gets a lock for use against the passed key
	/// </summary>
	/// <param name="key">An object belonging to the passed key</param>
	/// <returns>An object</returns>
	public object this[string key] {
		get {
			if (!_lockDict.ContainsKey(key)) {
				lock (_lockDict_Lock) {
					if (!_lockDict.ContainsKey(key))
						_lockDict[key] = new object();
				}
			}

			return _lockDict[key];
		}
	}

	/// <summary>
	/// Locks while an action is being performed
	/// </summary>
	/// <param name="key">The key to use while locking</param>
	/// <param name="action">The action to perform</param>
	/// <param name="masterLock">Masterlock instance to use. When <c>null</c> uses <see cref="MasterLock.Instance"/>.</param>
	public static void LockWhile(string key, Action action, MasterLock? masterLock = null) {
		lock ((masterLock ?? MasterLock.Instance)[key]) {
			action();
		}
	}

	/// <summary>
	/// Locks while an action is being performed
	/// </summary>
	/// <param name="key">The key to use while locking</param>
	/// <param name="func">The function to perform and result returned</param>
	/// <param name="masterLock">Masterlock instance to use. When <c>null</c> uses <see cref="MasterLock.Instance"/>.</param>
	public static T LockWhile<T>(string key, Func<T> func, MasterLock? masterLock = null) {
		lock ((masterLock ?? MasterLock.Instance)[key]) {
			return func();
		}
	}

	/// <summary>
	/// Locks while an action is being performed
	/// </summary>
	/// <param name="key">The key to use while locking</param>
	/// <param name="action">The action to perform</param>
	/// <param name="masterLock">Masterlock instance to use. When <c>null</c> uses <see cref="MasterLock.Instance"/>.</param>
	/// <returns>An awaitable task</returns>
	public static async Task LockWhileAsync(string key, Func<Task> action, MasterLock? masterLock = null) {
		SemaphoreSlim semSlimLock = (masterLock ?? MasterLock.Instance).Slim(key);
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
	/// <param name="masterLock">Masterlock instance to use. When <c>null</c> uses <see cref="MasterLock.Instance"/>.</param>
	/// <returns>An awaitable task</returns>
	public static async Task<T> LockWhileAsync<T>(string key, Func<Task<T>> func, MasterLock? masterLock = null) {
		SemaphoreSlim semSlimLock = (masterLock ?? MasterLock.Instance).Slim(key);
		try {
			await semSlimLock.WaitAsync().ConfigureAwait(false);
			return await func().ConfigureAwait(false);
		}
		finally {
			semSlimLock.Release();
		}
	}

	/// <summary>
	/// Gets a lock for use against the passed key
	/// </summary>
	/// <param name="key">A SemaphoreSlim configured for single access belonging to the passed key</param>
	/// <returns>A SemaphoreSlim(1,1)</returns>
	public SemaphoreSlim Slim(string key) {
		if (!_lockDictSlim.ContainsKey(key)) {
			lock (_lockDict_Lock) {
				if (!_lockDictSlim.ContainsKey(key))
					_lockDictSlim[key] = new SemaphoreSlim(1, 1);
			}
		}

		return _lockDictSlim[key];
	}

	/// <summary>
	/// Simple object that can be used both to "lock" upon and also to hold a key that can be used for
	/// semaphore or related operations 
	/// </summary>
	public class LockNKey(string? key = null) {
		/// <summary>
		/// Holds a string reference to a value that represents the key for this lock
		/// </summary>
		private readonly string _key = key ?? Crypto.Token.GuidAlpha();

		/// <summary>
		/// Gets the key associate with this instance
		/// </summary>
		public string Key => _key;
	}
}
