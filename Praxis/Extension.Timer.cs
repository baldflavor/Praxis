namespace Praxis;

public static partial class Extension {

	/// <summary>
	/// Restarts a <see cref="System.Timers.Timer"/> by first calling it's <see cref="System.Timers.Timer.Stop"/> method and then
	/// its <see cref="System.Timers.Timer.Start"/> method
	/// </summary>
	/// <param name="arg">Timer to restart</param>
	/// <returns>The passed timer</returns>
	public static System.Timers.Timer Restart(this System.Timers.Timer arg) {
		arg.Stop();
		arg.Start();
		return arg;
	}
}
