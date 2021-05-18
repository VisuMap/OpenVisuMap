namespace VisuMap.DataModeling {
    partial class ModelManager2 {
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
            this.components = new System.ComponentModel.Container();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnRename = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.listView = new System.Windows.Forms.ListView();
            this.hdModelName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hdDataset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hdMap = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hdTarget = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hdEpochs = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hdLastUpdate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnSetWorkDir = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Location = new System.Drawing.Point(9, 187);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(92, 36);
            this.btnDelete.TabIndex = 0;
            this.btnDelete.Text = "Delete Selected";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnImport
            // 
            this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnImport.Location = new System.Drawing.Point(107, 187);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(91, 36);
            this.btnImport.TabIndex = 0;
            this.btnImport.Text = "Import Models";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExport.Location = new System.Drawing.Point(204, 187);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(84, 36);
            this.btnExport.TabIndex = 0;
            this.btnExport.Text = "Export Models";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(507, 187);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(49, 36);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnRename
            // 
            this.btnRename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRename.Location = new System.Drawing.Point(294, 187);
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(63, 36);
            this.btnRename.TabIndex = 0;
            this.btnRename.Text = "Rename";
            this.toolTip1.SetToolTip(this.btnRename, "Rename the first selected model.");
            this.btnRename.UseVisualStyleBackColor = true;
            this.btnRename.Click += new System.EventHandler(this.btnRename_Click);
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.hdModelName,
            this.hdDataset,
            this.hdMap,
            this.hdTarget,
            this.hdEpochs,
            this.hdLastUpdate});
            this.listView.FullRowSelect = true;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(568, 183);
            this.listView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listView.TabIndex = 5;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_ColumnClick);
            this.listView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listView_KeyPress);
            // 
            // hdModelName
            // 
            this.hdModelName.Text = "Model Name";
            this.hdModelName.Width = 107;
            // 
            // hdDataset
            // 
            this.hdDataset.Text = "Dataset";
            this.hdDataset.Width = 80;
            // 
            // hdMap
            // 
            this.hdMap.Text = "Map";
            // 
            // hdTarget
            // 
            this.hdTarget.Text = "Target";
            this.hdTarget.Width = 67;
            // 
            // hdEpochs
            // 
            this.hdEpochs.Text = "Epochs";
            this.hdEpochs.Width = 57;
            // 
            // hdLastUpdate
            // 
            this.hdLastUpdate.Text = "Last Update";
            this.hdLastUpdate.Width = 88;
            // 
            // btnSetWorkDir
            // 
            this.btnSetWorkDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetWorkDir.Location = new System.Drawing.Point(363, 187);
            this.btnSetWorkDir.Name = "btnSetWorkDir";
            this.btnSetWorkDir.Size = new System.Drawing.Size(63, 36);
            this.btnSetWorkDir.TabIndex = 0;
            this.btnSetWorkDir.Text = "Work Dir";
            this.toolTip1.SetToolTip(this.btnSetWorkDir, "Change working directory.");
            this.btnSetWorkDir.UseVisualStyleBackColor = true;
            this.btnSetWorkDir.Click += new System.EventHandler(this.btnSetWorkDir_Click);
            // 
            // ModelManager2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 225);
            this.Controls.Add(this.listView);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSetWorkDir);
            this.Controls.Add(this.btnRename);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnDelete);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ModelManager2";
            this.Text = "Model Manager";
            this.toolTip1.SetToolTip(this, "Set Working/Project directory");
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnRename;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader hdModelName;
        private System.Windows.Forms.ColumnHeader hdDataset;
        private System.Windows.Forms.ColumnHeader hdMap;
        private System.Windows.Forms.ColumnHeader hdTarget;
        private System.Windows.Forms.ColumnHeader hdEpochs;
        private System.Windows.Forms.ColumnHeader hdLastUpdate;
        private System.Windows.Forms.Button btnSetWorkDir;
    }
}