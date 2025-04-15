namespace Praxis;

using System.Threading.Tasks;

/// <summary>
/// Contains delegate signatures for a variety of situations and use cases
/// </summary>
public static class Delegate {

	/// <summary>
	/// Generic delegate that can be used for asynchronous events
	/// </summary>
	/// <remarks>
	/// Event can be created like:
	/// <code>event EventHandlerAsync&lt;bool&gt;? SomeEvent;</code>
	/// And then invoked like such:
	/// <code>if (SomeEvent is not null)
	///     await SomeEvent(this, true);</code>
	/// </remarks>
	/// <typeparam name="T">Type of data used in event arguments</typeparam>
	/// <param name="sender">Object sending the event</param>
	/// <param name="e">Arguments related to the event</param>
	/// <returns>A Task</returns>
	private delegate Task EventHandlerAsync<T>(object? sender, T e);
}