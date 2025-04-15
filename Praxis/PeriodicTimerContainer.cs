namespace Praxis;

using System;
using System.Threading.Tasks;

/// <summary>
/// Used to contain the components needed for hosting a <see cref="PeriodicTimer"/> that can be used for processing background
/// operations in an asynchronous fashion in a specified interval. See the <b>remarks</b> section below and the comments on
/// individual methods for implementation.
/// </summary>
/// <remarks>
/// Note that the <see cref="_tickFrequency"/> is the maximum frequency; if the code in <see cref="Tick(CancellationToken)"/> takes longer than this
/// elapsed time period, it will not double fire, but makes no guarantee that it will fire "as fast" as the frequency.
/// <para>Make sure that any <see cref="Exception"/> that may be thrown in your implementation of <see cref="Tick(CancellationToken)"/> are handled, otherwise...</para>
/// <para>Exceptions that bubble up from that method will cause the <i>internal polling loop</i> to <b>short circuit and stop</b>. This will also cause the <c>CancellationToken</c>
/// passed to <see cref="Tick(CancellationToken)"/> to <b>request cancellation</b>.</para>
/// <para>Those exceptions will also <i><b>not</b></i> bubble outside of the containing method, so an implementer is responsible for logging / handling exceptional circumstances</para>
/// <para>When the <see cref="Stop"/> or <see cref="Dispose"/> methods are called, they internally call <see cref="CancellationTokenSource.Cancel()"/>
/// on the token used for controlling the lifetime of the polling loop. This does not guarantee an immediate cessation of all operations as the
/// cancel signal may need to propagate through various subscribers, or at all if the token is not passed to other asynchronous operations those operations</para>
/// <para>In this sense, be cognizant and aware of tasks/timing/exceptions when implementing this class</para>
/// </remarks>
public abstract class PeriodicTimerContainer : IDisposable {
	/// <summary>
	/// Object that can be used for locking purposes by operations that may be accessed by both <see cref="PeriodicTimerContainer.Tick(CancellationToken)"/>
	/// and other implemented methods
	/// </summary>
	protected readonly object _lock = new();

	/// <summary>
	/// Indicates whether the object is disposed
	/// </summary>
	protected bool _isDisposed;

	/// <summary>
	/// The timing period to use for each tick/loop on <see cref="_pTimer"/>
	/// </summary>
	private readonly TimeSpan _tickFrequency;

	/// <summary>
	/// When the polling loop is executed (after calling <see cref="Start"/>), if this value is not <see langword="null"/>
	/// then delay for this period of time and execute <see cref="Tick(CancellationToken)"/> before resuming the regular polling period
	/// </summary>
	private readonly TimeSpan? _tickOnStartSpecialDelay;

	/// <summary>
	/// Cancellation token source used for signalling cancellation to <see cref="_pTimerTask"/>
	/// </summary>
	private CancellationTokenSource? _cts;

	/// <summary>
	/// Action delegate that is used for invocation in <see cref="_PollingLoop"/> when an <see cref="Exception"/> is thrown outside of
	/// cancellation. It is called after the end of the polling loop block (after the cancellation token has been cancelled and disposed)
	/// so that <see cref="Start"/> can be attempted; but still
	/// </summary>
	private Action<Exception>? _pollingLoopOnException;


	/// <summary>
	/// Initializes a new instance of the <see cref="PeriodicTimerContainer" /> class
	/// </summary>
	/// <param name="tickFrequency">Period of time that is used to (approximate) between execution of the <see cref="Tick(CancellationToken)"/> method</param>
	/// <param name="tickOnStartSpecialDelay">When the polling loop is executed (after calling <see cref="Start"/>), if this value is not <see langword="null"/>,
	/// then delay for this value and then execute <see cref="Tick(CancellationToken)"/>. Must be greater than <see cref="TimeSpan.Zero"/></param>
	/// <param name="pollingLoopOnExceptionDelegate">Used during the polling loop: exceptions that are not cancellation related will be invoked against this delegate</param>
	public PeriodicTimerContainer(TimeSpan tickFrequency, TimeSpan? tickOnStartSpecialDelay = null, Action<Exception>? pollingLoopOnExceptionDelegate = default) {
		Assert.GreaterThanZero(tickFrequency);

		if (tickOnStartSpecialDelay != null)
			Assert.GreaterThanZero(tickOnStartSpecialDelay.Value);

		_tickFrequency = tickFrequency;
		_tickOnStartSpecialDelay = tickOnStartSpecialDelay;
		_pollingLoopOnException = pollingLoopOnExceptionDelegate;
	}


	/// <summary>
	/// Used to clear any collections / other conditions. Beware of concurrency issues when performing operations given that
	/// <see cref="Tick(CancellationToken)"/> may still be in the process of accessing or reading values
	/// </summary>
	/// <remarks>Default implementation performs no action</remarks>
	public virtual void Clear() { }

	/// <summary>
	/// Dispose of resources held by the class, suppress finalization
	/// </summary>
	public void Dispose() {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Starts the polling loop and will begin firing the <see cref="Tick(CancellationToken)"/> method
	/// at intervals
	/// </summary>
	/// <returns><see langword="this"/></returns>
	public PeriodicTimerContainer Start() {
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		if (_cts != null)
			return this;

		_cts = new CancellationTokenSource();
		_ = _PollingLoop();
		return this;
	}

	/// <summary>
	/// Stops <see cref="PeriodicTimer"/> polling
	/// </summary>
	/// <remarks>
	/// Note that this does not guarantee that all operations are immediately stopped, as transient operations that rely
	/// on <see cref="_cts"/> (<see cref="CancellationTokenSource"/>) will all propagate back to the initial pooling loop before resources
	/// are disposed.
	/// <para>Call <see cref="Start"/> to restart polling and hits to the <see cref="Tick(CancellationToken)"/> method</para></remarks>
	/// <param name="clear">Whether to call the <see cref="Clear"/> method after signalling stop</param>
	public void Stop(bool clear = false) {
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		_cts?.Cancel();

		if (clear)
			Clear();
	}


	/// <summary>
	/// Dispose of resources held by the class
	/// </summary>
	/// <remarks>If not disposed and disposing, called <see cref="Stop(bool)"/> passing <see langword="true"/></remarks>
	/// <param name="disposing">If the call is from the <see cref="Dispose()"/> method or from finalization</param>
	protected virtual void Dispose(bool disposing) {
		if (!_isDisposed) {
			if (disposing) {
				Stop(true);
				_pollingLoopOnException = null;
			}

			_isDisposed = true;
		}
	}

	/// <summary>
	/// Code to run for each tick of the polling loop provided by the <see cref="PeriodicTimer"/>
	/// </summary>
	/// <remarks>
	/// Be cognizant of putting long running operations in this method vs the <see cref="_tickFrequency"/>
	/// <para>Take care to catch exceptions thrown by this method as it will otherwise break out of the polling loop and effectively cause
	/// the class to be in a <b>stopped</b> state. (Which may be desired in the case of OperationCanceledException, etc)</para>
	/// <para>You may wish to include the following in your implementation to prevent some of the aforementioned "loop breaking" if operations have their own cancellation tokens / timing:</para>
	/// <code>
	/// try {
	/// } catch (OperationCanceledException) {
	///    //Catch these exceptions / discard so that cancellation messages do not throw out from this method altogether
	///    // Log / etc
	/// } catch (Exception ex) {
	///    // Log / throw etc depending
	/// }
	/// </code>
	/// </remarks>
	/// <param name="cToken">Token that should be passed to other asynchronous operations in order for <see cref="Stop"/> to propagate effectively</param>
	/// <returns><see cref="Task"/></returns>
	protected abstract Task Tick(CancellationToken cToken);



	/// <summary>
	/// Handles the creation of and loop that triggers the <see cref="Tick(CancellationToken)"/> method.
	/// <para>Creates a new <see cref="PeriodicTimer"/> for ticking</para>
	/// <para>Exceptions thrown will break out of / terminate the polling loop</para>
	/// <para>On exit of the polling loop, <see cref="CancellationTokenSource.Cancel()"/> is called on <see cref="_cts"/>,
	/// and then it is <b>disposed and set to <see langword="null"/></b></para>
	/// </summary>
	/// <returns><see cref="Task"/></returns>
	/// <exception cref="ObjectDisposedException">Thrown if this object is disposed when entering the method</exception>
	/// <exception cref="InvalidOperationException">Thrown if <see cref="_cts"/> is null</exception>
	/// <exception cref="Exception">May be thrown if an operation in the <see cref="Tick(CancellationToken)"/> method throws an exception</exception>
	private async Task _PollingLoop() {
		ObjectDisposedException.ThrowIf(_isDisposed, this);
		CancellationToken token = _cts?.Token ?? throw new InvalidOperationException("Cancellation Token Source cannot be null when calling this method");

		Exception? loopException = null;
		try {
			if (_tickOnStartSpecialDelay != null) {
				await Task.Delay(_tickOnStartSpecialDelay.Value, token).ConfigureAwait(false);
				await Tick(token).ConfigureAwait(false);
			}

			using var pTimer = new PeriodicTimer(_tickFrequency);
			while (!_isDisposed && await pTimer.WaitForNextTickAsync(token).ConfigureAwait(false))
				await Tick(token).ConfigureAwait(false);
		}
		catch (OperationCanceledException) {
			// Task or operation cancelled via a requested stop - perform no other action
		}
		catch (Exception ex) {
			loopException = ex;
		}
		finally {
			_cts?.Cancel();
			_cts?.Dispose();
			_cts = null;
		}

		if (loopException != null && _pollingLoopOnException != null)
			_pollingLoopOnException(loopException);
	}
}