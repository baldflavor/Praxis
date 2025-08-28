namespace Praxis.WinForm;

/// <summary>
/// Class used for executing a filter against a set of data that executes after a period of time when a user is no longer
/// entering filter characters or values.
/// </summary>
/// <typeparam name="T">Type of data that is stored / used for filtering.</typeparam>
/// <param name="data">Instance of data held and used for filtering operations / return.</param>
/// <param name="dataFilteredDelegate">Delegate that is called when a filtered subset of data is available.</param>
internal abstract class FilterExecutor<T>(T data, Action<T> dataFilteredDelegate) : IDisposable where T : System.Collections.ICollection {
	/// <summary>
	/// Source used for signalling cancellation that provides a delay from rapid firing the filter.
	/// </summary>
	private CancellationTokenSource _cts = new();

	/// <summary>
	/// Used for course locking when filter is changed.
	/// </summary>
	private readonly object _lock = new();

	/// <summary>
	/// Method that should perform filtering on the contained data and return whatever the subset is that satisfies said filter.
	/// </summary>
	/// <param name="cTok"><c>Token</c> that should be used to detect cancellation.</param>
	/// <returns>The result of <see cref="_filter"/> run against <c>data</c>.</returns>
	/// <exception cref="OperationCanceledException">This can and <b>should</b> be thrown if cancellation is detected.</exception>
	protected abstract T ReturnFilteredData(CancellationToken cTok);

	/// <summary>
	/// Original set of data - should not be altered directly.
	/// </summary>
	protected readonly T _data = data;

	/// <summary>
	/// String that should be used as the entered filter as desired by a user.
	/// </summary>
	protected string _filter = "";

	/// <summary>
	/// Indicates whether this class (value) has been disposed.
	/// </summary>
	private bool _disposedValue;

	/// <summary>
	/// The delay to use between changes in <see cref="_filter"/> before triggering actual filtering of data.
	/// </summary>
	public int MillisecondDelay { get; set; } = 650;

	/// <summary>
	/// Code for when the filter has been changed / should be set.
	/// </summary>
	/// <param name="arg">Argument to set as the new filter value.</param>
	public void FilterChange(string arg) {
		lock (_lock) {
			_cts.Cancel();
			_cts.Dispose();

			_filter = arg;
			_cts = new CancellationTokenSource();
			_ = _DelayedRun(_cts.Token);
		}

		async Task _DelayedRun(CancellationToken cTok) {
			try {
				await Task.Delay(this.MillisecondDelay, cTok).ConfigureAwait(false);
				if (!cTok.IsCancellationRequested)
					dataFilteredDelegate(string.IsNullOrWhiteSpace(_filter) ? _data : ReturnFilteredData(cTok));
			}
			catch (OperationCanceledException) {
				//Intentionally empty - cancellation / token will throw on cancellation
			}
		}
	}


	/// <summary>
	/// Handles disposing of managed resources.
	/// </summary>
	/// <param name="disposing">Whether the call is from <see cref="Dispose()"/> or finalizer.</param>
	protected virtual void Dispose(bool disposing) {
		if (!_disposedValue && disposing) {
			_cts?.Dispose();
			_disposedValue = true;
		}
	}


	/// <inheritdoc/>
	public void Dispose() {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
