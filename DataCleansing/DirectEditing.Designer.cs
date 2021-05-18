namespace VisuMap.DataCleansing {
    partial class DirectEditing {
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miImport = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miTrim = new System.Windows.Forms.ToolStripMenuItem();
            this.miShowSeparator = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.cutLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miReplace = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miWrapLines = new System.Windows.Forms.ToolStripMenuItem();
            this.miLargeText = new System.Windows.Forms.ToolStripMenuItem();
            this.miNormalText = new System.Windows.Forms.ToolStripMenuItem();
            this.miSmallText = new System.Windows.Forms.ToolStripMenuItem();
            this.dataPanel = new System.Windows.Forms.RichTextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.windowToolStripMenuItem});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.menuStrip1.Size = new System.Drawing.Size(467, 21);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.miImport,
            this.toolStripMenuItem3,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 17);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.openToolStripMenuItem.Text = "&Open...";
            this.openToolStripMenuItem.ToolTipText = "Load a CSV file.";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // miImport
            // 
            this.miImport.Name = "miImport";
            this.miImport.Size = new System.Drawing.Size(152, 22);
            this.miImport.Text = "Import data...";
            this.miImport.ToolTipText = "Import selected (or all) data.";
            this.miImport.Click += new System.EventHandler(this.miImport_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(179, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miTrim,
            this.miShowSeparator,
            this.toolStripSeparator1,
            this.undoToolStripMenuItem,
            this.cutToolStripMenuItem2,
            this.copyToolStripMenuItem1,
            this.pasteToolStripMenuItem1,
            this.cutLineToolStripMenuItem,
            this.toolStripMenuItem4,
            this.findToolStripMenuItem,
            this.findNextToolStripMenuItem,
            this.miReplace});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(37, 17);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // miTrim
            // 
            this.miTrim.Name = "miTrim";
            this.miTrim.Size = new System.Drawing.Size(156, 22);
            this.miTrim.Text = "Trim Lines";
            this.miTrim.Click += new System.EventHandler(this.miTrim_Click);
            // 
            // miShowSeparator
            // 
            this.miShowSeparator.Name = "miShowSeparator";
            this.miShowSeparator.Size = new System.Drawing.Size(156, 22);
            this.miShowSeparator.Text = "Show Separators";
            this.miShowSeparator.ToolTipText = "Make the separators visible.";
            this.miShowSeparator.Click += new System.EventHandler(this.miShowSeparator_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(153, 6);
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.undoToolStripMenuItem.Text = "&Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // cutToolStripMenuItem2
            // 
            this.cutToolStripMenuItem2.Name = "cutToolStripMenuItem2";
            this.cutToolStripMenuItem2.Size = new System.Drawing.Size(156, 22);
            this.cutToolStripMenuItem2.Text = "Cu&t";
            this.cutToolStripMenuItem2.Click += new System.EventHandler(this.cutToolStripMenuItem2_Click);
            // 
            // copyToolStripMenuItem1
            // 
            this.copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
            this.copyToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem1.Size = new System.Drawing.Size(156, 22);
            this.copyToolStripMenuItem1.Text = "&Copy";
            this.copyToolStripMenuItem1.Click += new System.EventHandler(this.copyToolStripMenuItem1_Click);
            // 
            // pasteToolStripMenuItem1
            // 
            this.pasteToolStripMenuItem1.Name = "pasteToolStripMenuItem1";
            this.pasteToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem1.Size = new System.Drawing.Size(156, 22);
            this.pasteToolStripMenuItem1.Text = "&Paste";
            this.pasteToolStripMenuItem1.Click += new System.EventHandler(this.pasteToolStripMenuItem1_Click);
            // 
            // cutLineToolStripMenuItem
            // 
            this.cutLineToolStripMenuItem.Name = "cutLineToolStripMenuItem";
            this.cutLineToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.cutLineToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.cutLineToolStripMenuItem.Text = "Cut &Line";
            this.cutLineToolStripMenuItem.Click += new System.EventHandler(this.cutLineToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(153, 6);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.findToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.findToolStripMenuItem.Text = "&Find...";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.miFind_Click);
            // 
            // findNextToolStripMenuItem
            // 
            this.findNextToolStripMenuItem.Name = "findNextToolStripMenuItem";
            this.findNextToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.findNextToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.findNextToolStripMenuItem.Text = "Find &Next";
            this.findNextToolStripMenuItem.Click += new System.EventHandler(this.miFindNext_Click);
            // 
            // miReplace
            // 
            this.miReplace.Name = "miReplace";
            this.miReplace.Size = new System.Drawing.Size(156, 22);
            this.miReplace.Text = "Repalce...";
            this.miReplace.Click += new System.EventHandler(this.miReplace_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miWrapLines,
            this.miLargeText,
            this.miNormalText,
            this.miSmallText});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(57, 17);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // miWrapLines
            // 
            this.miWrapLines.Name = "miWrapLines";
            this.miWrapLines.Size = new System.Drawing.Size(132, 22);
            this.miWrapLines.Text = "Wrap Lines";
            this.miWrapLines.Click += new System.EventHandler(this.miWrapLines_Click);
            // 
            // miLargeText
            // 
            this.miLargeText.Name = "miLargeText";
            this.miLargeText.Size = new System.Drawing.Size(132, 22);
            this.miLargeText.Text = "Large Text";
            this.miLargeText.Click += new System.EventHandler(this.miLargeText_Click);
            // 
            // miNormalText
            // 
            this.miNormalText.Checked = true;
            this.miNormalText.CheckState = System.Windows.Forms.CheckState.Checked;
            this.miNormalText.Name = "miNormalText";
            this.miNormalText.Size = new System.Drawing.Size(132, 22);
            this.miNormalText.Text = "Normal Text";
            this.miNormalText.Click += new System.EventHandler(this.miNormalText_Click);
            // 
            // miSmallText
            // 
            this.miSmallText.Name = "miSmallText";
            this.miSmallText.Size = new System.Drawing.Size(132, 22);
            this.miSmallText.Text = "Small Text";
            this.miSmallText.Click += new System.EventHandler(this.miSmallText_Click);
            // 
            // dataPanel
            // 
            this.dataPanel.AcceptsTab = true;
            this.dataPanel.AutoWordSelection = true;
            this.dataPanel.BackColor = System.Drawing.Color.White;
            this.dataPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataPanel.DetectUrls = false;
            this.dataPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataPanel.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataPanel.HideSelection = false;
            this.dataPanel.Location = new System.Drawing.Point(0, 21);
            this.dataPanel.Name = "dataPanel";
            this.dataPanel.Size = new System.Drawing.Size(467, 340);
            this.dataPanel.TabIndex = 5;
            this.dataPanel.Text = "";
            this.dataPanel.WordWrap = false;
            // 
            // DirectEditing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 361);
            this.Controls.Add(this.dataPanel);
            this.Controls.Add(this.menuStrip1);
            this.Name = "DirectEditing";
            this.RightToLeftLayout = true;
            this.Text = "Data Source Editing";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem cutLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextToolStripMenuItem;
        private System.Windows.Forms.RichTextBox dataPanel;
        private System.Windows.Forms.ToolStripMenuItem miTrim;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miWrapLines;
        private System.Windows.Forms.ToolStripMenuItem miImport;
        private System.Windows.Forms.ToolStripMenuItem miLargeText;
        private System.Windows.Forms.ToolStripMenuItem miNormalText;
        private System.Windows.Forms.ToolStripMenuItem miSmallText;
        private System.Windows.Forms.ToolStripMenuItem miShowSeparator;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem miReplace;
    }
}