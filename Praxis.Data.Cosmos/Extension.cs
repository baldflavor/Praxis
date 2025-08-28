namespace Praxis.Data.Cosmos;

using Microsoft.Azure.Cosmos;

/// <summary>
/// Static class for providing extensions to classes that are derived from <see cref="BaseModel"/>
/// </summary>
public static class Extension {

	/// <summary>
	/// Sets the <see cref="BaseModel.Id"/> property to the string representation of a new <see cref="Guid"/>
	/// </summary>
	/// <typeparam name="T">The type of arg type of model to work with</typeparam>
	/// <param name="arg">The arg object to set the new property on</param>
	/// <returns>The object that was modified with the new id</returns>
	public static T SetGuidId<T>(this T arg) where T : BaseModel {
		arg.Id = Guid.NewGuid().ToString("N");
		return arg;
	}

	/// <summary>
	/// Creates a partition key from the passed value.
	/// </summary>
	/// <param name="arg">Value to use for a partition key.</param>
	/// <returns><see cref="PartitionKey"/></returns>
	public static PartitionKey ToPartitionKey(this string arg) => new(arg);

	/// <summary>
	/// Creates a partition key from the passed value.
	/// </summary>
	/// <param name="arg">Value to use for a partition key.</param>
	/// <returns><see cref="PartitionKey"/></returns>
	public static PartitionKey ToPartitionKey(this bool arg) => new(arg);

	/// <summary>
	/// Creates a partition key from the passed value.
	/// </summary>
	/// <param name="arg">Value to use for a partition key.</param>
	/// <returns><see cref="PartitionKey"/></returns>
	public static PartitionKey ToPartitionKey(this double arg) => new(arg);

	/// <summary>
	/// Creates a partition key from the passed value.
	/// </summary>
	/// <param name="arg">Value to use for a partition key.</param>
	/// <returns><see cref="PartitionKey"/></returns>
	public static PartitionKey ToPartitionKey(this int arg) => new(arg);

	/// <summary>
	/// Creates a partition key from the passed value.
	/// </summary>
	/// <param name="arg">Value to use for a partition key.</param>
	/// <returns><see cref="PartitionKey"/></returns>
	public static PartitionKey ToPartitionKey(this long arg) => new(arg);
}
