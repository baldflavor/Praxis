namespace Praxis;

using System.Diagnostics;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Returns the current elapsed time of the arg stopwatch and calls restart on it (sets it to zero and restarts timing)
	/// </summary>
	/// <param name="arg">Target stopwatch to use</param>
	/// <returns>A TimeSpan</returns>
	public static TimeSpan ElapsedRestart(this Stopwatch arg) {
		try {
			arg.Stop();
			TimeSpan elapsed = arg.Elapsed;
			return elapsed;
		}
		finally {
			arg.Restart();
		}
	}
}