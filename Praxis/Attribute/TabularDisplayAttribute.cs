namespace Praxis.Attribute;

/// <summary>
/// Attribute that can direct extra output control when using the <see cref="ITabularDisplay"/> generation methods
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class TabularDisplayAttribute : System.Attribute {

	/// <summary>
	/// Backing field for both the <see cref="Order"/> property as well as the value returned by the <see cref="GetOrder"/> method
	/// </summary>
	private int? _order;

	/// <summary>
	/// Gets or sets a value that indicates whether or not the given property is included in tabular output or excluded
	/// </summary>
	public bool IsExcluded { get; set; }

	/// <summary>
	/// Gets or sets a string that may be used as an alternate label for a property as opposed to the class property name
	/// </summary>
	public string? Label { get; set; }

	/// <summary>
	/// Get or sets a value that can be used to specify the order of output for header and values in a tabular format.
	/// This property will return a coalesced value of 0; if it is necessary to check whether a value has been explicitly specified (e.g. null)
	/// then use the  <see cref="GetOrder"/> method instead of this value getter.
	/// <para>Collisions between the same order value are valid and are sub sorted by the existence of either <see cref="Label"/> or the reflected property name the attribute is applied to</para>
	/// </summary>
	public int Order {
		get => _order ?? 0;
		set => _order = value;
	}

	/// <summary>
	/// Returns the <see cref="_order"/> backing field -- in contrast to <see cref="Order"/> this method will return null if no value has ever appreciably
	/// been set, whereas the <see cref="Order"/> property will return a 0
	/// </summary>
	/// <returns>Null if no order is specified; othewise the integer value with which to order output</returns>
	public int? GetOrder() => _order;
}