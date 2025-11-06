namespace Praxis.Winform.Test;

partial class Simple {
	/// <summary>
	///  Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	///  Clean up any resources being used.
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
	///  Required method for Designer support - do not modify
	///  the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent() {
		_nLogViewerButton = new Button();
		flowLayoutPanel1 = new FlowLayoutPanel();
		flowLayoutPanel1.SuspendLayout();
		SuspendLayout();
		// 
		// _nLogViewerButton
		// 
		_nLogViewerButton.Location = new Point(3, 3);
		_nLogViewerButton.Name = "_nLogViewerButton";
		_nLogViewerButton.Size = new Size(91, 44);
		_nLogViewerButton.TabIndex = 0;
		_nLogViewerButton.Text = "NLog Viewer";
		_nLogViewerButton.UseVisualStyleBackColor = true;
		// 
		// flowLayoutPanel1
		// 
		flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		flowLayoutPanel1.AutoScroll = true;
		flowLayoutPanel1.Controls.Add(_nLogViewerButton);
		flowLayoutPanel1.Location = new Point(12, 12);
		flowLayoutPanel1.Name = "flowLayoutPanel1";
		flowLayoutPanel1.Size = new Size(564, 212);
		flowLayoutPanel1.TabIndex = 1;
		// 
		// Simple
		// 
		this.AutoScaleDimensions = new SizeF(7F, 15F);
		this.AutoScaleMode = AutoScaleMode.Font;
		this.ClientSize = new Size(588, 368);
		this.Controls.Add(flowLayoutPanel1);
		this.MinimumSize = new Size(140, 248);
		this.Name = "Simple";
		this.Text = "Simple";
		flowLayoutPanel1.ResumeLayout(false);
		ResumeLayout(false);
	}

	#endregion

	private Button _nLogViewerButton;
	private FlowLayoutPanel flowLayoutPanel1;
}
