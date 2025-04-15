namespace Praxis;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

#nullable disable

/// <summary>
/// Represents a class that can be used to transport data and to signify fail states passed between service boundaries
/// </summary>
/// <typeparam name="T">The type of data that is to be transported</typeparam>
public class DataTransport<T> : DataTransport {

	/// <summary>
	/// Gets or sets a resulting data payload to be used when operations are successful
	/// </summary>
	public T Data { get; set; }
}


/// <summary>
/// Represents a class that can be used to transport data and to signify fail states passed between service boundaries
/// </summary>
public class DataTransport {

	/// <summary>
	/// Gets or sets a value indicating whether an (general) exception occurred at some point during operation.
	/// </summary>
	public bool HadException { get; set; }

	/// <summary>
	/// Gets a value indicating whether there is currently a failure state present on the class.
	/// By default, this checks any public, instance, boolean-type properties on the class, and if ANY ARE TRUE, THIS RETURNS TRUE
	/// If you wish to exclude other properties from consideration, override and include them in the <see cref="ExcludeHasFailStatePropertyNames"/> call
	/// </summary>
	/// <returns>True if there is a failure state present, otherwise false</returns>
	public bool HasFailState {
		get =>
			GetType()
			.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.Where(pi => pi.CanRead && pi.PropertyType == typeof(bool) && pi.Name != nameof(this.HasFailState) && !ExcludeHasFailStatePropertyNames().Contains(pi.Name))
			.Any(pi => (bool)pi.GetValue(this) == true);
	}

	/// <summary>
	/// Gets a value that indicates whether there are *currently* validation failures stored in the class's ValidationFailures
	/// property
	/// </summary>
	public bool HasValidationFailure {
		get => this.ValidationFailures?.Count() > 0;
	}

	/// <summary>
	/// Gets or sets a string that is typically used to describe current failure state or other instructions - the presence of a
	/// value here may not necessarily indicate failure
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	/// Gets or sets a list of validation failures to the data transport. Setting this property will update the value of the HadValidationFailure
	/// to reflect the content state of the passed list. If not null and the count is greater than 0, the property will be set to true.
	/// </summary>
	public IEnumerable<ValidationResult> ValidationFailures { get; set; }

	/// <summary>
	/// When using the <see cref="HasFailState"/> property, any property names added to this list will be excluded from the otherwise global boolean check for
	/// failure indicators
	/// </summary>
	/// <returns>An array of strings -> warning overriding this method and returning null will cause an exception</returns>
	protected virtual string[] ExcludeHasFailStatePropertyNames() => [];
}