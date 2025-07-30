namespace Praxis;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

/// <summary>
/// Contains delegate signatures and static helpers for invocation.
/// </summary>
public static class Delegate {

	/// <summary>
	/// Does nothing
	/// </summary>
	public static void Nothing() {
	}

	/// <summary>
	/// Throws an exception - used for ensuring implementers properly assign delegates before using a class
	/// </summary>
	/// <exception cref="InvalidOperationException">Always thrown</exception>
	[DoesNotReturn]
	public static void ThrowInvalidOperationException() => throw new InvalidOperationException();


	/// <summary>
	/// Provides resilience/retry to delegate execution.
	/// </summary>
	/// <remarks>
	/// Be cautious when using this method as transient failures will otherwise not be visible to calling code.
	/// <para><c>Exception</c>s that are thrown by <paramref name="action"/> invocation are added to an internal list and then thrown in an <c>AggregateException</c> if the
	/// number of attempted executions surpasses <paramref name="attempts"/>.</para>
	/// <para><c>OperationCanceledException</c>s are thrown when <paramref name="cTok"/> signals cancellation. Those thrown by <paramref name="action"/> invocation are
	/// treated as any other thrown <c>Exception</c>. To synchronize the two, pass a <c>CancellationToken</c> that is linked to the same <c>CancellationTokenSource</c>.</para>
	/// </remarks>
	/// <param name="action">Delegate to execute.</param>
	/// <param name="attempts">The number of times to retry execution.</param>
	/// <param name="cTok"><c>CancellationToken</c> used for signaling that execution attempts/delays should be cancelled.</param>
	/// <param name="pauseBetween">When supplied, execution is awaited between successive attempts. Cannot be less than <see cref="TimeSpan.Zero"/>.</param>
	/// <returns><c>Task</c></returns>
	/// <exception cref="AggregateException">Thrown if the number of execution attempts is surpassed.</exception>
	/// <exception cref="OperationCanceledException">Thrown if cancellation is signalled.</exception>
	public static async Task Resilient(Action action, int attempts, CancellationToken? cTok = null, TimeSpan? pauseBetween = null) {
		ArgumentOutOfRangeException.ThrowIfLessThan(attempts, 1);
		if (pauseBetween.HasValue)
			Assert.NonNegative(pauseBetween.Value);

		var exceptions = new List<Exception>(attempts);
		CancellationToken ct = cTok ?? CancellationToken.None;

		while (attempts-- > 0) {
			ct.ThrowIfCancellationRequested();

			try {
				action();
				return;
			}
			catch (Exception ex) {
				exceptions.Add(ex);

				if (pauseBetween.HasValue)
					await Task.Delay(pauseBetween.Value, ct).ConfigureAwait(false);
			}
		}

		throw new AggregateException(exceptions);
	}

	/// <summary>
	/// Provides resilience/retry to delegate execution.
	/// </summary>
	/// <remarks>
	/// Be cautious when using this method as transient failures will otherwise not be visible to calling code.
	/// <para><c>Exception</c>s that are thrown by <paramref name="action"/> invocation are added to an internal list and then thrown in an <c>AggregateException</c> if the
	/// number of attempted executions surpasses <paramref name="attempts"/>.</para>
	/// <para><c>OperationCanceledException</c>s are thrown when <paramref name="cTok"/> signals cancellation. Those thrown by <paramref name="action"/> invocation are
	/// treated as any other thrown <c>Exception</c>. To synchronize the two, pass a <c>CancellationToken</c> that is linked to the same <c>CancellationTokenSource</c>.</para>
	/// </remarks>
	/// <param name="action">Delegate to execute and return a value.</param>
	/// <param name="attempts">The number of times to retry execution.</param>
	/// <param name="cTok"><c>CancellationToken</c> used for signaling that execution attempts/delays should be cancelled.</param>
	/// <param name="pauseBetween">When supplied, execution is awaited between successive attempts. Cannot be less than <see cref="TimeSpan.Zero"/>.</param>
	/// <returns><c>Task</c></returns>
	/// <exception cref="AggregateException">Thrown if the number of execution attempts is surpassed.</exception>
	/// <exception cref="OperationCanceledException">Thrown if cancellation is signalled.</exception>
	public static async Task<T> Resilient<T>(Func<T> action, int attempts, CancellationToken? cTok = null, TimeSpan? pauseBetween = null) {
		ArgumentOutOfRangeException.ThrowIfLessThan(attempts, 1);
		if (pauseBetween.HasValue)
			Assert.NonNegative(pauseBetween.Value);

		var exceptions = new List<Exception>(attempts);
		CancellationToken ct = cTok ?? CancellationToken.None;

		while (attempts-- > 0) {
			ct.ThrowIfCancellationRequested();

			try {
				return action();
			}
			catch (Exception ex) {
				exceptions.Add(ex);

				if (pauseBetween.HasValue)
					await Task.Delay(pauseBetween.Value, ct).ConfigureAwait(false);
			}
		}

		throw new AggregateException(exceptions);
	}

	/// <summary>
	/// Provides resilience/retry to delegate execution.
	/// </summary>
	/// <remarks>
	/// Be cautious when using this method as transient failures will otherwise not be visible to calling code.
	/// <para><c>Exception</c>s that are thrown by <paramref name="action"/> invocation are added to an internal list and then thrown in an <c>AggregateException</c> if the
	/// number of attempted executions surpasses <paramref name="attempts"/>.</para>
	/// <para><c>OperationCanceledException</c>s are thrown when <paramref name="cTok"/> signals cancellation. Those thrown by <paramref name="action"/> invocation are
	/// treated as any other thrown <c>Exception</c>. To synchronize the two, pass a <c>CancellationToken</c> that is linked to the same <c>CancellationTokenSource</c>.</para>
	/// </remarks>
	/// <param name="action">(awaitable) Delegate to execute.</param>
	/// <param name="attempts">The number of times to retry execution.</param>
	/// <param name="cTok"><c>CancellationToken</c> used for signaling that execution attempts/delays should be cancelled.</param>
	/// <param name="pauseBetween">When supplied, execution is awaited between successive attempts. Cannot be less than <see cref="TimeSpan.Zero"/>.</param>
	/// <returns><c>Task</c></returns>
	/// <exception cref="AggregateException">Thrown if the number of execution attempts is surpassed.</exception>
	/// <exception cref="OperationCanceledException">Thrown if cancellation is signalled.</exception>
	public static async Task Resilient(Func<Task> action, int attempts, CancellationToken? cTok = null, TimeSpan? pauseBetween = null) {
		ArgumentOutOfRangeException.ThrowIfLessThan(attempts, 1);
		if (pauseBetween.HasValue)
			Assert.NonNegative(pauseBetween.Value);

		var exceptions = new List<Exception>(attempts);
		CancellationToken ct = cTok ?? CancellationToken.None;

		while (attempts-- > 0) {
			ct.ThrowIfCancellationRequested();

			try {
				await action().ConfigureAwait(false);
				return;
			}
			catch (Exception ex) {
				exceptions.Add(ex);

				if (pauseBetween.HasValue)
					await Task.Delay(pauseBetween.Value, ct).ConfigureAwait(false);
			}
		}

		throw new AggregateException(exceptions);
	}

	/// <summary>
	/// Provides resilience/retry to delegate execution.
	/// </summary>
	/// <remarks>
	/// Be cautious when using this method as transient failures will otherwise not be visible to calling code.
	/// <para><c>Exception</c>s that are thrown by <paramref name="action"/> invocation are added to an internal list and then thrown in an <c>AggregateException</c> if the
	/// number of attempted executions surpasses <paramref name="attempts"/>.</para>
	/// <para><c>OperationCanceledException</c>s are thrown when <paramref name="cTok"/> signals cancellation. Those thrown by <paramref name="action"/> invocation are
	/// treated as any other thrown <c>Exception</c>. To synchronize the two, pass a <c>CancellationToken</c> that is linked to the same <c>CancellationTokenSource</c>.</para>
	/// </remarks>
	/// <param name="action">(awaitable) Delegate to execute and return a value.</param>
	/// <param name="attempts">The number of times to retry execution.</param>
	/// <param name="cTok"><c>CancellationToken</c> used for signaling that execution attempts/delays should be cancelled.</param>
	/// <param name="pauseBetween">When supplied, execution is awaited between successive attempts. Cannot be less than <see cref="TimeSpan.Zero"/>.</param>
	/// <returns><c>Task</c></returns>
	/// <exception cref="AggregateException">Thrown if the number of execution attempts is surpassed.</exception>
	/// <exception cref="OperationCanceledException">Thrown if cancellation is signalled.</exception>
	public static async Task<T> Resilient<T>(Func<Task<T>> action, int attempts, CancellationToken? cTok = null, TimeSpan? pauseBetween = null) {
		ArgumentOutOfRangeException.ThrowIfLessThan(attempts, 1);
		if (pauseBetween.HasValue)
			Assert.NonNegative(pauseBetween.Value);

		var exceptions = new List<Exception>(attempts);
		CancellationToken ct = cTok ?? CancellationToken.None;

		while (attempts-- > 0) {
			ct.ThrowIfCancellationRequested();

			try {
				return await action().ConfigureAwait(false);
			}
			catch (Exception ex) {
				exceptions.Add(ex);

				if (pauseBetween.HasValue)
					await Task.Delay(pauseBetween.Value, ct).ConfigureAwait(false);
			}
		}

		throw new AggregateException(exceptions);
	}
}
