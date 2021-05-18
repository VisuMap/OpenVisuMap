namespace VisuMap.DataModeling {
    partial class ModelServer {
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
            this.tboxArgs = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.cboxScript = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cboxModelNames = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnShowGraph = new System.Windows.Forms.Button();
            this.btnStopServer = new System.Windows.Forms.Button();
            this.btnStartServer = new System.Windows.Forms.Button();
            this.btnConfig = new System.Windows.Forms.Button();
            this.btnWeights = new System.Windows.Forms.Button();
            this.btnShowActivity = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // tboxArgs
            // 
            this.tboxArgs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tboxArgs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.tboxArgs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tboxArgs.Location = new System.Drawing.Point(93, 96);
            this.tboxArgs.Name = "tboxArgs";
            this.tboxArgs.Size = new System.Drawing.Size(239, 13);
            this.tboxArgs.TabIndex = 53;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 96);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(84, 13);
            this.label9.TabIndex = 52;
            this.label9.Text = "Arguments (opt):";
            // 
            // btnStart
            // 
            this.btnStart.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnStart.Location = new System.Drawing.Point(277, 139);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(55, 37);
            this.btnStart.TabIndex = 32;
            this.btnStart.Text = "Run Script";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // cboxScript
            // 
            this.cboxScript.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboxScript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cboxScript.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxScript.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cboxScript.FormattingEnabled = true;
            this.cboxScript.Location = new System.Drawing.Point(93, 63);
            this.cboxScript.Name = "cboxScript";
            this.cboxScript.Size = new System.Drawing.Size(203, 21);
            this.cboxScript.TabIndex = 50;
            this.cboxScript.SelectedIndexChanged += new System.EventHandler(this.cboxScript_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(26, 69);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(66, 13);
            this.label8.TabIndex = 49;
            this.label8.Text = "Client Script:";
            // 
            // cboxModelNames
            // 
            this.cboxModelNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboxModelNames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cboxModelNames.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cboxModelNames.FormattingEnabled = true;
            this.cboxModelNames.Location = new System.Drawing.Point(93, 12);
            this.cboxModelNames.Name = "cboxModelNames";
            this.cboxModelNames.Size = new System.Drawing.Size(239, 21);
            this.cboxModelNames.TabIndex = 34;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(11, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 33;
            this.label2.Text = "Model Name:";
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEdit.Image = global::DataModeling.Properties.Resources.Editing1;
            this.btnEdit.Location = new System.Drawing.Point(302, 59);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(30, 28);
            this.btnEdit.TabIndex = 51;
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnShowGraph
            // 
            this.btnShowGraph.Image = global::DataModeling.Properties.Resources.EditScript;
            this.btnShowGraph.Location = new System.Drawing.Point(94, 150);
            this.btnShowGraph.Name = "btnShowGraph";
            this.btnShowGraph.Size = new System.Drawing.Size(28, 26);
            this.btnShowGraph.TabIndex = 48;
            this.btnShowGraph.Text = ".";
            this.btnShowGraph.UseVisualStyleBackColor = true;
            this.btnShowGraph.Click += new System.EventHandler(this.btnShowGraph_Click);
            // 
            // btnStopServer
            // 
            this.btnStopServer.Image = global::DataModeling.Properties.Resources.ShutdownServer;
            this.btnStopServer.Location = new System.Drawing.Point(184, 150);
            this.btnStopServer.Name = "btnStopServer";
            this.btnStopServer.Size = new System.Drawing.Size(28, 26);
            this.btnStopServer.TabIndex = 47;
            this.btnStopServer.Text = ".";
            this.toolTip1.SetToolTip(this.btnStopServer, "Shutdown the server.");
            this.btnStopServer.UseVisualStyleBackColor = true;
            this.btnStopServer.Click += new System.EventHandler(this.btnStopServer_Click);
            // 
            // btnStartServer
            // 
            this.btnStartServer.Image = global::DataModeling.Properties.Resources.StartServer;
            this.btnStartServer.Location = new System.Drawing.Point(64, 150);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(28, 26);
            this.btnStartServer.TabIndex = 47;
            this.btnStartServer.Text = ".";
            this.toolTip1.SetToolTip(this.btnStartServer, "Start the server with given model");
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
            // 
            // btnConfig
            // 
            this.btnConfig.Image = global::DataModeling.Properties.Resources.CfgSettings;
            this.btnConfig.Location = new System.Drawing.Point(14, 150);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(28, 26);
            this.btnConfig.TabIndex = 47;
            this.btnConfig.Text = ".";
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.btnConfig_Click);
            // 
            // btnWeights
            // 
            this.btnWeights.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnWeights.Location = new System.Drawing.Point(124, 150);
            this.btnWeights.Name = "btnWeights";
            this.btnWeights.Size = new System.Drawing.Size(28, 26);
            this.btnWeights.TabIndex = 54;
            this.btnWeights.Text = "W";
            this.toolTip1.SetToolTip(this.btnWeights, "Show weights and biases.");
            this.btnWeights.UseVisualStyleBackColor = true;
            this.btnWeights.Click += new System.EventHandler(this.btnWeights_Click);
            // 
            // btnShowActivity
            // 
            this.btnShowActivity.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnShowActivity.Location = new System.Drawing.Point(154, 150);
            this.btnShowActivity.Name = "btnShowActivity";
            this.btnShowActivity.Size = new System.Drawing.Size(28, 26);
            this.btnShowActivity.TabIndex = 54;
            this.btnShowActivity.Text = "A";
            this.toolTip1.SetToolTip(this.btnShowActivity, "Evaluate activity of node for selected input data.");
            this.btnShowActivity.UseVisualStyleBackColor = true;
            this.btnShowActivity.Click += new System.EventHandler(this.btnShowActivity_Click);
            // 
            // ModelServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 180);
            this.Controls.Add(this.tboxArgs);
            this.Controls.Add(this.btnShowActivity);
            this.Controls.Add(this.btnWeights);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.cboxScript);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnShowGraph);
            this.Controls.Add(this.btnStopServer);
            this.Controls.Add(this.btnStartServer);
            this.Controls.Add(this.btnConfig);
            this.Controls.Add(this.cboxModelNames);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ModelServer";
            this.Text = "Model Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tboxArgs;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.ComboBox cboxScript;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnShowGraph;
        private System.Windows.Forms.Button btnConfig;
        private System.Windows.Forms.ComboBox cboxModelNames;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Button btnStopServer;
        private System.Windows.Forms.Button btnWeights;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnShowActivity;
    }
}