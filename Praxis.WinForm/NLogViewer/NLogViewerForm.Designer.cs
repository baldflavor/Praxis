namespace Praxis.WinForm.NLogViewer;

partial class NLogViewerForm {
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	protected override void Dispose(bool disposing) {
		if (disposing && (components != null)) {
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent() {
		FlowLayoutPanel _optionsFlowLayoutPanel;
		Panel _filterPanel;
		var resources = new System.ComponentModel.ComponentResourceManager(typeof(NLogViewerForm));
		_expandNewCheckBox = new CheckBox();
		_collapseButton = new Button();
		_clearButton = new Button();
		_filterTextBox = new TextBox();
		_clearFilterButton = new Button();
		_allEntriesTreeView = new TreeView();
		_statusPanel = new Panel();
		_statusLabel = new Label();
		_filteredTreeView = new TreeView();
		_optionsFlowLayoutPanel = new FlowLayoutPanel();
		_filterPanel = new Panel();
		_optionsFlowLayoutPanel.SuspendLayout();
		_filterPanel.SuspendLayout();
		_statusPanel.SuspendLayout();
		SuspendLayout();
		// 
		// _optionsFlowLayoutPanel
		// 
		_optionsFlowLayoutPanel.BorderStyle = BorderStyle.Fixed3D;
		_optionsFlowLayoutPanel.Controls.Add(_expandNewCheckBox);
		_optionsFlowLayoutPanel.Controls.Add(_collapseButton);
		_optionsFlowLayoutPanel.Controls.Add(_clearButton);
		_optionsFlowLayoutPanel.Controls.Add(_filterPanel);
		_optionsFlowLayoutPanel.Dock = DockStyle.Top;
		_optionsFlowLayoutPanel.Location = new Point(0, 0);
		_optionsFlowLayoutPanel.Name = "_optionsFlowLayoutPanel";
		_optionsFlowLayoutPanel.Padding = new Padding(6, 6, 0, 0);
		_optionsFlowLayoutPanel.Size = new Size(737, 49);
		_optionsFlowLayoutPanel.TabIndex = 2;
		_optionsFlowLayoutPanel.WrapContents = false;
		// 
		// _expandNewCheckBox
		// 
		_expandNewCheckBox.Location = new Point(9, 9);
		_expandNewCheckBox.Name = "_expandNewCheckBox";
		_expandNewCheckBox.Size = new Size(91, 32);
		_expandNewCheckBox.TabIndex = 0;
		_expandNewCheckBox.TabStop = false;
		_expandNewCheckBox.Text = "Expand New";
		_expandNewCheckBox.UseVisualStyleBackColor = true;
		// 
		// _collapseButton
		// 
		_collapseButton.Location = new Point(113, 9);
		_collapseButton.Margin = new Padding(10, 3, 0, 3);
		_collapseButton.Name = "_collapseButton";
		_collapseButton.Size = new Size(75, 32);
		_collapseButton.TabIndex = 1;
		_collapseButton.TabStop = false;
		_collapseButton.Text = "Collapse";
		_collapseButton.UseVisualStyleBackColor = true;
		// 
		// _clearButton
		// 
		_clearButton.Location = new Point(295, 9);
		_clearButton.Margin = new Padding(16, 3, 0, 3);
		_clearButton.Name = "_clearButton";
		_clearButton.Size = new Size(75, 32);
		_clearButton.TabIndex = 4;
		_clearButton.TabStop = false;
		_clearButton.Text = "Clear";
		_clearButton.UseVisualStyleBackColor = true;
		// 
		// _filterPanel
		// 
		_filterPanel.BorderStyle = BorderStyle.Fixed3D;
		_filterPanel.Controls.Add(_filterTextBox);
		_filterPanel.Controls.Add(_clearFilterButton);
		_filterPanel.Location = new Point(390, 9);
		_filterPanel.Margin = new Padding(20, 3, 0, 3);
		_filterPanel.Name = "_filterPanel";
		_filterPanel.Size = new Size(337, 32);
		_filterPanel.TabIndex = 5;
		// 
		// _filterTextBox
		// 
		_filterTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		_filterTextBox.BorderStyle = BorderStyle.FixedSingle;
		_filterTextBox.Location = new Point(34, 3);
		_filterTextBox.Margin = new Padding(0);
		_filterTextBox.MaxLength = 1000;
		_filterTextBox.Name = "_filterTextBox";
		_filterTextBox.Size = new Size(296, 23);
		_filterTextBox.TabIndex = 1;
		_filterTextBox.TabStop = false;
		// 
		// _clearFilterButton
		// 
		_clearFilterButton.Dock = DockStyle.Left;
		_clearFilterButton.Location = new Point(0, 0);
		_clearFilterButton.Margin = new Padding(0);
		_clearFilterButton.Name = "_clearFilterButton";
		_clearFilterButton.Size = new Size(32, 28);
		_clearFilterButton.TabIndex = 0;
		_clearFilterButton.TabStop = false;
		_clearFilterButton.Text = "✖️";
		_clearFilterButton.UseVisualStyleBackColor = true;
		// 
		// _allEntriesTreeView
		// 
		_allEntriesTreeView.BackColor = Color.AntiqueWhite;
		_allEntriesTreeView.Dock = DockStyle.Fill;
		_allEntriesTreeView.Font = new Font("Cascadia Code", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
		_allEntriesTreeView.Indent = 22;
		_allEntriesTreeView.ItemHeight = 16;
		_allEntriesTreeView.LineColor = Color.SaddleBrown;
		_allEntriesTreeView.Location = new Point(0, 96);
		_allEntriesTreeView.Name = "_allEntriesTreeView";
		_allEntriesTreeView.Size = new Size(737, 827);
		_allEntriesTreeView.TabIndex = 0;
		_allEntriesTreeView.TabStop = false;
		// 
		// _statusPanel
		// 
		_statusPanel.BackColor = Color.Black;
		_statusPanel.Controls.Add(_statusLabel);
		_statusPanel.Dock = DockStyle.Top;
		_statusPanel.Location = new Point(0, 49);
		_statusPanel.Name = "_statusPanel";
		_statusPanel.Padding = new Padding(4);
		_statusPanel.Size = new Size(737, 47);
		_statusPanel.TabIndex = 3;
		_statusPanel.Visible = false;
		// 
		// _statusLabel
		// 
		_statusLabel.BackColor = Color.PowderBlue;
		_statusLabel.Dock = DockStyle.Fill;
		_statusLabel.Location = new Point(4, 4);
		_statusLabel.Name = "_statusLabel";
		_statusLabel.Size = new Size(729, 39);
		_statusLabel.TabIndex = 0;
		_statusLabel.TextAlign = ContentAlignment.MiddleCenter;
		// 
		// _filteredTreeView
		// 
		_filteredTreeView.BackColor = Color.AntiqueWhite;
		_filteredTreeView.Dock = DockStyle.Fill;
		_filteredTreeView.Font = new Font("Cascadia Code", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
		_filteredTreeView.Indent = 22;
		_filteredTreeView.ItemHeight = 16;
		_filteredTreeView.LineColor = Color.SaddleBrown;
		_filteredTreeView.Location = new Point(0, 96);
		_filteredTreeView.Name = "_filteredTreeView";
		_filteredTreeView.Size = new Size(737, 827);
		_filteredTreeView.TabIndex = 0;
		_filteredTreeView.TabStop = false;
		_filteredTreeView.Visible = false;
		// 
		// NLogViewerForm
		// 
		this.AutoScaleDimensions = new SizeF(7F, 15F);
		this.AutoScaleMode = AutoScaleMode.Font;
		this.ClientSize = new Size(737, 923);
		this.Controls.Add(_filteredTreeView);
		this.Controls.Add(_allEntriesTreeView);
		this.Controls.Add(_statusPanel);
		this.Controls.Add(_optionsFlowLayoutPanel);
		this.DoubleBuffered = true;
		this.HelpButton = true;
		this.Icon = SystemIcons.GetStockIcon(StockIconId.AutoList);
		this.Name = "NLogViewerForm";
		this.StartPosition = FormStartPosition.CenterScreen;
		this.Text = "NLog Viewer (Right click any node to copy a log to the clipboard)";
		_optionsFlowLayoutPanel.ResumeLayout(false);
		_filterPanel.ResumeLayout(false);
		_filterPanel.PerformLayout();
		_statusPanel.ResumeLayout(false);
		ResumeLayout(false);
	}

	#endregion

	private TreeView _allEntriesTreeView;
	private CheckBox _expandNewCheckBox;
	private Button _collapseButton;
	private Button _clearButton;
	private TextBox _filterTextBox;
	private Button _clearFilterButton;
	private Panel _statusPanel;
	private Label _statusLabel;
	private TreeView _filteredTreeView;
}
