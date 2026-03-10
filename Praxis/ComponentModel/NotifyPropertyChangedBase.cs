namespace Praxis.ComponentModel;

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>
/// Base class used for classes that want to broadcast when properties have changed.
/// </summary>
public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged {

	/// <summary>
	/// Event signifying that a property has changed on the class
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;


	/// <summary>
	/// This method should be called everywhere that a set operation is being triggered; or when one wishes to notify that something has changed "internally" on a class.
	/// </summary>
	/// <param name="propertyName">The name of the property that has changed. Read by compiler services; only pass to override.</param>
	protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	/// <summary>
	/// Code that is used to set property values, checking to see if the value set is the current value and will call <see cref="NotifyPropertyChanged(string)"/>
	/// if a new value is set.
	/// </summary>
	/// <typeparam name="T">Type of data in a backing field or property</typeparam>
	/// <param name="field">Reference to the backing field for a property</param>
	/// <param name="newValue">The new value to set to a backing field</param>
	/// <param name="propertyName">Name of the property that is calling this method</param>
	/// <returns>True if a new value was set, otherwise false</returns>
	protected bool SetProperty<T>([NotNullIfNotNull(nameof(newValue))] ref T field, T newValue, [CallerMemberName] string? propertyName = null) {
		if (EqualityComparer<T>.Default.Equals(field, newValue))
			return false;

		field = newValue;

		NotifyPropertyChanged(propertyName!);

		return true;
	}
}
