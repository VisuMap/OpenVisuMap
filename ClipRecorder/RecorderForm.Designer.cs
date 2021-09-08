namespace ClipRecorder {
    partial class RecorderForm {
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
            this.labelMax = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnToEnd = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnPlayStop = new System.Windows.Forms.Button();
            this.btnRecording = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.progressPanel = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.labelCurrentFrame = new System.Windows.Forms.Label();
            this.clipTitle = new System.Windows.Forms.Label();
            this.ctxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miCaptureFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.miAppendFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.miDeleteFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.miRefreshFrame = new System.Windows.Forms.ToolStripMenuItem();
            this.miInterpolation = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelpInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.miRunScript = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.miProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelMax
            // 
            this.labelMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMax.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMax.ForeColor = System.Drawing.Color.Navy;
            this.labelMax.Location = new System.Drawing.Point(331, 7);
            this.labelMax.Margin = new System.Windows.Forms.Padding(0);
            this.labelMax.Name = "labelMax";
            this.labelMax.Size = new System.Drawing.Size(44, 14);
            this.labelMax.TabIndex = 7;
            this.labelMax.Text = "0";
            this.labelMax.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnLoad.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnLoad.FlatAppearance.BorderSize = 0;
            this.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoad.ForeColor = System.Drawing.Color.Transparent;
            this.btnLoad.Image = global::ClipRecorder.Properties.Resources.LoadRecord;
            this.btnLoad.Location = new System.Drawing.Point(335, 58);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(16, 16);
            this.btnLoad.TabIndex = 8;
            this.toolTip1.SetToolTip(this.btnLoad, "load a clip.");
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.ForeColor = System.Drawing.Color.Transparent;
            this.btnClose.Image = global::ClipRecorder.Properties.Resources.CloseWindow;
            this.btnClose.Location = new System.Drawing.Point(359, 58);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(16, 16);
            this.btnClose.TabIndex = 9;
            this.toolTip1.SetToolTip(this.btnClose, "Close the window.");
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnToEnd
            // 
            this.btnToEnd.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnToEnd.FlatAppearance.BorderSize = 0;
            this.btnToEnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnToEnd.ForeColor = System.Drawing.Color.Transparent;
            this.btnToEnd.Image = global::ClipRecorder.Properties.Resources.ToEnd;
            this.btnToEnd.Location = new System.Drawing.Point(203, 50);
            this.btnToEnd.Name = "btnToEnd";
            this.btnToEnd.Size = new System.Drawing.Size(24, 24);
            this.btnToEnd.TabIndex = 5;
            this.toolTip1.SetToolTip(this.btnToEnd, "Move foreward to the end.");
            this.btnToEnd.UseVisualStyleBackColor = true;
            this.btnToEnd.Click += new System.EventHandler(this.btnToEnd_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.ForeColor = System.Drawing.Color.Transparent;
            this.btnSave.Image = global::ClipRecorder.Properties.Resources.SaveRecord;
            this.btnSave.Location = new System.Drawing.Point(311, 58);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(16, 16);
            this.btnSave.TabIndex = 7;
            this.toolTip1.SetToolTip(this.btnSave, "Save the clip.");
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnPlayStop
            // 
            this.btnPlayStop.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnPlayStop.FlatAppearance.BorderSize = 0;
            this.btnPlayStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlayStop.ForeColor = System.Drawing.Color.Transparent;
            this.btnPlayStop.Image = global::ClipRecorder.Properties.Resources.Play;
            this.btnPlayStop.Location = new System.Drawing.Point(179, 50);
            this.btnPlayStop.Name = "btnPlayStop";
            this.btnPlayStop.Size = new System.Drawing.Size(24, 24);
            this.btnPlayStop.TabIndex = 4;
            this.toolTip1.SetToolTip(this.btnPlayStop, "start/stop replay.");
            this.btnPlayStop.UseVisualStyleBackColor = true;
            this.btnPlayStop.Click += new System.EventHandler(this.btnPlayStop_Click);
            // 
            // btnRecording
            // 
            this.btnRecording.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnRecording.FlatAppearance.BorderSize = 0;
            this.btnRecording.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecording.ForeColor = System.Drawing.Color.Transparent;
            this.btnRecording.Image = global::ClipRecorder.Properties.Resources.NotRecording;
            this.btnRecording.Location = new System.Drawing.Point(5, 58);
            this.btnRecording.Name = "btnRecording";
            this.btnRecording.Size = new System.Drawing.Size(16, 16);
            this.btnRecording.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btnRecording, "Start/stop recording.");
            this.btnRecording.UseVisualStyleBackColor = true;
            this.btnRecording.Click += new System.EventHandler(this.btnRecording_Click);
            // 
            // btnClearAll
            // 
            this.btnClearAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnClearAll.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnClearAll.FlatAppearance.BorderSize = 0;
            this.btnClearAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearAll.ForeColor = System.Drawing.Color.Transparent;
            this.btnClearAll.Image = global::ClipRecorder.Properties.Resources.ClearAll;
            this.btnClearAll.Location = new System.Drawing.Point(287, 58);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(16, 16);
            this.btnClearAll.TabIndex = 1;
            this.toolTip1.SetToolTip(this.btnClearAll, "Clear all record.");
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
            // 
            // btnReset
            // 
            this.btnReset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnReset.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnReset.FlatAppearance.BorderSize = 0;
            this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReset.ForeColor = System.Drawing.Color.Transparent;
            this.btnReset.Image = global::ClipRecorder.Properties.Resources.Reset;
            this.btnReset.Location = new System.Drawing.Point(155, 50);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(24, 24);
            this.btnReset.TabIndex = 3;
            this.toolTip1.SetToolTip(this.btnReset, "Rewind to the beginning.");
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // progressPanel
            // 
            this.progressPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressPanel.BackColor = System.Drawing.Color.PowderBlue;
            this.progressPanel.Location = new System.Drawing.Point(3, 24);
            this.progressPanel.Name = "progressPanel";
            this.progressPanel.Size = new System.Drawing.Size(373, 20);
            this.progressPanel.TabIndex = 11;
            this.progressPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.progressPanel_MouseDown);
            this.progressPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.progressPanel_MouseMove);
            this.progressPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.progressPanel_MouseUp);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Navy;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(12, 14);
            this.label1.TabIndex = 12;
            this.label1.Text = "1";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelCurrentFrame
            // 
            this.labelCurrentFrame.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCurrentFrame.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCurrentFrame.ForeColor = System.Drawing.Color.Navy;
            this.labelCurrentFrame.Location = new System.Drawing.Point(129, 7);
            this.labelCurrentFrame.Margin = new System.Windows.Forms.Padding(0);
            this.labelCurrentFrame.Name = "labelCurrentFrame";
            this.labelCurrentFrame.Size = new System.Drawing.Size(68, 14);
            this.labelCurrentFrame.TabIndex = 13;
            this.labelCurrentFrame.Text = "0";
            this.labelCurrentFrame.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // clipTitle
            // 
            this.clipTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clipTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clipTitle.Location = new System.Drawing.Point(12, 82);
            this.clipTitle.Name = "clipTitle";
            this.clipTitle.Size = new System.Drawing.Size(356, 20);
            this.clipTitle.TabIndex = 14;
            this.clipTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ctxMenu
            // 
            this.ctxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miCaptureFrame,
            this.miAppendFrame,
            this.miDeleteFrame,
            this.miRefreshFrame,
            this.miInterpolation,
            this.miHelpInfo,
            this.miRunScript,
            this.toolStripSeparator1,
            this.miProperties});
            this.ctxMenu.Name = "ctxMenu";
            this.ctxMenu.Size = new System.Drawing.Size(205, 208);
            // 
            // miCaptureFrame
            // 
            this.miCaptureFrame.Name = "miCaptureFrame";
            this.miCaptureFrame.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.miCaptureFrame.Size = new System.Drawing.Size(204, 22);
            this.miCaptureFrame.Text = "Capture Frame";
            this.miCaptureFrame.ToolTipText = "Capture the map into current frame.";
            this.miCaptureFrame.Click += new System.EventHandler(this.miCaptureFrame_Click);
            // 
            // miAppendFrame
            // 
            this.miAppendFrame.Name = "miAppendFrame";
            this.miAppendFrame.Size = new System.Drawing.Size(204, 22);
            this.miAppendFrame.Text = "Append Frame";
            this.miAppendFrame.Click += new System.EventHandler(this.miAppendFrame_Click);
            // 
            // miDeleteFrame
            // 
            this.miDeleteFrame.Name = "miDeleteFrame";
            this.miDeleteFrame.Size = new System.Drawing.Size(204, 22);
            this.miDeleteFrame.Text = "Delete Frame";
            this.miDeleteFrame.ToolTipText = "Delete the current frame.";
            this.miDeleteFrame.Click += new System.EventHandler(this.miDeleteFrame_Click);
            // 
            // miRefreshFrame
            // 
            this.miRefreshFrame.Name = "miRefreshFrame";
            this.miRefreshFrame.Size = new System.Drawing.Size(204, 22);
            this.miRefreshFrame.Text = "Refresh Frame";
            this.miRefreshFrame.ToolTipText = "Refresh current map with current frame.";
            this.miRefreshFrame.Click += new System.EventHandler(this.miRefreshFrame_Click);
            // 
            // miInterpolation
            // 
            this.miInterpolation.Name = "miInterpolation";
            this.miInterpolation.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.miInterpolation.Size = new System.Drawing.Size(204, 22);
            this.miInterpolation.Text = "Add Interpolation";
            this.miInterpolation.ToolTipText = "Add Interpolation Frames";
            this.miInterpolation.Click += new System.EventHandler(this.miInterpolation_Click);
            // 
            // miHelpInfo
            // 
            this.miHelpInfo.Name = "miHelpInfo";
            this.miHelpInfo.Size = new System.Drawing.Size(204, 22);
            this.miHelpInfo.Text = "Help Info...";
            this.miHelpInfo.Click += new System.EventHandler(this.btnInformation_Click);
            // 
            // miRunScript
            // 
            this.miRunScript.Name = "miRunScript";
            this.miRunScript.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.miRunScript.Size = new System.Drawing.Size(204, 22);
            this.miRunScript.Text = "Run Script";
            this.miRunScript.Click += new System.EventHandler(this.btnScript_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(201, 6);
            // 
            // miProperties
            // 
            this.miProperties.Name = "miProperties";
            this.miProperties.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.miProperties.Size = new System.Drawing.Size(204, 22);
            this.miProperties.Text = "Properties...";
            this.miProperties.Click += new System.EventHandler(this.btnConfigure_Click);
            // 
            // RecorderForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.ClientSize = new System.Drawing.Size(380, 79);
            this.ContextMenuStrip = this.ctxMenu;
            this.Controls.Add(this.clipTitle);
            this.Controls.Add(this.labelCurrentFrame);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressPanel);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnToEnd);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnPlayStop);
            this.Controls.Add(this.btnRecording);
            this.Controls.Add(this.labelMax);
            this.Controls.Add(this.btnClearAll);
            this.Controls.Add(this.btnReset);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "RecorderForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "RecorderForm";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.RecorderForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.RecorderForm_DragEnter);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RecorderForm_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RecorderForm_MouseUp);
            this.ctxMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnRecording;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnPlayStop;
        private System.Windows.Forms.Button btnToEnd;
        private System.Windows.Forms.Label labelMax;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Panel progressPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelCurrentFrame;
        private System.Windows.Forms.Label clipTitle;
        private System.Windows.Forms.ContextMenuStrip ctxMenu;
        private System.Windows.Forms.ToolStripMenuItem miHelpInfo;
        private System.Windows.Forms.ToolStripMenuItem miRunScript;
        private System.Windows.Forms.ToolStripMenuItem miProperties;
        private System.Windows.Forms.ToolStripMenuItem miCaptureFrame;
        private System.Windows.Forms.ToolStripMenuItem miAppendFrame;
        private System.Windows.Forms.ToolStripMenuItem miDeleteFrame;
        private System.Windows.Forms.ToolStripMenuItem miRefreshFrame;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem miInterpolation;
    }
}