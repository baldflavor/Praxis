namespace Praxis.WinForm.UserControl;

using System;
using System.Drawing.Drawing2D;
using System.Drawing.Text;


/// <summary>
/// This control is meant for one to provide colors and borders, and then have the text that is set "scale up" to fit the size
/// of the displayed area, primarily for single line pieces of text. It is not meant for rapid changes in text as that will cause re-calculation
/// of the size to be determined
/// </summary>
public sealed class ScaledFontLabelVer2 : Control {

	/// <summary>
	/// Used for locking so fonts aren't disposed while drawing
	/// </summary>
	private readonly object _fontLock = new();

	/// <summary>
	/// The width of the border to draw
	/// </summary>
	private int _borderWidth = 4;

	/// <summary>
	/// Indicates whether the font needs to be recalculated
	/// </summary>
	private bool _createNewFont = true;

	/// <summary>
	/// The maximum size of font to use
	/// </summary>
	private int _fontMaxSize = 100;

	/// <summary>
	/// Gets the internal font to use for drawing
	/// </summary>
	private Font _internalFont;

	/// <summary>
	/// Holds the smoothing mode to be used for drawing
	/// </summary>
	/// <remarks>
	/// There are really only two important choices:
	/// <para><see cref="SmoothingMode.None"/> and <see cref="SmoothingMode.AntiAlias"/></para>
	/// All other choices are effectively the same as those two.
	/// <para><see href="https://learn.microsoft.com/en-us/dotnet/api/system.drawing.drawing2d.smoothingmode"/></para>
	/// </remarks>
	private SmoothingMode _smoothingMode = SmoothingMode.None;

	/// <summary>
	/// Rectangle used for drawing the bounds of the control
	/// </summary>
	private Rectangle _textAreaRect;

	/// <summary>
	/// Gets or sets a value indicating whether anti-aliased drawing is used
	/// </summary>
	public bool AntiAlias {
		get => _smoothingMode == SmoothingMode.AntiAlias;
		set {
			_smoothingMode = value ? SmoothingMode.AntiAlias : SmoothingMode.None;
			Invalidate();
		}
	}

	/// <summary>
	/// Gets or sets the color to be used for drawing the border
	/// </summary>
	public Color BorderColor { get; set; } = Color.Black;

	/// <summary>
	/// Gets or sets the border width to use -- values less than 4 may look strange when <see cref="AntiAlias"/> is <see langword="true"/>
	/// </summary>
	/// <remarks>
	/// Fixed to a minimum value of <b>0</b>
	/// </remarks>
	public int BorderWidth {
		get => _borderWidth;
		set {
			_borderWidth = Math.Max(0, value);
			_CalcRect();
			Invalidate();
		}
	}

	/// <summary>
	/// Gets or sets the maximum size of a font to render. Note, large sizes (greater than <b>100</b>) may impact performance when scaling
	/// </summary>
	/// <remarks>
	/// Fixed to a minimum value of <b>2</b>
	/// </remarks>
	public int FontMaxSize {
		get => _fontMaxSize;
		set {
			_fontMaxSize = Math.Max(2, value);
			_CalcRect();
			Invalidate();
		}
	}

	/// <summary>
	/// Gets or sets the text rendering hint to be used when writing text
	/// </summary>
	public TextRenderingHint TextRenderingHint { get; set; } = TextRenderingHint.AntiAlias;


	/// <summary>
	/// Initializes a new instance of the <see cref="ScaledFontLabelVer2" /> class
	/// </summary>
	public ScaledFontLabelVer2() {
		SetStyle(ControlStyles.Selectable, false);

		this.DoubleBuffered = true;
		this.Margin = new Padding(0);
		this.MinimumSize = new Size(40, 20);
		this.Padding = new Padding(0);
		this.ResizeRedraw = true;
		this.TabStop = false;
		this.Size = new Size(200, 100);
		_internalFont = (Font)this.Font.Clone();
	}

	/// <summary>
	/// Code that runs when the font is changed
	/// </summary>
	/// <param name="e">Event arguments</param>
	protected override void OnFontChanged(EventArgs e) {
		_CalcRect();
		base.OnFontChanged(e);
	}

	/// <summary>
	/// Method called to repaint this control -- performs all necessary drawing
	/// </summary>
	/// <param name="e">Paint event arguments</param>
	protected override void OnPaint(PaintEventArgs e) {
		if (!this.Visible)
			return;

		Rectangle textAreaRect = _textAreaRect;
		Graphics gfx = e.Graphics;
		gfx.SmoothingMode = _smoothingMode;
		gfx.TextRenderingHint = this.TextRenderingHint;

		gfx.Clear(this.BackColor);

		if (this.BorderWidth > 0) {
			using Pen borderPen = new(this.BorderColor, _borderWidth);
			borderPen.Alignment = PenAlignment.Inset;
			gfx.DrawRectangle(borderPen, this.DisplayRectangle);
		}

		using Brush textBrush = new SolidBrush(this.ForeColor);
		using StringFormat stringFormat = new() {
			Trimming = StringTrimming.None,
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center,
			FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap
		};

		lock (_fontLock) {
			gfx.DrawString(this.Text, _GetFont(), textBrush, textAreaRect, stringFormat);
		}

		Font _GetFont() {
			if (_createNewFont) {
				_createNewFont = false;

				string text = this.Text;
				Size textAreaSize = _textAreaRect.Size;
				_internalFont.Dispose();
				_internalFont = new Font(this.Font.FontFamily, _ShrinkUntil(_GrowUntil(2)), this.Font.Style);

				/* ----------------------------------------------------------------------------------------------------------
				 * Grows the size of a potential font until too large to fit in bounding area */
				int _GrowUntil(int fSize) {
					const int FONTGROWFACTOR = 4;

					while (fSize < _fontMaxSize) {
						using Font tempFont = new Font(this.Font.FontFamily, fSize);
						if (_IsTooLarge(_MeasureText(tempFont)))
							break;
						else
							fSize += FONTGROWFACTOR;
					}

					return fSize;
				}

				/* ----------------------------------------------------------------------------------------------------------
				 * Shrinks a font until it's small enough to fit in a bounding area */
				int _ShrinkUntil(int fSize) {
					const int FONTSHRINKFACTOR = 1;

					while (fSize > 1) {
						using Font tempFont = new Font(this.Font.FontFamily, fSize);

						if (_IsTooLarge(_MeasureText(tempFont)))
							fSize -= FONTSHRINKFACTOR;
						else
							break;
					}

					return fSize;
				}

				/* ----------------------------------------------------------------------------------------------------------
				 * Measures text given a font */
				Size _MeasureText(Font font) => TextRenderer.MeasureText(gfx, text, font, textAreaSize, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);

				/* ----------------------------------------------------------------------------------------------------------
				 * Determines whether the measured size is too large to fit in a text area */
				bool _IsTooLarge(Size measuredSize) {
					return measuredSize.Width > textAreaSize.Width || measuredSize.Height > textAreaSize.Height;
				}
			}

			return _internalFont;
		}
	}

	/// <summary>
	/// Code that runs when the control's size has changed
	/// </summary>
	/// <param name="e">Event arugments</param>
	protected override void OnSizeChanged(EventArgs e) {
		_CalcRect();
		base.OnSizeChanged(e);
	}

	protected override void OnTextChanged(EventArgs e) {
		_createNewFont = true;
		Invalidate();
		base.OnTextChanged(e);
	}

	/// <summary>
	/// Calculates an interior rectangle size which is where you want to draw the actual text
	/// </summary>
	private void _CalcRect() {
		int borderWidth = this.BorderWidth;
		Rectangle dispRect = this.DisplayRectangle;

		if (borderWidth == 0) {
			_textAreaRect = dispRect;
		}
		else {
			int borderNegFactor = borderWidth * 2;
			_textAreaRect = new Rectangle(dispRect.X + borderWidth, dispRect.Y + borderWidth, dispRect.Width - borderNegFactor, dispRect.Height - borderNegFactor);
		}

		_createNewFont = true;
	}
}