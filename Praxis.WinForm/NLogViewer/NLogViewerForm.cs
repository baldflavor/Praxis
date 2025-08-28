namespace Praxis.WinForm.NLogViewer;

using System;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Praxis;

/// <summary>
/// Main form interface
/// </summary>
public partial class NLogViewerForm : Form {

	/// <summary>
	/// Constant string for error log levels
	/// </summary>
	private const string _ERROR = "E";

	/// <summary>
	/// Constant string for the ID property
	/// </summary>
	private const string _ID = "ID";

	/// <summary>
	/// Constant string for information log levels
	/// </summary>
	private const string _INFORMATION = "I";

	/// <summary>
	/// Constant string for the level property
	/// </summary>
	private const string _LEVEL = "Level";

	/// <summary>
	/// Constant string for the message property
	/// </summary>
	private const string _MESSAGE = "Message";

	/// <summary>
	/// Constant string for the UTC property
	/// </summary>
	private const string _UTC = "Utc";

	/// <summary>
	/// Constant string for the Tzi (Time Zone Info) property
	/// </summary>
	private const string _TZI = "Tzi";

	/// <summary>
	/// Constant string for warning log levels
	/// </summary>
	private const string _WARNING = "W";

	/// <summary>
	/// Represents the name of properties to avoid creating nodes for from a json payload
	/// </summary>
	private static readonly string[] _skipNames = [_ID, _LEVEL, _TZI];

	/// <summary>
	/// Creates an instance of the <see cref="NLogViewerForm"/> class.
	/// </summary>
	/// <param name="directory">The directory to watch</param>
	/// <param name="maxNodeCount">Maximum number of nodes to display in the tree before removing them from the bottom of the list</param>
	/// <param name="tickFrequency">How often to process file changes</param>
	/// <param name="startupDelay">Delay in initial startup to process changes / existing files</param>
	public NLogViewerForm(string directory, int maxNodeCount, TimeSpan tickFrequency, TimeSpan startupDelay) {
		InitializeComponent();

		_clearButton.Click += (s, e) => {
			_allEntriesTreeView.BeginUpdate();
			_allEntriesTreeView.Nodes.Clear();
			_allEntriesTreeView.EndUpdate();
		};

		_collapseButton.Click += (s, e) => {
			var tv = _GetVisibleTreeView();
			tv.BeginUpdate();
			tv.CollapseAll();
			tv.EndUpdate();
		};

		_expandButton.Click += (s, e) => {
			var tv = _GetVisibleTreeView();
			tv.BeginUpdate();
			tv.ExpandAll();
			tv.EndUpdate();
		};

		_statusPanel.VisibleChanged += (_, _) => _clearButton.Enabled = !_statusPanel.Visible;

		_allEntriesTreeView.MouseUp += _CopyToClipboard;
		_filteredTreeView.MouseUp += _CopyToClipboard;

		_filterTextBox.HandleEnterAction(_ApplyFilter);

		_clearFilterButton.Click += (_, _) => {
			_filterTextBox.Text = null;
			_ApplyFilter();
		};


		bool firstRun = true;

		var watcher =
			new FileWatcher(
				directory,
				tickFrequency,
				startupDelay,
				_HandleException,
				_ProcessLineDelegate,
				() => BeginInvoke(() => _allEntriesTreeView.BeginUpdate()),
				_FinishedReadDelegate)
			.Start();

		FormClosed += (_, _) => watcher.Dispose();

		// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

		/* ----------------------------------------------------------------------------------------------------------
		 * Delegate for when reading from a file / changes has been completed */
		void _FinishedReadDelegate() {
			BeginInvoke(_ActualWork);
			void _ActualWork() {
				if (firstRun == true) {
					firstRun = false;
					TreeNode[] orderedNodes = [.. _allEntriesTreeView.Nodes.Cast<TreeNode>().OrderByDescending(t => t.Text)];
					_allEntriesTreeView.Nodes.Clear();
					_allEntriesTreeView.Nodes.AddRange(orderedNodes);
				}
				else {
					if (_allEntriesTreeView.SelectedNode == null)
						_allEntriesTreeView.Nodes[0].EnsureVisible();
				}

				_allEntriesTreeView.EndUpdate();
			}
		}

		/* ----------------------------------------------------------------------------------------------------------
		 * Gets the currently visible tree view */
		TreeView _GetVisibleTreeView() => _allEntriesTreeView.Visible ? _allEntriesTreeView : _filteredTreeView;

		/* ----------------------------------------------------------------------------------------------------------
		 * Delegate wrapper for processing a line from the file watcher */
		void _ProcessLineDelegate(string line) {
			BeginInvoke(_ActualWork);
			void _ActualWork() {
				if (!line.StartsWith('{'))
					return;

				_allEntriesTreeView.BeginUpdate();
				_allEntriesTreeView.Nodes.Insert(0, _TreeNodeFromJson(line, _expandNewCheckBox.Checked));

				if (_allEntriesTreeView.Nodes.Count > maxNodeCount)
					_allEntriesTreeView.Nodes.Remove(_allEntriesTreeView.Nodes[^1]);

				_allEntriesTreeView.EndUpdate();
			}
		}
	}


	/// <summary>
	/// Prevents calling this method other than by designer support
	/// </summary>
	[Obsolete("Designer Only", true)]
	private NLogViewerForm() {
		InitializeComponent();
	}

	/// <summary>
	/// Creates a string with padding between the name and value
	/// </summary>
	/// <param name="name">FullName</param>
	/// <param name="value">value</param>
	/// <returns><see cref="string"/></returns>
	private static string _PaddedString(string name, string value) => $"{$"{name}: ",-25}{value.Trim()}";

	/// <summary>
	/// Initializes a new instance of the <see cref="TreeNode" /> class from a source Json string.
	/// </summary>
	/// <param name="json">Source Json string.</param>
	/// <param name="expanded">Indicates whether the node should be fully expanded.</param>
	/// <exception cref="Exception"></exception>
	private static TreeNode _TreeNodeFromJson(string json, bool expanded = false) {
		var jObj = JsonNode.Parse(json)!.AsObject();

		TreeNode newNode = new($"{_UTCToLocal(jObj)}    {jObj.FindNodeValues(_MESSAGE).FirstOrDefault()?.ToString().SubstringLeft(140)}") {
			BackColor = jObj[_LEVEL]!.ToString() switch {
				_INFORMATION => Color.Empty,
				_WARNING => Color.Yellow,
				_ERROR => Color.Red,
				_ => Color.Orchid
			},
			Name = jObj[_ID]!.ToString(),
			Tag = jObj
		};

		_AddDescendants(newNode, jObj, true);

		if (expanded)
			newNode.ExpandAll();

		return newNode;

		/* ----------------------------------------------------------------------------------------------------------
		 * Adds children recursively to a node using the passed JsonObject */
		static void _AddDescendants(TreeNode parentNode, JsonObject jObj, bool applySkip = false) {
			foreach (var jObjProperty in jObj) {
				string key = jObjProperty.Key;

				if (applySkip && _skipNames.Contains(key))
					continue;

				JsonNode? value = jObjProperty.Value;

				if (value == null) {
					parentNode.Nodes.Add(_PaddedString(key, "null"));
				}
				else {
					JsonValueKind valueKind = value.GetValueKind();
					if (valueKind != JsonValueKind.Object) {
						parentNode.Nodes.Add(_PaddedString(key, value.ToString()));
					}
					else {
						TreeNode childNode = new(key);
						parentNode.Nodes.Add(childNode);
						_AddDescendants(childNode, value.AsObject());
					}
				}
			}
		}

		/* ----------------------------------------------------------------------------------------------------------
		 * Converts utc time of the node to local time */
		static string _UTCToLocal(JsonObject jObj) {
			var utcValue = DateTime.Parse((string)(jObj[_UTC] ?? throw new Exception("Could not find a required property: " + _UTC))!);

			var tzi = TimeZoneInfo.FindSystemTimeZoneById((string)(jObj[_TZI] ?? throw new Exception("Could not find a required property: " + _TZI))!);

			var dt =
				TimeZoneInfo.ConvertTimeFromUtc(utcValue, tzi);

			return dt.ToString("yyyy-MM-ddTHH:mm:ss.fff");
		}
	}

	/// <summary>
	/// Apply a filter so that only those that pass are visible in the node list
	/// </summary>
	private void _ApplyFilter() {
		string? filter = _filterTextBox.Text.TrimToNull();
		_filteredTreeView.BeginUpdate();
		_filteredTreeView.Nodes.Clear();

		if (filter == null) {
			_allEntriesTreeView.Visible = true;
			_filteredTreeView.Visible = false;
			_SetStatus(null);
		}
		else {
			_filteredTreeView.BeginUpdate();
			foreach (TreeNode topNode in _allEntriesTreeView.Nodes) {
				bool hadMatch = false;
				var nodeClone = (TreeNode)topNode.Clone();
				nodeClone.SelfAndDescendants((node) => {
					if (node.Name.ContainsOIC(filter)) {
						node.BackColor = Color.HotPink;
						hadMatch = true;
					}
					else if (node.Text.ContainsOIC(filter)) {
						node.BackColor = Color.Aqua;
						hadMatch = true;
					}
				});

				if (hadMatch) {
					nodeClone.ExpandAll();
					_filteredTreeView.Nodes.Add(nodeClone);
				}
			}

			_filteredTreeView.EndUpdate();
			_filteredTreeView.Visible = true;
			_allEntriesTreeView.Visible = false;
			_SetStatus($"Filtered: {filter}");
		}

		_filteredTreeView.EndUpdate();
	}

	/// <summary>
	/// Copies the Json of a node to the clipboard
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void _CopyToClipboard(object? sender, MouseEventArgs e) {
		var treeView = (TreeView)(sender ?? throw new Exception("Handler must supply the sending treeview"));

		TreeNode? topParentClickNode;
		if (e.Button != MouseButtons.Right || (topParentClickNode = treeView.GetNodeAt(e.Location)?.GetTopParent()) == null)
			return;

		treeView.SelectedNode = topParentClickNode;

		Clipboard.SetText(
			treeView == _allEntriesTreeView ?
			((JsonObject)topParentClickNode.Tag).ToJsonString(Json.Options) :
			((JsonObject)_allEntriesTreeView.Nodes.Find(topParentClickNode.Name, false).Single().Tag).ToJsonString(Json.Options));
	}

	/// <summary>
	/// Code to handle an exception
	/// </summary>
	/// <param name="ex">Exception that was thrown</param>
	private void _HandleException(Exception ex) => BeginInvoke(() => MessageBox.Show(ex.GetDetail(), "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error));

	/// <summary>
	/// Sets a status to be displayed and shown to the user
	/// </summary>
	/// <param name="arg">If <see langword="null"/> then status is hidden from the user, otherwise it is made visible and shown</param>
	private void _SetStatus(string? arg) {
		if (arg == null) {
			_statusPanel.Visible = false;
		}
		else {
			_statusLabel.AutoSizeFont(arg);
			_statusPanel.Visible = true;
		}
	}
}
