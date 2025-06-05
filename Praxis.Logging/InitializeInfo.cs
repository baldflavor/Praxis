namespace Praxis.Logging;

using System.Reflection;

/// <summary>
/// Used for holding initialization information
/// </summary>
/// <remarks>
/// Consider cacheing or setting this to a variable to avoid creation of multiple instances of this class since the information
/// does not typically change during the lifetime of an application domain
/// </remarks>
/// <param name="executingAssembly">The assembly that is currently executing code</param>
/// <param name="extended">Extended properties to include in the initialization log</param>
public class InitializeInfo(Assembly executingAssembly, Dictionary<string, object>? extended = null) {
	/// <summary>
	/// Gets the name and version of the entry assembly for the current application domain
	/// </summary>
	public Assembly? AssemblyEntry { get; } = Assembly.GetEntryAssembly();

	/// <summary>
	/// Gets the name and version of an executing assembly
	/// </summary>
	public Assembly AssemblyExecuting { get; } = executingAssembly;

	/// <summary>
	/// Gets extended properties pertinent to initialization info
	/// </summary>
	public Dictionary<string, object>? Extended { get; } = extended;

	/// <summary>
	/// Gets the machine name
	/// </summary>
	public string MachineName { get; } = Environment.MachineName;
}
