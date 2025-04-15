namespace Praxis.WinForm.UserControl;

partial class PropertyGridder
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        TabControl mainTabControl;
        TabPage tabPage1;
        TabPage tabPage2;
        mainPropertyGrid = new PropertyGrid();
        changesTextBox = new TextBox();
        controlsComboBox = new ComboBox();
        topPanel = new Panel();
        viewPropDifferencesButton = new Button();
        mainTabControl = new TabControl();
        tabPage1 = new TabPage();
        tabPage2 = new TabPage();
        mainTabControl.SuspendLayout();
        tabPage1.SuspendLayout();
        tabPage2.SuspendLayout();
        topPanel.SuspendLayout();
        SuspendLayout();
        // 
        // mainTabControl
        // 
        mainTabControl.Controls.Add(tabPage1);
        mainTabControl.Controls.Add(tabPage2);
        mainTabControl.Dock = DockStyle.Fill;
        mainTabControl.Location = new Point(0, 42);
        mainTabControl.Name = "mainTabControl";
        mainTabControl.SelectedIndex = 0;
        mainTabControl.Size = new Size(348, 688);
        mainTabControl.TabIndex = 7;
        mainTabControl.TabStop = false;
        // 
        // tabPage1
        // 
        tabPage1.Controls.Add(mainPropertyGrid);
        tabPage1.Location = new Point(4, 24);
        tabPage1.Name = "tabPage1";
        tabPage1.Padding = new Padding(3);
        tabPage1.Size = new Size(340, 660);
        tabPage1.TabIndex = 0;
        tabPage1.Text = "Grid";
        tabPage1.UseVisualStyleBackColor = true;
        // 
        // mainPropertyGrid
        // 
        mainPropertyGrid.Dock = DockStyle.Fill;
        mainPropertyGrid.Location = new Point(3, 3);
        mainPropertyGrid.Name = "mainPropertyGrid";
        mainPropertyGrid.Size = new Size(334, 654);
        mainPropertyGrid.TabIndex = 4;
        mainPropertyGrid.TabStop = false;
        // 
        // tabPage2
        // 
        tabPage2.Controls.Add(changesTextBox);
        tabPage2.Location = new Point(4, 24);
        tabPage2.Name = "tabPage2";
        tabPage2.Padding = new Padding(3);
        tabPage2.Size = new Size(340, 660);
        tabPage2.TabIndex = 1;
        tabPage2.Text = "Changes";
        tabPage2.UseVisualStyleBackColor = true;
        // 
        // changesTextBox
        // 
        changesTextBox.AcceptsReturn = true;
        changesTextBox.AcceptsTab = true;
        changesTextBox.BorderStyle = BorderStyle.None;
        changesTextBox.CausesValidation = false;
        changesTextBox.Dock = DockStyle.Fill;
        changesTextBox.Font = new Font("Lucida Console", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        changesTextBox.Location = new Point(3, 3);
        changesTextBox.MaxLength = 50000;
        changesTextBox.Multiline = true;
        changesTextBox.Name = "changesTextBox";
        changesTextBox.ScrollBars = ScrollBars.Both;
        changesTextBox.Size = new Size(334, 654);
        changesTextBox.TabIndex = 0;
        changesTextBox.TabStop = false;
        changesTextBox.WordWrap = false;
        // 
        // controlsComboBox
        // 
        controlsComboBox.DisplayMember = "Name";
        controlsComboBox.Dock = DockStyle.Fill;
        controlsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        controlsComboBox.FormattingEnabled = true;
        controlsComboBox.Location = new Point(30, 10);
        controlsComboBox.Name = "controlsComboBox";
        controlsComboBox.Size = new Size(312, 23);
        controlsComboBox.TabIndex = 5;
        controlsComboBox.TabStop = false;
        // 
        // topPanel
        // 
        topPanel.Controls.Add(viewPropDifferencesButton);
        topPanel.Controls.Add(controlsComboBox);
        topPanel.Dock = DockStyle.Top;
        topPanel.Location = new Point(0, 0);
        topPanel.Name = "topPanel";
        topPanel.Padding = new Padding(30, 10, 6, 0);
        topPanel.Size = new Size(348, 42);
        topPanel.TabIndex = 6;
        // 
        // viewPropDifferencesButton
        // 
        viewPropDifferencesButton.BackColor = Color.SandyBrown;
        viewPropDifferencesButton.Location = new Point(6, 12);
        viewPropDifferencesButton.Name = "viewPropDifferencesButton";
        viewPropDifferencesButton.Size = new Size(18, 18);
        viewPropDifferencesButton.TabIndex = 6;
        viewPropDifferencesButton.TabStop = false;
        viewPropDifferencesButton.UseVisualStyleBackColor = false;
        // 
        // PropertyGridder
        // 
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(348, 730);
        this.Controls.Add(mainTabControl);
        this.Controls.Add(topPanel);
        this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
        this.Name = "PropertyGridder";
        this.ShowIcon = false;
        this.StartPosition = FormStartPosition.Manual;
        this.Text = "Property Gridder";
        mainTabControl.ResumeLayout(false);
        tabPage1.ResumeLayout(false);
        tabPage2.ResumeLayout(false);
        tabPage2.PerformLayout();
        topPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion
    private PropertyGrid mainPropertyGrid;
    private ComboBox controlsComboBox;
    private Panel topPanel;
    private Button viewPropDifferencesButton;
    private TextBox changesTextBox;
}