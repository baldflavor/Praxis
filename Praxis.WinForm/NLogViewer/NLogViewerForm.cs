namespace Praxis.WinForm.NLogViewer;

using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
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
	private static readonly string[] _skipNames = [_LEVEL, _TZI];

	/// <summary>
	/// Holds a reference to a comparer for TreeNode instances.
	/// </summary>
	private static readonly Comparer<TreeNode> _treeNodeComparer = _CreateTreeNodeComparer();

	/// <summary>
	/// Maximum number of nodes that should be displayed in the tree.
	/// </summary>
	private readonly int _maxNodeCount;

	/// <summary>
	/// Panel showing that the control is performing its initial load.
	/// </summary>
	private Panel? _loadingBlockingPanel;

	/// <summary>
	/// Used for locking while updating nodes / reading.
	/// </summary>
	private readonly object _lock = new();


	/// <summary>
	/// Creates an instance of the <see cref="NLogViewerForm"/> class.
	/// </summary>
	/// <param name="directory">The directory to watch</param>
	/// <param name="maxNodeCount">Maximum number of nodes to display in the tree before removing them from the bottom of the list</param>
	/// <param name="tickFrequency">How often to process file changes</param>
	/// <param name="startupDelay">Delay in initial startup to process changes / existing files</param>
	public NLogViewerForm(string directory, int maxNodeCount, TimeSpan tickFrequency, TimeSpan startupDelay) {
		InitializeComponent();

		_loadingBlockingPanel = this.AddBlockingPanel("Loading data...\r\nPlease wait", out _, 72, Cursors.WaitCursor);

		_maxNodeCount = maxNodeCount;

		_clearButton.Click += (s, e) => {
			_allEntriesTreeView.BeginUpdate();
			_allEntriesTreeView.SuspendLayout();
			_allEntriesTreeView.Nodes.Clear();
			_allEntriesTreeView.ResumeLayout(false);
			_allEntriesTreeView.EndUpdate();
		};

		_collapseButton.Click += (s, e) => {
			var tv = _GetVisibleTreeView();
			tv.BeginUpdate();
			tv.SuspendLayout();
			tv.CollapseAll();
			tv.ResumeLayout(false);
			tv.EndUpdate();
		};

		_statusPanel.VisibleChanged += (_, _) => _clearButton.Enabled = !_statusPanel.Visible;

		_allEntriesTreeView.NodeMouseClick += _CopyToClipboard;
		_filteredTreeView.NodeMouseClick += _CopyToClipboard;

		_filterTextBox.HandleEnterAction(_ApplyFilter);

		_clearFilterButton.Click += (_, _) => {
			_filterTextBox.Text = null;
			_ApplyFilter();
		};

		var watcher =
			new FileWatcher(
				directory,
				tickFrequency,
				startupDelay,
				_HandleException,
				_ProcessLines)
			.Start();

		FormClosed += (_, _) => watcher.Dispose();

		/* ----------------------------------------------------------------------------------------------------------
		 * Gets the currently visible tree view */
		TreeView _GetVisibleTreeView() => _allEntriesTreeView.Visible ? _allEntriesTreeView : _filteredTreeView;
	}


	void _ProcessLines(Queue<string> lines) {
		lock (_lock) {
			List<TreeNode> nodes = new List<TreeNode>(lines.Count);

			bool expandNew = Invoke(() => _expandNewCheckBox.Checked);

			while (lines.TryDequeue(out string? line)) {
				if (!line.StartsWith('{'))
					continue;

				nodes.Add(_TreeNodeFromJson(line, expandNew));
			}

			nodes.Sort(_treeNodeComparer);

			if (_loadingBlockingPanel is not null) {
				if (nodes.Count > _maxNodeCount)
					nodes = [.. nodes.Skip(_maxNodeCount)];
			}

			_PushToTreeView(nodes);
		}

		void _PushToTreeView(List<TreeNode> nodes) {
			BeginInvoke(async () => {
				if (_loadingBlockingPanel is not null)
					_allEntriesTreeView.Scrollable = false;

				TreeNode? selNode = _allEntriesTreeView.SelectedNode;
				bool isBottom = selNode is null || selNode.Index == _allEntriesTreeView.Nodes.Count - 1;

				_allEntriesTreeView.BeginUpdate();
				_allEntriesTreeView.SuspendLayout();

				foreach (TreeNode[] nChunk in nodes.Chunk(250)) {
					_allEntriesTreeView.Nodes.AddRange(nChunk);

					while (_allEntriesTreeView.Nodes.Count > _maxNodeCount)
						_allEntriesTreeView.Nodes.RemoveAt(0);

					await Task.Delay(20);
				}

				if (_loadingBlockingPanel is not null) {
					_allEntriesTreeView.Scrollable = true;
					_loadingBlockingPanel.Dispose();
					_loadingBlockingPanel = null;
				}

				_allEntriesTreeView.ResumeLayout(false);
				_allEntriesTreeView.EndUpdate();

				if (selNode is not null)
					selNode.EnsureVisible();
				else
					_allEntriesTreeView.Nodes[^1].EnsureVisible();
			});
		}
	}


	/// <summary>
	/// Creates a comparer used for sorting TreeNodes.
	/// </summary>
	/// <returns>Comparer</returns>
	private static Comparer<TreeNode> _CreateTreeNodeComparer() {
		return
			Comparer<TreeNode>.Create((x, y) => {
				if (x.Parent is not null && y.Parent is not null)
					return x.Index.CompareTo(y.Index);

				var xId = ((double oadBatch, int sequence))x.Tag;
				var yId = ((double oadBatch, int sequence))y.Tag;

				if (xId.oadBatch == yId.oadBatch)
					return xId.sequence.CompareTo(yId.sequence);
				else
					return xId.oadBatch.CompareTo(yId.oadBatch);
			});
	}


	/// <summary>
	/// Prevents calling this method other than by designer support
	/// </summary>
	[Obsolete("Designer Only", true)]
	private NLogViewerForm() {
		InitializeComponent();
	}

	/// <summary>
	/// Creates a string with padding between a name and value.
	/// </summary>
	/// <param name="name">Represents the name of a property.</param>
	/// <param name="value">Represents a property's value.</param>
	/// <returns>string with padding</returns>
	private static string _PaddedString(string name, string value) => $"{$"{name}: ",-25}{value.Trim()}";

	/// <summary>
	/// Initializes a new instance of the <see cref="TreeNode" /> class from a source Json string.
	/// </summary>
	/// <param name="json">Source Json string.</param>
	/// <param name="expand">Indicates whether the node should be expanded.</param>
	/// <exception cref="Exception">Thrown if TreeNode creation fails.</exception>
	private static TreeNode _TreeNodeFromJson(string json, bool expand) {
		var jObj = JsonNode.Parse(json)!.AsObject();

		string nlId = jObj[_ID]!.ToString();

		TreeNode newNode = new($"{_UTCToLocal(jObj)}    {jObj.FindNodeValues(_MESSAGE).FirstOrDefault()?.ToString().SubstringLeft(140)}") {
			BackColor = jObj[_LEVEL]!.ToString() switch {
				_INFORMATION => Color.Empty,
				_WARNING => Color.Yellow,
				_ERROR => Color.Red,
				_ => Color.Orchid
			},
			Name = nlId,
			Tag = nlId.ToNLogIDComponents()
		};

		_AddDescendants(newNode, jObj, true);

		if (expand)
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

				if (value is null) {
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

			return dt.ToString("yyyy-MM-dd HH:mm:ss.ffff");
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
	/// <param name="sender">Sender of the event - should be a TreeView.</param>
	/// <param name="e">Arguments for the event.</param>
	private void _CopyToClipboard(object? sender, TreeNodeMouseClickEventArgs e) {
		if (e.Button != MouseButtons.Right)
			return;

		bool isAllEntriesSender =
			((sender as TreeView) ?? throw new Exception("Handler must supply the sending treeview")) == _allEntriesTreeView;

		var topParentClickNode = e.Node.GetTopParent();

		Clipboard.SetText(
			(isAllEntriesSender ?
				topParentClickNode :
				_allEntriesTreeView.Nodes.Find(topParentClickNode.Name, false).Single()
			)
			.ToIndentedRecursiveString());
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
