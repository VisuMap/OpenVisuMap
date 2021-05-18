namespace VisuMap.DataModeling {
    partial class ModelTraining {
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
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tboxEpochs = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cboxModelName = new System.Windows.Forms.ComboBox();
            this.startStopButton1 = new VisuMap.Lib.StartStopButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnEditModel = new System.Windows.Forms.Button();
            this.btnShowGraph = new System.Windows.Forms.Button();
            this.cboxJobs = new System.Windows.Forms.ComboBox();
            this.cbLogLevel = new System.Windows.Forms.ComboBox();
            this.tbxLogFreq = new System.Windows.Forms.TextBox();
            this.btnConfig = new System.Windows.Forms.Button();
            this.labelS2 = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.cboxModelScript = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(6, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Model Name:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(23, 70);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Epochs:";
            // 
            // tboxEpochs
            // 
            this.tboxEpochs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.tboxEpochs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tboxEpochs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tboxEpochs.Location = new System.Drawing.Point(78, 70);
            this.tboxEpochs.Name = "tboxEpochs";
            this.tboxEpochs.ShortcutsEnabled = false;
            this.tboxEpochs.Size = new System.Drawing.Size(40, 13);
            this.tboxEpochs.TabIndex = 9;
            this.tboxEpochs.Text = "1000";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Model Script:";
            // 
            // cboxModelName
            // 
            this.cboxModelName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboxModelName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cboxModelName.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cboxModelName.FormattingEnabled = true;
            this.cboxModelName.Location = new System.Drawing.Point(78, 39);
            this.cboxModelName.Name = "cboxModelName";
            this.cboxModelName.Size = new System.Drawing.Size(262, 21);
            this.cboxModelName.TabIndex = 20;
            this.cboxModelName.SelectedIndexChanged += new System.EventHandler(this.cboxModelName_SelectedIndexChanged);
            // 
            // startStopButton1
            // 
            this.startStopButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.startStopButton1.IsRunning = false;
            this.startStopButton1.Location = new System.Drawing.Point(316, 115);
            this.startStopButton1.Name = "startStopButton1";
            this.startStopButton1.Size = new System.Drawing.Size(24, 24);
            this.startStopButton1.TabIndex = 21;
            this.startStopButton1.Stop += new System.EventHandler(this.StartStopButton1_Stop);
            this.startStopButton1.Start += new System.EventHandler(this.BtnStart_Click);
            // 
            // btnEditModel
            // 
            this.btnEditModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditModel.Image = global::DataModeling.Properties.Resources.Editing1;
            this.btnEditModel.Location = new System.Drawing.Point(316, 10);
            this.btnEditModel.Name = "btnEditModel";
            this.btnEditModel.Size = new System.Drawing.Size(24, 24);
            this.btnEditModel.TabIndex = 19;
            this.btnEditModel.Text = ".";
            this.toolTip1.SetToolTip(this.btnEditModel, "Editing the model script.");
            this.btnEditModel.UseVisualStyleBackColor = true;
            this.btnEditModel.Click += new System.EventHandler(this.BtnEditScript_Click);
            // 
            // btnShowGraph
            // 
            this.btnShowGraph.Image = global::DataModeling.Properties.Resources.EditScript;
            this.btnShowGraph.Location = new System.Drawing.Point(8, 116);
            this.btnShowGraph.Name = "btnShowGraph";
            this.btnShowGraph.Size = new System.Drawing.Size(28, 26);
            this.btnShowGraph.TabIndex = 27;
            this.btnShowGraph.Text = ".";
            this.toolTip1.SetToolTip(this.btnShowGraph, "Show the model graph.");
            this.btnShowGraph.UseVisualStyleBackColor = true;
            this.btnShowGraph.Click += new System.EventHandler(this.btnShowGraph_Click);
            // 
            // cboxJobs
            // 
            this.cboxJobs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cboxJobs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxJobs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboxJobs.FormattingEnabled = true;
            this.cboxJobs.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12"});
            this.cboxJobs.Location = new System.Drawing.Point(163, 66);
            this.cboxJobs.Name = "cboxJobs";
            this.cboxJobs.Size = new System.Drawing.Size(40, 21);
            this.cboxJobs.TabIndex = 29;
            this.toolTip1.SetToolTip(this.cboxJobs, "Number of parallel jobs.");
            this.cboxJobs.SelectedIndexChanged += new System.EventHandler(this.cboxJobs_SelectedIndexChanged);
            // 
            // cbLogLevel
            // 
            this.cbLogLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbLogLevel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbLogLevel.FormattingEnabled = true;
            this.cbLogLevel.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6"});
            this.cbLogLevel.Location = new System.Drawing.Point(306, 66);
            this.cbLogLevel.Name = "cbLogLevel";
            this.cbLogLevel.Size = new System.Drawing.Size(34, 21);
            this.cbLogLevel.TabIndex = 31;
            this.toolTip1.SetToolTip(this.cbLogLevel, "Log Level");
            this.cbLogLevel.SelectedIndexChanged += new System.EventHandler(this.cbLogLevel_SelectedIndexChanged);
            // 
            // tbxLogFreq
            // 
            this.tbxLogFreq.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxLogFreq.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.tbxLogFreq.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxLogFreq.Location = new System.Drawing.Point(269, 70);
            this.tbxLogFreq.Name = "tbxLogFreq";
            this.tbxLogFreq.ShortcutsEnabled = false;
            this.tbxLogFreq.Size = new System.Drawing.Size(26, 13);
            this.tbxLogFreq.TabIndex = 33;
            this.tbxLogFreq.Text = "10";
            this.toolTip1.SetToolTip(this.tbxLogFreq, "Log Frequence");
            this.tbxLogFreq.TextChanged += new System.EventHandler(this.tbxLogFreq_TextChanged);
            // 
            // btnConfig
            // 
            this.btnConfig.Image = global::DataModeling.Properties.Resources.CfgSettings;
            this.btnConfig.Location = new System.Drawing.Point(44, 116);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(28, 26);
            this.btnConfig.TabIndex = 19;
            this.btnConfig.Text = ".";
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.BtnConfig_Click);
            // 
            // labelS2
            // 
            this.labelS2.AutoSize = true;
            this.labelS2.Location = new System.Drawing.Point(36, 95);
            this.labelS2.Name = "labelS2";
            this.labelS2.Size = new System.Drawing.Size(40, 13);
            this.labelS2.TabIndex = 24;
            this.labelS2.Text = "Status:";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(76, 95);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(10, 13);
            this.labelStatus.TabIndex = 25;
            this.labelStatus.Text = "-";
            // 
            // cboxModelScript
            // 
            this.cboxModelScript.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboxModelScript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cboxModelScript.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxModelScript.FormattingEnabled = true;
            this.cboxModelScript.Location = new System.Drawing.Point(78, 12);
            this.cboxModelScript.Name = "cboxModelScript";
            this.cboxModelScript.Size = new System.Drawing.Size(234, 21);
            this.cboxModelScript.TabIndex = 26;
            this.cboxModelScript.SelectedIndexChanged += new System.EventHandler(this.cboxModelScript_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(129, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "Jobs:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(242, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(28, 13);
            this.label3.TabIndex = 32;
            this.label3.Text = "Log:";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(295, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(13, 13);
            this.label1.TabIndex = 34;
            this.label1.Text = "&&";
            // 
            // ModelTraining
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 148);
            this.Controls.Add(this.cboxJobs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnShowGraph);
            this.Controls.Add(this.cboxModelScript);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelS2);
            this.Controls.Add(this.startStopButton1);
            this.Controls.Add(this.cboxModelName);
            this.Controls.Add(this.btnEditModel);
            this.Controls.Add(this.btnConfig);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tboxEpochs);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbxLogFreq);
            this.Controls.Add(this.cbLogLevel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Name = "ModelTraining";
            this.Text = "Model Training";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tboxEpochs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnConfig;
        private System.Windows.Forms.ComboBox cboxModelName;
        private Lib.StartStopButton startStopButton1;
        private System.Windows.Forms.Button btnEditModel;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label labelS2;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ComboBox cboxModelScript;
        private System.Windows.Forms.Button btnShowGraph;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboxJobs;
        private System.Windows.Forms.ComboBox cbLogLevel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbxLogFreq;
        private System.Windows.Forms.Label label1;
    }
}