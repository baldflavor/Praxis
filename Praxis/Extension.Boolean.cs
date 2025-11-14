namespace Praxis;

/// <summary>
/// Extension methods for various object types
/// </summary>
public static partial class Extension {

	/// <summary>
	/// Returns either 1 or 0 based on true/false respectively
	/// </summary>
	/// <param name="arg">Boolean value to examine</param>
	/// <returns>1 or 0</returns>
	public static int ToInteger(this bool arg) => arg ? Const.TRUEINT : Const.FALSEINT;

	/// <summary>
	/// Returns either "Yes" or "No" based on true/false respectively
	/// </summary>
	/// <param name="arg">Boolean value to examine</param>
	/// <returns>"Yes" or "No"</returns>
	public static string ToYesNo(this bool arg) => arg ? "Yes" : "No";
}
