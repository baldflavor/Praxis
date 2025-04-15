namespace Praxis;

/// <summary>
/// Used to provide reference and interlocking around an integer counter for methods that are async and cannot otherwise be passed a value type by reference
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Incrementer" /> class
/// </remarks>
public sealed class Incrementer(int initialValue = 0) {

	public int Increment() {
		return Interlocked.Increment(ref initialValue);
	}
}