namespace Praxis.WinForm.UserControl;

using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

/// <summary>
/// Form used for altering properties of controls in real time
/// </summary>
public partial class PropertyGridder : Form {
	/// <summary>
	/// Static reference to an item that can be selected to refresh the controls in the combo box
	/// </summary>
	private static readonly ComboBoxLabeledControl _refreshItem = new("-REFRESH LIST-", null!);

	/// <summary>
	/// Holds a reference to the source form for this one
	/// </summary>
	private readonly Form _source;

	/// <summary>
	/// Holds the original properties as they were before they were selected / loaded into the property grid
	/// </summary>
	private Dictionary<string, string> _ogProperties = [];

	/// <summary>
	/// Initializes a new instance of the <see cref="SandboxForm" /> class
	/// </summary>
	/// <param name="manager">Manager used for Api dependency</param>
	public PropertyGridder(Form source) {
		const string DIVLINE = "########################################";

		InitializeComponent();
		_source = source;

		changesTextBox.Font = new Font(FontFamily.GenericMonospace, 10);

		this.Owner = source;
		this.Name += $" - {source.Name}";
		this.Location = new Point(source.Left + source.Width + 2, source.Top);

		controlsComboBox.SelectionChangeCommitted += (s, _) => {
			var selItem = ((ComboBox)s!).SelectedItem;
			if (selItem != null) {
				var cblc = (ComboBoxLabeledControl)selItem;

				if (cblc == _refreshItem) {
					_FillComboBox();
				}
				else {
					Control c = cblc.Control;
					_ogProperties = new(_GetProps(c));
					mainPropertyGrid.SelectedObject = c;
				}
			}
			else {
				mainPropertyGrid.SelectedObject = null;
			}
		};

		_FillComboBox();

		viewPropDifferencesButton.Click += (_, _) => {
			object? selObj = mainPropertyGrid.SelectedObject;
			if (selObj == null)
				return;

			var sb = new StringBuilder(DIVLINE);
			sb.AppendLine();
			sb.AppendLine(((Control)selObj).Name);
			sb.AppendLine(DIVLINE);

			var changedProps =
					_GetProps(selObj)
					.Where(p => _ogProperties[p.Key] != p.Value)
					.OrderBy(p => p.Key)
					.Select(p => $"{p.Key}{Const.RIGHTARROWHEAD}{p.Value}");

			foreach (var chp in changedProps)
				sb.AppendLine(chp);

			changesTextBox.AppendText(sb.ToString());
		};
	}

	/// <summary>
	/// Gets properties that are not explicitly marked as non-browseable in a property grid
	/// </summary>
	/// <param name="obj">Objec to get properties for</param>
	/// <returns><see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> with <see langword="string"/> for key and value</returns>
	private static IEnumerable<KeyValuePair<string, string>> _GetProps(object obj) {
		return
				obj
				.GetType()
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => (p.GetCustomAttribute<BrowsableAttribute>()?.Browsable ?? true) == true)
				.Select(p => KeyValuePair.Create(p.Name, p.GetValue(obj)?.ToString() ?? ""));
	}

	/// <summary>
	/// Fills the combo box with information regarding the controls available for the grid as well as the special <see cref="_refreshItem"/>
	/// </summary>
	private void _FillComboBox() {
		controlsComboBox.SetItems(
				Enumerable.Repeat(_refreshItem, 1)
				.Concat([ComboBoxLabeledControl.From(_source)])
				.Concat(_source.ControlsRecursive<Control>().Select(c => ComboBoxLabeledControl.From(c)))
				.OrderBy(c => c.Name),
				true);
	}

	/// <summary>
	/// Record class that holds both a name and reference to a control
	/// </summary>
	/// <param name="Name">The name to display</param>
	/// <param name="Control">The control in question</param>
	private record class ComboBoxLabeledControl(string Name, Control Control) {
		public static ComboBoxLabeledControl From(Control control) {
			StringBuilder sb = new();
			_ParentNameRecursive(control, sb);
			_AppendName(control, sb);

			return new ComboBoxLabeledControl(sb.ToString(), control);

			/* ----------------------------------------------------------------------------------------------------------
			 * Appends the name of the control or it's type to a StringBuilder */
			static void _AppendName(Control arg, StringBuilder sb) {
				if (string.IsNullOrWhiteSpace(arg.Name))
					sb.Append(arg.GetType().Name);
				else
					sb.Append(arg.Name);
			}

			/* ----------------------------------------------------------------------------------------------------------
			 * Retrieves the recursive parent of the control and adds the name of each to a StringBuilder */
			static void _ParentNameRecursive(Control arg, StringBuilder sb) {
				if (arg.Parent is not null) {
					_ParentNameRecursive(arg.Parent, sb);
					_AppendName(arg.Parent, sb);
					sb.Append('.');
				}
			}
		}
	}
}
