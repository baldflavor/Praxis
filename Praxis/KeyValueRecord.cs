namespace Praxis;

/// <summary>
/// A generic key-value record.
/// </summary>
/// <typeparam name="K">The type of the key.</typeparam>
/// <typeparam name="V">The type of the value.</typeparam>
/// <param name="Key">The key.</param>
/// <param name="Value">The value.</param>
public record class KeyValueRecord<K, V>(K Key, V Value);
