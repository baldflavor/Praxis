namespace Praxis.WinForm;

using System.Collections;

// Extension methods for controls

public static partial class Extension {
	/// <summary>
	/// Adds a panel sized to the client size of its owner and an inner label that displays a text message
	/// </summary>
	/// <remarks>
	/// Performs layout on <paramref name="owner"/> after controls are added
	/// </remarks>
	/// <param name="owner"><see cref="Control"/> to add the panel to and for setting size and location</param>
	/// <param name="text">Set on the inner label as a message to a user</param>
	/// <param name="autoSizeFontMax">If not <see langword="null"/> then the font used for <paramref name="text"/> will be autosized with this as the font's maximum size</param>
	/// <param name="cursor">Cursor to set on the blocking panel and inner label. When <see langword="null"/>, <see cref="Cursors.Default"/> is used</param>
	/// <param name="borderPanelConfig">Optional delegate used for further configuration of the blocking (border) panel. Set sizing and spacing with care.</param>
	/// <param name="innerLabelConfig">Optional delegate used for further configuration of the inner text label. Set sizing and spacing with care.</param>
	/// <param name="innerLabel">Set to the inner text label. Useful when setting the <see cref="Label.Text"/> property to a different message</param>
	/// <returns>The outer border <see cref="Panel"/></returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="autoSizeFontMax"/> is less than <c>1</c></exception>
	public static Panel AddBlockingPanel(this Control owner, string text, out Label innerLabel, float? autoSizeFontMax = 100, Cursor? cursor = null, Action<Panel>? borderPanelConfig = null, Action<Label>? innerLabelConfig = null) {
		ArgumentOutOfRangeException.ThrowIfLessThan(autoSizeFontMax ?? 100, 1);

		var borderPanel = new Panel {
			Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
			BackColor = Color.SaddleBrown,
			BorderStyle = BorderStyle.None,
			CausesValidation = false,
			Cursor = cursor ?? Cursors.Default,
			Location = new Point(0, 0),
			Margin = new Padding(0),
			Padding = new Padding(6),
			Size = new Size(owner.ClientSize.Width, owner.ClientSize.Height),
			TabIndex = int.MaxValue,
			TabStop = false,
			Visible = false
		};

		borderPanelConfig?.Invoke(borderPanel);

		innerLabel = new Label {
			BackColor = Color.Bisque,
			BorderStyle = borderPanel.BorderStyle,
			CausesValidation = borderPanel.CausesValidation,
			Cursor = borderPanel.Cursor,
			Dock = DockStyle.Fill,
			ForeColor = Color.Black,
			TabIndex = borderPanel.TabIndex,
			Text = text,
		}.SetNoMarginPadding();

		innerLabelConfig?.Invoke(innerLabel);

		borderPanel.Controls.Add(innerLabel);
		owner.Controls.Add(borderPanel);

		if (autoSizeFontMax != null)
			innerLabel.AutoSizeFont(startingFontSize: autoSizeFontMax.Value);

		borderPanel.Visible = true;
		borderPanel.BringToFront();

		owner.PerformLayout();

		return borderPanel;
	}

	/// <summary>
	/// Auto sizes the font of a innerLabel so that the text fits inside it's client rectangle.
	/// Will also set the <see cref="Label.TextAlign"/> to <see cref="ContentAlignment.MiddleLeft"/>
	/// </summary>
	/// <remarks>
	/// Drawing is suspended while autosizing the font
	/// </remarks>
	/// <param name="label">Label to change the font size for</param>
	/// <param name="newText">If not <see langword="default"/> will be set to <see cref="Label.Text"/> before font sizing</param>
	/// <param name="startingFontSize">The starting font size desired - avoid using abnormally large values</param>
	/// <returns><paramref name="label"/></returns>
	public static Label AutoSizeFont(this Label label, string? newText = null, float startingFontSize = 100) {
		if (label.TextAlign != ContentAlignment.MiddleLeft)
			label.TextAlign = ContentAlignment.MiddleLeft;

		FontFamily fontFam = label.Font.FontFamily;
		Size clientSize = label.ClientSize;
		clientSize.Width -= label.Padding.Left + label.Padding.Right;
		clientSize.Height -= label.Padding.Top + label.Padding.Bottom;

		if (newText is not null)
			label.Text = newText;

		string text = label.Text;

		while (true) {
			using Font tempFont = new Font(fontFam, startingFontSize);

			Size textSize = TextRenderer.MeasureText(text, tempFont, clientSize, TextFormatFlags.WordBreak);
			if ((textSize.Width <= clientSize.Width && textSize.Height <= clientSize.Height) || startingFontSize == 1f) {
				label.Font = tempFont;
				return label;
			}

			startingFontSize -= .5f;
		}
	}

	/// <summary>
	/// Gets a recursive list of all controls in the controls collection of target and its children
	/// </summary>
	/// <typeparam name="T">The type of controls, specifically, to retrieve. Use <c>Control</c> for all controls</typeparam>
	/// <param name="arg">Target control to use as a starting point for navigation</param>
	/// <returns><see cref="IEnumerable{T}"/></returns>
	public static IEnumerable<T> ControlsRecursive<T>(this Control arg) where T : Control {
		foreach (var matching in arg.Controls.OfType<T>()) {
			yield return matching;

			foreach (var child in ControlsRecursive<T>(matching))
				yield return child;
		}
	}

	/// <summary>
	/// Disposes all child controls
	/// </summary>
	/// <remarks>
	/// Dispose on a control disposes all child controls of that control and also removes them from their parent
	/// </remarks>
	/// <param name="arg"><see cref="Control"/> to operate on</param>
	public static void DisposeChildren(this Control arg) {
		while (arg.Controls.Count > 0)
			arg.Controls[0].Dispose();
	}

	/// <summary>
	/// For the given control, will handle perform the passed Action for whenever either Keys.Enter or Keys.Return is depressed
	/// and does such so that the "ding" will not be played through speaker audio
	/// </summary>
	/// <param name="control">Control to target for handling</param>
	/// <param name="action">An Action to be performed when the handler fires</param>
	public static void HandleEnterAction(this Control control, Action action) {
		void kd(object? s, KeyEventArgs e) {
			bool suppress = (e.KeyCode is Keys.Enter or Keys.Return);
			e.SuppressKeyPress = suppress;
			if (suppress)
				action();
		}

		void kp(object? s, KeyPressEventArgs e) {
			if (e.KeyChar == (char)13)
				e.Handled = true;
		}

		control.KeyDown += kd;
		control.KeyPress += kp;
	}

	/// <summary>
	/// Sets the target combo box to have items of: N/A, Yes, No as selections, with a corresponding
	/// nullable boolean for: null, true, false respectively. Sets display and value members appropriately
	/// </summary>
	/// <param name="arg">Target combo box to alter</param>
	/// <returns><paramref name="arg"/></returns>
	public static ComboBox MakeYesNo(this ComboBox arg) {
		arg.SetItems([
				Tuple.Create((bool?)null, ""),
						Tuple.Create((bool?)true, "Yes"),
						Tuple.Create((bool?)false, "No")],
				true);
		arg.ValueMember = "Item1";
		arg.DisplayMember = "Item2";
		arg.Refresh();
		Application.DoEvents();

		return arg;
	}

	/// <summary>
	/// Offsets a control relative to another control with optional extra spacing
	/// </summary>
	/// <typeparam name="T">Type of control</typeparam>
	/// <param name="target">Target control to offset</param>
	/// <param name="relativeTo">The control to offset <paramref name="target"/> to</param>
	/// <param name="isHorizontal">Indicated whether to offset horizontally or vertically</param>
	/// <param name="pixels">Number of pixels to add to the offset (i.e. extra spacing)</param>
	/// <returns><paramref name="target"/></returns>
	public static T OffsetRelative<T>(this T target, Control relativeTo, bool isHorizontal, int pixels = 0) where T : Control {
		if (isHorizontal)
			target.Location = new Point(relativeTo.Location.X + relativeTo.Width + pixels, relativeTo.Location.Y);
		else
			target.Location = new Point(relativeTo.Location.X, relativeTo.Location.Y + relativeTo.Height + pixels);

		return target;
	}

	/// <summary>
	/// Sets the passed list of enumerable items to the items collection of the target combo box and optionally selects the first item automatically
	/// </summary>
	/// <param name="control">ComboBox to set the items to</param>
	/// <param name="items">IEnumerable item source</param>
	/// <param name="selectFirstItem">True to select the first item</param>
	/// <returns><paramref name="control"/></returns>
	public static ComboBox SetItems<T>(this ComboBox control, IEnumerable<T> items, bool selectFirstItem = false) where T : notnull {
		_SetItems(control, items, selectFirstItem);
		return control;
	}

	/// <summary>
	/// Sets the passed list of enumerable items to the items collection of the target list box and optionally selects the first item automatically
	/// </summary>
	/// <param name="control">ListoBox to set the items to</param>
	/// <param name="items">IEnumerable item source</param>
	/// <param name="selectFirstItem">True to select the first item</param>
	/// <returns><paramref name="control"/></returns>
	public static ListBox SetItems<T>(this ListBox control, IEnumerable<T> items, bool selectFirstItem = false) where T : notnull {
		_SetItems(control, items, selectFirstItem);
		return control;
	}

	/// <summary>
	/// Sets the control to have <see cref="Control.Margin"/> and <see cref="Control.Padding"/> to a new
	/// <see cref="Padding"/> set to <c>0</c> for all dimensions
	/// </summary>
	/// <typeparam name="T">Type of actual control being altered</typeparam>
	/// <param name="arg">Control to alter</param>
	/// <returns><paramref name="arg"/></returns>
	public static T SetNoMarginPadding<T>(this T arg) where T : Control {
		arg.Margin = new Padding(0);
		arg.Padding = new Padding(0);
		return arg;
	}

	/// <summary>
	/// Sets the starting location of a form to one quarter of it's width and height in from the left and top of another form and sets it's <see cref="Form.StartPosition"/> to <see cref="FormStartPosition.Manual"/>
	/// </summary>
	/// <param name="arg">The form to position</param>
	/// <param name="otherForm">Another form to use relative for the location of <paramref name="arg"/></param>
	/// <param name="setOwner">Whether to set <paramref name="otherForm"/> as the <see cref="Form.Owner"/> or <paramref name="arg"/></param>
	/// <returns><paramref name="arg"/></returns>
	public static Form SetStartLoc(this Form arg, Form otherForm, bool setOwner = false) {
		if (setOwner)
			arg.Owner = otherForm;

		arg.StartPosition = FormStartPosition.Manual;
		arg.Location = new Point(otherForm.Left + (arg.Width / 4), otherForm.Top + (arg.Height / 4));

		return arg;
	}

	/// <summary>
	/// Sets the selected item of the target combo box to be the FIRST matching item that maches the result of the evaluator function
	/// <para>myCbo.SetSelectedItem((MyClass ssi) => ssi.Food == foodVariable);</para>
	/// </summary>
	/// <typeparam name="T">Type of item to cast / operate on in the items of the combo box</typeparam>
	/// <param name="control">Target combo box to work with</param>
	/// <param name="evaluator">Function used for evaluating which object should be selected (matches equivalency)</param>
	/// <returns>A value indicating whether the value was set or not</returns>
	public static bool TrySetSelectedItem<T>(this ComboBox control, Func<T, bool> evaluator) => _TrySetSelectedItem(control, evaluator);

	/// <summary>
	/// Sets the selected item of the target combo box to be the FIRST matching item that maches the result of the evaluator function
	/// <para>myCbo.SetSelectedItem((MyClass ssi) => ssi.Food == foodVariable);</para>
	/// </summary>
	/// <typeparam name="T">Type of item to cast / operate on in the items of the combo box</typeparam>
	/// <param name="comboBox">Target combo box to work with</param>
	/// <param name="evaluator">Function used for evaluating which object should be selected (matches equivalency)</param>
	/// <returns>A value indicating whether the value was set or not</returns>
	public static bool TrySetSelectedItem<T>(this ListBox control, Func<T, bool> evaluator) => _TrySetSelectedItem(control, evaluator);

	/// <summary>
	/// Peforms an action on all controls that are recursive children of <paramref name="control"/>
	/// </summary>
	/// <param name="control"><paramref name="control"/></param>
	/// <param name="action">Delegate to perform on each recursively found child</param>
	/// <returns><paramref name="control"/></returns>
	public static Control WithChildren(this Control control, Action<Control> action) {
		action(control);
		foreach (Control child in control.Controls)
			_ = child.WithChildren(action);

		return control;
	}

	/// <summary>
	/// Sets the passed list of enumerable items to the items collection of the control and optionally selects the first item automatically
	/// </summary>
	/// <param name="control">Control to set the items to</param>
	/// <param name="items">IEnumerable item source</param>
	/// <param name="selectFirstItem">True to select the first item</param>
	private static void _SetItems<T>(dynamic control, IEnumerable<T> items, bool selectFirstItem = false) where T : notnull {
		control.BeginUpdate();
		control.Items.Clear();

		foreach (var item in items)
			control.Items.Add(item);

		control.EndUpdate();
		control.Refresh();
		Application.DoEvents();

		if (selectFirstItem)
			control.SelectedIndex = 0;
	}

	/// <summary>
	/// Sets the selected item of the target control to be the FIRST matching item that maches the result of the evaluator function
	/// <para>myCtrl.SetSelectedItem((MyClass ssi) => ssi.Food == foodVariable);</para>
	/// </summary>
	/// <typeparam name="T">Type of item to cast / operate on in the items of the combo box</typeparam>
	/// <param name="control">Target control to work with - make sure it supports all of the properties / methods included in the method</param>
	/// <param name="evaluator">Function used for evaluating which object should be selected (matches equivalency)</param>
	/// <returns>A value indicating whether the value was set or not</returns>
	private static bool _TrySetSelectedItem<T>(dynamic control, Func<T, bool> evaluator) {
		control.Refresh();
		Application.DoEvents();

		var itemsCast = ((IList)control.Items).Cast<T>();
		var p = itemsCast.FirstOrDefault(item => evaluator(item));

		if (!itemsCast.Any(evaluator))
			return false;

		control.SelectedItem = itemsCast.First(evaluator);
		return true;
	}
}
