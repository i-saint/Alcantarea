namespace primitive.Alcantarea
{
    partial class SymbolFilterWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SymbolFilterWindow));
            this.symbolDataGridView = new System.Windows.Forms.DataGridView();
            this.Uppdate = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.LinkToLocal = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Handler = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.NameShort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AttrDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AttrLinkType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AttrAccessType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NameWithSignature = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NameMangled = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Blank = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.symbolFilterDataBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textPath = new System.Windows.Forms.TextBox();
            this.buttonDefault = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.symbolDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.symbolFilterDataBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // symbolDataGridView
            // 
            this.symbolDataGridView.AllowUserToAddRows = false;
            this.symbolDataGridView.AllowUserToDeleteRows = false;
            this.symbolDataGridView.AllowUserToResizeRows = false;
            this.symbolDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.symbolDataGridView.AutoGenerateColumns = false;
            this.symbolDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.symbolDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Uppdate,
            this.LinkToLocal,
            this.Handler,
            this.NameShort,
            this.AttrDataType,
            this.AttrLinkType,
            this.AttrAccessType,
            this.NameWithSignature,
            this.NameMangled,
            this.Blank});
            this.symbolDataGridView.DataSource = this.symbolFilterDataBindingSource;
            this.symbolDataGridView.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.symbolDataGridView.Location = new System.Drawing.Point(12, 38);
            this.symbolDataGridView.Name = "symbolDataGridView";
            this.symbolDataGridView.RowHeadersVisible = false;
            this.symbolDataGridView.RowTemplate.Height = 21;
            this.symbolDataGridView.Size = new System.Drawing.Size(968, 492);
            this.symbolDataGridView.TabIndex = 1;
            this.symbolDataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.symbolDataGridView_CellEndEdit);
            this.symbolDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.symbolGridView_CellValueChanged);
            this.symbolDataGridView.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.symbolGridView_ColumnHeaderMouseClick);
            this.symbolDataGridView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.symbolGridView_KeyUp);
            // 
            // Uppdate
            // 
            this.Uppdate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Uppdate.DataPropertyName = "FlagUpdate";
            this.Uppdate.HeaderText = "Uppdate";
            this.Uppdate.Name = "Uppdate";
            this.Uppdate.Width = 53;
            // 
            // LinkToLocal
            // 
            this.LinkToLocal.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.LinkToLocal.DataPropertyName = "FlagLinkToLocal";
            this.LinkToLocal.HeaderText = "LinkToLocal";
            this.LinkToLocal.Name = "LinkToLocal";
            this.LinkToLocal.Width = 72;
            // 
            // Handler
            // 
            this.Handler.DataPropertyName = "Handler";
            this.Handler.HeaderText = "Handler";
            this.Handler.Items.AddRange(new object[] {
            "",
            "OnLoad",
            "OnUnload"});
            this.Handler.Name = "Handler";
            this.Handler.Width = 80;
            // 
            // NameShort
            // 
            this.NameShort.DataPropertyName = "Name";
            this.NameShort.HeaderText = "Name";
            this.NameShort.Name = "NameShort";
            this.NameShort.ReadOnly = true;
            this.NameShort.Width = 450;
            // 
            // AttrDataType
            // 
            this.AttrDataType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.AttrDataType.DataPropertyName = "AttrDataType";
            this.AttrDataType.HeaderText = "Type";
            this.AttrDataType.Name = "AttrDataType";
            this.AttrDataType.ReadOnly = true;
            this.AttrDataType.Width = 55;
            // 
            // AttrLinkType
            // 
            this.AttrLinkType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.AttrLinkType.DataPropertyName = "AttrLinkType";
            this.AttrLinkType.HeaderText = "Linkage";
            this.AttrLinkType.Name = "AttrLinkType";
            this.AttrLinkType.ReadOnly = true;
            this.AttrLinkType.Width = 69;
            // 
            // AttrAccessType
            // 
            this.AttrAccessType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.AttrAccessType.DataPropertyName = "AttrAccessType";
            this.AttrAccessType.HeaderText = "Access";
            this.AttrAccessType.Name = "AttrAccessType";
            this.AttrAccessType.ReadOnly = true;
            this.AttrAccessType.Width = 68;
            // 
            // NameWithSignature
            // 
            this.NameWithSignature.DataPropertyName = "NameWithSignature";
            this.NameWithSignature.HeaderText = "Name (With Signature)";
            this.NameWithSignature.Name = "NameWithSignature";
            this.NameWithSignature.ReadOnly = true;
            this.NameWithSignature.Width = 400;
            // 
            // NameMangled
            // 
            this.NameMangled.DataPropertyName = "NameMangled";
            this.NameMangled.HeaderText = "Name (Mangled)";
            this.NameMangled.MaxInputLength = 4096;
            this.NameMangled.Name = "NameMangled";
            this.NameMangled.ReadOnly = true;
            this.NameMangled.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.NameMangled.Width = 300;
            // 
            // Blank
            // 
            this.Blank.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Blank.HeaderText = "";
            this.Blank.Name = "Blank";
            this.Blank.ReadOnly = true;
            this.Blank.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // symbolFilterDataBindingSource
            // 
            this.symbolFilterDataBindingSource.DataSource = typeof(primitive.Alcantarea.alcSymbolFilterColumn);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(799, 548);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(905, 548);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textPath
            // 
            this.textPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textPath.Location = new System.Drawing.Point(13, 13);
            this.textPath.Name = "textPath";
            this.textPath.ReadOnly = true;
            this.textPath.Size = new System.Drawing.Size(967, 19);
            this.textPath.TabIndex = 4;
            // 
            // buttonDefault
            // 
            this.buttonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDefault.Location = new System.Drawing.Point(13, 548);
            this.buttonDefault.Name = "buttonDefault";
            this.buttonDefault.Size = new System.Drawing.Size(75, 23);
            this.buttonDefault.TabIndex = 5;
            this.buttonDefault.Text = "Default";
            this.buttonDefault.UseVisualStyleBackColor = true;
            this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
            // 
            // SymbolFilterWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(992, 586);
            this.Controls.Add(this.buttonDefault);
            this.Controls.Add(this.textPath);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.symbolDataGridView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SymbolFilterWindow";
            this.Text = "Symbol Filter";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.SymbolFilterWindow_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.symbolDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.symbolFilterDataBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView symbolDataGridView;
        private System.Windows.Forms.BindingSource symbolFilterDataBindingSource;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TextBox textPath;
        private System.Windows.Forms.DataGridViewCheckBoxColumn updateDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn LinkToLocalDataGridViewCheckBoxColumn;
        private System.Windows.Forms.Button buttonDefault;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Uppdate;
        private System.Windows.Forms.DataGridViewCheckBoxColumn LinkToLocal;
        private System.Windows.Forms.DataGridViewComboBoxColumn Handler;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameShort;
        private System.Windows.Forms.DataGridViewTextBoxColumn AttrDataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn AttrLinkType;
        private System.Windows.Forms.DataGridViewTextBoxColumn AttrAccessType;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameWithSignature;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameMangled;
        private System.Windows.Forms.DataGridViewTextBoxColumn Blank;

    }
}