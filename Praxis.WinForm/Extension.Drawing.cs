namespace Praxis.WinForm;

using System.Collections;
using System.Windows.Forms;

// Extension methods for drawing namespace objects

internal static partial class Extension {

	/// <summary>
	/// Using the provided image, creates an icon using binary header information
	/// </summary>
	/// <param name="img">Image to use for conversion</param>
	/// <returns><see cref="Icon"/></returns>
	/// <exception cref="Exception">May be thrown if the supplied image cannot be returned as an <see cref="Icon"/></exception>
	public static Icon ToIcon(this Image arg) => Icon.FromHandle(((Bitmap)arg).GetHicon());
}