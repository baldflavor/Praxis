namespace Praxis.WinForm;

using System.Drawing.Drawing2D;

// Extension methods for drawing namespace objects

public static partial class Extension {

	/// <summary>
	/// Using the provided image, creates an icon using binary header information
	/// </summary>
	/// <param name="arg">Image to use for conversion</param>
	/// <returns><see cref="Icon"/></returns>
	/// <exception cref="Exception">May be thrown if the supplied image cannot be returned as an <see cref="Icon"/></exception>
	public static Icon ToIcon(this Image arg) => Icon.FromHandle(((Bitmap)arg).GetHicon());

	/// <summary>
	/// Draws a left-pointing filled arrow.
	/// </summary>
	/// <remarks>
	/// <para>Uses whatever smoothing mode is set on <paramref name="gfx"/>.</para>
	/// The triangular head occupies headFraction of the rectangle's width and its tip touches the left-center while the
	/// triangle's base touches top and bottom.<br/>
	/// The trunk (shaft) occupies the remaining horizontal space and is vertically half as tall as the head and centered vertically.
	/// </remarks>
	/// <param name="brush">Brush to use for filling the arrow.</param>
	/// <param name="gfx">Graphics used for drawing operation.</param>
	/// <param name="headPercentage">Percentage of the space taken up by the head of the arrow (i.e. .5f is 50% of the horizontal space). Clamped to: 0f - 1f.</param>
	/// <param name="rect">Rectangle used for bounds of the arrow.</param>
	/// <exception cref="ArgumentException">Thrown if the rectangle has a width or height less than or equal to 0f.</exception>
	public static void DrawFilledLeftArrow(Brush brush, Graphics gfx, float headPercentage, RectangleF rect) {
		if (rect.Width <= 0f || rect.Height <= 0f)
			throw new ArgumentException("Rectangle must have a width and height greater than 0").AddData(rect.Width).AddData(rect.Height);

		headPercentage = Math.Clamp(headPercentage, 0f, 1f);

		float headW = rect.Width * headPercentage;
		float trunkW = rect.Width - headW;
		float centerY = rect.Top + rect.Height / 2f;
		float trunkLeftX = rect.Left + headW;

		// Trunk: half the head (triangle) height, centered vertically
		float trunkH = rect.Height / 2f;
		float trunkTop = centerY - trunkH / 2f;
		RectangleF trunkRect = new RectangleF(trunkLeftX, trunkTop, trunkW, trunkH);

		using var path = new GraphicsPath();
		path.AddPolygon([
			new PointF(rect.Left, centerY), // tip
			new PointF(trunkLeftX, rect.Top), // base top
			new PointF(trunkLeftX, rect.Bottom) // base bottom
		]);

		if (trunkRect.Width > 0.0f && trunkRect.Height > 0.0f)
			path.AddRectangle(trunkRect);
		
		gfx.FillPath(brush, path);
	}
}
