namespace Praxis.Data.Cosmos;

/// <summary>
/// Attribute that marks a class with the string Id of a Cosmos container to use for it's backing storage
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ContainerIdAttribute" /> class.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ContainerIdAttribute(string arg) : System.Attribute {

	/// <summary>
	/// Gets or sets the Id of a container to use when working with Cosmos storage
	/// </summary>
	public string Id { get; set; } = arg;
}