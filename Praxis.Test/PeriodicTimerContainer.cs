namespace Praxis.Test;

using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Praxis.Knowledge;

[TestClass]
public class PeriodicTimerContainer {

	[TestMethod]
	public async Task Klinklin() {
		using var i = new Implementation(TimeSpan.FromSeconds(2));
		i.Start();
		await Task.Delay(TimeSpan.FromMinutes(.2));
		i.Stop();
	}



	private class Implementation(TimeSpan tickFrequency) : Praxis.PeriodicTimerContainer(tickFrequency) {

		/// <summary>
		/// Holds some strings that are drained during the paralell loop
		/// </summary>
		private readonly ConcurrentQueue<string> _someStuff = new(Enumerable.Range(0, 1000).Select(e => Word.Random()));

		private int _runningHashCode;

		private int _counter = 0;

		protected override async Task Tick(CancellationToken cToken) {
			try {
				await Parallel.ForEachAsync(Enumerable.Range(0, _someStuff.Count), new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = cToken }, async (d, cTok) => {
					if (_someStuff.TryDequeue(out string? dat))
						await _Dibby(dat, cTok);
				});
			}
			catch (OperationCanceledException oce) {
				// Caught so cancellation messages do not throw out from this method
				var mEx = oce;
			}
			catch (Exception ex) {
				var mEx = ex;
			}

			_runningHashCode = 0;
			_counter = 0;
		}


		private async Task _Dibby(string arg, CancellationToken cTok) {
			int curCounter = Interlocked.Increment(ref _counter);
			if (curCounter == 500)
				throw new Exception();

			await Task.Delay(10, cTok).ConfigureAwait(false);

			Interlocked.Exchange(
				ref _runningHashCode,
				HashCode.Combine(
					arg.GetHashCode(),
					Interlocked.CompareExchange(ref _runningHashCode, int.MinValue, int.MinValue)));
		}
	}
}