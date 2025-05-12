namespace Praxis;

using System.Drawing;
using System.Reflection;

/// <summary>
/// Static class for drawing namespace helpers
/// </summary>
public static class Drawing {

	/// <summary>
	/// Array of all the system named colors
	/// </summary>
	private static Color[] _namedColors = [.. typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public).Select(p => (Color)p.GetValue(null)!)];

	/// <summary>
	/// Gets an array of all named colors
	/// </summary>
	public static Color[] NamedColors => _namedColors;

	/// <summary>
	/// Returns a random named color
	/// </summary>
	/// <returns><see cref="Color"/></returns>
	public static Color RandomNamedColor() => _namedColors[Random.Shared.Next(0, _namedColors.Length)];
}