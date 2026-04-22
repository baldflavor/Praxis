namespace Praxis;

/// <summary>
/// Represents a generic key-value pair as a record type.
/// </summary>
/// <typeparam name="K">The type of the key.</typeparam>
/// <typeparam name="V">The type of the value.</typeparam>
public record class KeyValueRecord<K, V>(K Key, V Value);

/// <summary>
/// Factory methods for creating <see cref="KeyValueRecord{K, V}" /> instances.
/// </summary>
public static class KeyValueRecord {
	/// <summary>
	/// Creates a new <see cref="KeyValueRecord{K, V}" /> instance.
	/// </summary>
	/// <typeparam name="K">The type of the key.</typeparam>
	/// <typeparam name="V">The type of the value.</typeparam>
	/// <param name="key">The key.</param>
	/// <param name="value">The value.</param>
	/// <returns>A new <see cref="KeyValueRecord{K, V}" /> containing the specified key and value.</returns>
	public static KeyValueRecord<K, V> Create<K, V>(K key, V value) => new(key, value);
}
