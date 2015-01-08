namespace primitive.Alcantarea
{
    partial class OptionWindow
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Environment");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Symbol Filter");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("About");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionWindow));
            this.treeNavi = new System.Windows.Forms.TreeView();
            this.panelSymIgnore = new System.Windows.Forms.Panel();
            this.groupBoxSymIgnore = new System.Windows.Forms.GroupBox();
            this.buttonSymIgnoreDefault = new System.Windows.Forms.Button();
            this.labelSymIgnore = new System.Windows.Forms.Label();
            this.dataGridViewSymIgnore = new System.Windows.Forms.DataGridView();
            this.InExclude = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.patternDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.functionDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.variableDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Blank = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.alcIgnoreSymbolColumnBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.panelEnvironment = new System.Windows.Forms.Panel();
            this.groupBoxEnvironment = new System.Windows.Forms.GroupBox();
            this.checkBoxHook = new System.Windows.Forms.CheckBox();
            this.labelTCPPort = new System.Windows.Forms.Label();
            this.textBoxTCPPort = new System.Windows.Forms.TextBox();
            this.panelAbout = new System.Windows.Forms.Panel();
            this.groupBoxAbout = new System.Windows.Forms.GroupBox();
            this.labelAbout = new System.Windows.Forms.Label();
            this.linkLabelWeb = new System.Windows.Forms.LinkLabel();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panelSymIgnore.SuspendLayout();
            this.groupBoxSymIgnore.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSymIgnore)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.alcIgnoreSymbolColumnBindingSource)).BeginInit();
            this.panelEnvironment.SuspendLayout();
            this.groupBoxEnvironment.SuspendLayout();
            this.panelAbout.SuspendLayout();
            this.groupBoxAbout.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeNavi
            // 
            this.treeNavi.Location = new System.Drawing.Point(12, 13);
            this.treeNavi.Name = "treeNavi";
            treeNode1.Name = "Environment";
            treeNode1.Text = "Environment";
            treeNode2.Name = "SymbolFilter";
            treeNode2.Text = "Symbol Filter";
            treeNode3.Name = "About";
            treeNode3.Text = "About";
            this.treeNavi.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3});
            this.treeNavi.PathSeparator = "/";
            this.treeNavi.ShowLines = false;
            this.treeNavi.Size = new System.Drawing.Size(130, 358);
            this.treeNavi.TabIndex = 0;
            this.treeNavi.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeNavi_AfterSelect);
            // 
            // panelSymIgnore
            // 
            this.panelSymIgnore.Controls.Add(this.groupBoxSymIgnore);
            this.panelSymIgnore.Location = new System.Drawing.Point(148, 13);
            this.panelSymIgnore.Name = "panelSymIgnore";
            this.panelSymIgnore.Size = new System.Drawing.Size(597, 359);
            this.panelSymIgnore.TabIndex = 1;
            // 
            // groupBoxSymIgnore
            // 
            this.groupBoxSymIgnore.Controls.Add(this.buttonSymIgnoreDefault);
            this.groupBoxSymIgnore.Controls.Add(this.labelSymIgnore);
            this.groupBoxSymIgnore.Controls.Add(this.dataGridViewSymIgnore);
            this.groupBoxSymIgnore.Location = new System.Drawing.Point(3, 3);
            this.groupBoxSymIgnore.Name = "groupBoxSymIgnore";
            this.groupBoxSymIgnore.Size = new System.Drawing.Size(591, 352);
            this.groupBoxSymIgnore.TabIndex = 1;
            this.groupBoxSymIgnore.TabStop = false;
            this.groupBoxSymIgnore.Text = "Symbol Filter: Ignore Pattern";
            // 
            // buttonSymIgnoreDefault
            // 
            this.buttonSymIgnoreDefault.Location = new System.Drawing.Point(510, 12);
            this.buttonSymIgnoreDefault.Name = "buttonSymIgnoreDefault";
            this.buttonSymIgnoreDefault.Size = new System.Drawing.Size(75, 25);
            this.buttonSymIgnoreDefault.TabIndex = 4;
            this.buttonSymIgnoreDefault.Text = "Default";
            this.buttonSymIgnoreDefault.UseVisualStyleBackColor = true;
            this.buttonSymIgnoreDefault.Click += new System.EventHandler(this.buttonSymIgnoreDefault_Click);
            // 
            // labelSymIgnore
            // 
            this.labelSymIgnore.AutoSize = true;
            this.labelSymIgnore.Location = new System.Drawing.Point(12, 24);
            this.labelSymIgnore.Name = "labelSymIgnore";
            this.labelSymIgnore.Size = new System.Drawing.Size(307, 13);
            this.labelSymIgnore.TabIndex = 1;
            this.labelSymIgnore.Text = "Symbols that match these patterns are not shown in symbol filter";
            // 
            // dataGridViewSymIgnore
            // 
            this.dataGridViewSymIgnore.AllowUserToResizeRows = false;
            this.dataGridViewSymIgnore.AutoGenerateColumns = false;
            this.dataGridViewSymIgnore.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSymIgnore.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.InExclude,
            this.patternDataGridViewTextBoxColumn,
            this.functionDataGridViewCheckBoxColumn,
            this.variableDataGridViewCheckBoxColumn,
            this.Blank});
            this.dataGridViewSymIgnore.DataSource = this.alcIgnoreSymbolColumnBindingSource;
            this.dataGridViewSymIgnore.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.dataGridViewSymIgnore.Location = new System.Drawing.Point(6, 43);
            this.dataGridViewSymIgnore.Name = "dataGridViewSymIgnore";
            this.dataGridViewSymIgnore.RowTemplate.Height = 21;
            this.dataGridViewSymIgnore.Size = new System.Drawing.Size(579, 302);
            this.dataGridViewSymIgnore.TabIndex = 0;
            this.dataGridViewSymIgnore.KeyUp += new System.Windows.Forms.KeyEventHandler(this.symbolGridView_KeyUp);
            // 
            // InExclude
            // 
            this.InExclude.DataPropertyName = "Inclusion";
            this.InExclude.HeaderText = "In/Exclude";
            this.InExclude.Items.AddRange(new object[] {
            "Include",
            "Exclude"});
            this.InExclude.Name = "InExclude";
            this.InExclude.Width = 75;
            // 
            // patternDataGridViewTextBoxColumn
            // 
            this.patternDataGridViewTextBoxColumn.DataPropertyName = "Pattern";
            this.patternDataGridViewTextBoxColumn.HeaderText = "Pattern (regex)";
            this.patternDataGridViewTextBoxColumn.MaxInputLength = 4096;
            this.patternDataGridViewTextBoxColumn.Name = "patternDataGridViewTextBoxColumn";
            this.patternDataGridViewTextBoxColumn.Width = 250;
            // 
            // functionDataGridViewCheckBoxColumn
            // 
            this.functionDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.functionDataGridViewCheckBoxColumn.DataPropertyName = "Function";
            this.functionDataGridViewCheckBoxColumn.HeaderText = "Function";
            this.functionDataGridViewCheckBoxColumn.Name = "functionDataGridViewCheckBoxColumn";
            this.functionDataGridViewCheckBoxColumn.Width = 54;
            // 
            // variableDataGridViewCheckBoxColumn
            // 
            this.variableDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.variableDataGridViewCheckBoxColumn.DataPropertyName = "Variable";
            this.variableDataGridViewCheckBoxColumn.HeaderText = "Variable";
            this.variableDataGridViewCheckBoxColumn.Name = "variableDataGridViewCheckBoxColumn";
            this.variableDataGridViewCheckBoxColumn.Width = 51;
            // 
            // Blank
            // 
            this.Blank.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Blank.HeaderText = "";
            this.Blank.Name = "Blank";
            this.Blank.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Blank.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // alcIgnoreSymbolColumnBindingSource
            // 
            this.alcIgnoreSymbolColumnBindingSource.DataSource = typeof(primitive.Alcantarea.alcGlobalSymFilterColumn);
            // 
            // panelEnvironment
            // 
            this.panelEnvironment.Controls.Add(this.groupBoxEnvironment);
            this.panelEnvironment.Location = new System.Drawing.Point(148, 13);
            this.panelEnvironment.Name = "panelEnvironment";
            this.panelEnvironment.Size = new System.Drawing.Size(597, 359);
            this.panelEnvironment.TabIndex = 1;
            // 
            // groupBoxEnvironment
            // 
            this.groupBoxEnvironment.Controls.Add(this.checkBoxHook);
            this.groupBoxEnvironment.Controls.Add(this.labelTCPPort);
            this.groupBoxEnvironment.Controls.Add(this.textBoxTCPPort);
            this.groupBoxEnvironment.Location = new System.Drawing.Point(9, 3);
            this.groupBoxEnvironment.Name = "groupBoxEnvironment";
            this.groupBoxEnvironment.Size = new System.Drawing.Size(585, 352);
            this.groupBoxEnvironment.TabIndex = 0;
            this.groupBoxEnvironment.TabStop = false;
            this.groupBoxEnvironment.Text = "Environment";
            // 
            // checkBoxHook
            // 
            this.checkBoxHook.AutoSize = true;
            this.checkBoxHook.Location = new System.Drawing.Point(6, 77);
            this.checkBoxHook.Name = "checkBoxHook";
            this.checkBoxHook.Size = new System.Drawing.Size(298, 17);
            this.checkBoxHook.TabIndex = 2;
            this.checkBoxHook.Text = "Hook LoadLibrary() to load symbols for DLLs automatically\r\n";
            this.checkBoxHook.UseVisualStyleBackColor = true;
            // 
            // labelTCPPort
            // 
            this.labelTCPPort.AutoSize = true;
            this.labelTCPPort.Location = new System.Drawing.Point(79, 43);
            this.labelTCPPort.Name = "labelTCPPort";
            this.labelTCPPort.Size = new System.Drawing.Size(195, 13);
            this.labelTCPPort.TabIndex = 1;
            this.labelTCPPort.Text = "TCP port to communicate with programs";
            // 
            // textBoxTCPPort
            // 
            this.textBoxTCPPort.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBoxTCPPort.Location = new System.Drawing.Point(8, 40);
            this.textBoxTCPPort.MaxLength = 5;
            this.textBoxTCPPort.Name = "textBoxTCPPort";
            this.textBoxTCPPort.Size = new System.Drawing.Size(65, 20);
            this.textBoxTCPPort.TabIndex = 0;
            this.textBoxTCPPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxTCPPort_KeyPress);
            // 
            // panelAbout
            // 
            this.panelAbout.Controls.Add(this.groupBoxAbout);
            this.panelAbout.Location = new System.Drawing.Point(148, 13);
            this.panelAbout.Name = "panelAbout";
            this.panelAbout.Size = new System.Drawing.Size(597, 359);
            this.panelAbout.TabIndex = 2;
            // 
            // groupBoxAbout
            // 
            this.groupBoxAbout.Controls.Add(this.labelAbout);
            this.groupBoxAbout.Controls.Add(this.linkLabelWeb);
            this.groupBoxAbout.Location = new System.Drawing.Point(9, 3);
            this.groupBoxAbout.Name = "groupBoxAbout";
            this.groupBoxAbout.Size = new System.Drawing.Size(585, 352);
            this.groupBoxAbout.TabIndex = 0;
            this.groupBoxAbout.TabStop = false;
            this.groupBoxAbout.Text = "About";
            // 
            // labelAbout
            // 
            this.labelAbout.AutoSize = true;
            this.labelAbout.Location = new System.Drawing.Point(6, 24);
            this.labelAbout.Name = "labelAbout";
            this.labelAbout.Size = new System.Drawing.Size(192, 39);
            this.labelAbout.TabIndex = 1;
            this.labelAbout.Text = "Alcantarea 1.0.2 (2015/01/08)\r\n\r\n(c) Copyright primitive all rights reserved";
            // 
            // linkLabelWeb
            // 
            this.linkLabelWeb.AutoSize = true;
            this.linkLabelWeb.Location = new System.Drawing.Point(6, 77);
            this.linkLabelWeb.Name = "linkLabelWeb";
            this.linkLabelWeb.Size = new System.Drawing.Size(181, 13);
            this.linkLabelWeb.TabIndex = 0;
            this.linkLabelWeb.TabStop = true;
            this.linkLabelWeb.Text = "http://primitive-games.jp/alcantarea/";
            this.linkLabelWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelWeb_LinkClicked);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(426, 378);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 25);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(580, 378);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 25);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // OptionWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(757, 416);
            this.Controls.Add(this.panelAbout);
            this.Controls.Add(this.panelEnvironment);
            this.Controls.Add(this.panelSymIgnore);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.treeNavi);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OptionWindow";
            this.Text = "Alcantarea Options";
            this.panelSymIgnore.ResumeLayout(false);
            this.groupBoxSymIgnore.ResumeLayout(false);
            this.groupBoxSymIgnore.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSymIgnore)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.alcIgnoreSymbolColumnBindingSource)).EndInit();
            this.panelEnvironment.ResumeLayout(false);
            this.groupBoxEnvironment.ResumeLayout(false);
            this.groupBoxEnvironment.PerformLayout();
            this.panelAbout.ResumeLayout(false);
            this.groupBoxAbout.ResumeLayout(false);
            this.groupBoxAbout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeNavi;
        private System.Windows.Forms.Panel panelSymIgnore;
        private System.Windows.Forms.GroupBox groupBoxSymIgnore;
        private System.Windows.Forms.DataGridView dataGridViewSymIgnore;
        private System.Windows.Forms.Panel panelEnvironment;
        private System.Windows.Forms.Panel panelAbout;
        private System.Windows.Forms.GroupBox groupBoxEnvironment;
        private System.Windows.Forms.GroupBox groupBoxAbout;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelTCPPort;
        private System.Windows.Forms.TextBox textBoxTCPPort;
        private System.Windows.Forms.BindingSource alcIgnoreSymbolColumnBindingSource;
        private System.Windows.Forms.Label labelSymIgnore;
        private System.Windows.Forms.Label labelAbout;
        private System.Windows.Forms.LinkLabel linkLabelWeb;
        private System.Windows.Forms.Button buttonSymIgnoreDefault;
        private System.Windows.Forms.CheckBox checkBoxHook;
        private System.Windows.Forms.DataGridViewComboBoxColumn InExclude;
        private System.Windows.Forms.DataGridViewTextBoxColumn patternDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn functionDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn variableDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Blank;
    }
}