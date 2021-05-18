namespace VisuMap.DataModeling {
    partial class ModelTest {
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
            this.btnStart = new System.Windows.Forms.Button();
            this.cboxModelNames = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblDatasetName = new System.Windows.Forms.Label();
            this.lblVariables = new System.Windows.Forms.Label();
            this.lblLearningTarget = new System.Windows.Forms.Label();
            this.lblUpdateTime = new System.Windows.Forms.Label();
            this.lblLearningEpochs = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblMapSize = new System.Windows.Forms.Label();
            this.cboxScript = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnConfig = new System.Windows.Forms.Button();
            this.btnShowGraph = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.tboxArgs = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(356, 190);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(55, 37);
            this.btnStart.TabIndex = 7;
            this.btnStart.Text = "Apply Model";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // cboxModelNames
            // 
            this.cboxModelNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboxModelNames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cboxModelNames.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cboxModelNames.FormattingEnabled = true;
            this.cboxModelNames.Location = new System.Drawing.Point(108, 36);
            this.cboxModelNames.Name = "cboxModelNames";
            this.cboxModelNames.Size = new System.Drawing.Size(303, 21);
            this.cboxModelNames.TabIndex = 9;
            this.cboxModelNames.SelectedValueChanged += new System.EventHandler(this.CboxModelNames_SelectedValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(57, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Model:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Training Dataset:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Learning Target:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Training Variables:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(34, 161);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Last Update:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 179);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(87, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Training Epochs:";
            // 
            // lblDatasetName
            // 
            this.lblDatasetName.AutoSize = true;
            this.lblDatasetName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblDatasetName.Location = new System.Drawing.Point(104, 89);
            this.lblDatasetName.Name = "lblDatasetName";
            this.lblDatasetName.Size = new System.Drawing.Size(10, 13);
            this.lblDatasetName.TabIndex = 12;
            this.lblDatasetName.Text = "-";
            // 
            // lblVariables
            // 
            this.lblVariables.AutoSize = true;
            this.lblVariables.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblVariables.Location = new System.Drawing.Point(104, 107);
            this.lblVariables.Name = "lblVariables";
            this.lblVariables.Size = new System.Drawing.Size(10, 13);
            this.lblVariables.TabIndex = 12;
            this.lblVariables.Text = "-";
            // 
            // lblLearningTarget
            // 
            this.lblLearningTarget.AutoSize = true;
            this.lblLearningTarget.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblLearningTarget.Location = new System.Drawing.Point(104, 125);
            this.lblLearningTarget.Name = "lblLearningTarget";
            this.lblLearningTarget.Size = new System.Drawing.Size(10, 13);
            this.lblLearningTarget.TabIndex = 12;
            this.lblLearningTarget.Text = "-";
            // 
            // lblUpdateTime
            // 
            this.lblUpdateTime.AutoSize = true;
            this.lblUpdateTime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblUpdateTime.Location = new System.Drawing.Point(104, 161);
            this.lblUpdateTime.Name = "lblUpdateTime";
            this.lblUpdateTime.Size = new System.Drawing.Size(10, 13);
            this.lblUpdateTime.TabIndex = 12;
            this.lblUpdateTime.Text = "-";
            // 
            // lblLearningEpochs
            // 
            this.lblLearningEpochs.AutoSize = true;
            this.lblLearningEpochs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblLearningEpochs.Location = new System.Drawing.Point(104, 179);
            this.lblLearningEpochs.Name = "lblLearningEpochs";
            this.lblLearningEpochs.Size = new System.Drawing.Size(10, 13);
            this.lblLearningEpochs.TabIndex = 12;
            this.lblLearningEpochs.Text = "-";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 143);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Target Map Size:";
            // 
            // lblMapSize
            // 
            this.lblMapSize.AutoSize = true;
            this.lblMapSize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblMapSize.Location = new System.Drawing.Point(104, 143);
            this.lblMapSize.Name = "lblMapSize";
            this.lblMapSize.Size = new System.Drawing.Size(10, 13);
            this.lblMapSize.TabIndex = 14;
            this.lblMapSize.Text = "-";
            // 
            // cboxScript
            // 
            this.cboxScript.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboxScript.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cboxScript.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxScript.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cboxScript.FormattingEnabled = true;
            this.cboxScript.Location = new System.Drawing.Point(108, 9);
            this.cboxScript.Name = "cboxScript";
            this.cboxScript.Size = new System.Drawing.Size(271, 21);
            this.cboxScript.TabIndex = 28;
            this.cboxScript.SelectedIndexChanged += new System.EventHandler(this.cboxScript_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 12);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(90, 13);
            this.label8.TabIndex = 27;
            this.label8.Text = "Evaluation Script:";
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEdit.Image = global::DataModeling.Properties.Resources.Editing1;
            this.btnEdit.Location = new System.Drawing.Point(385, 4);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(30, 28);
            this.btnEdit.TabIndex = 29;
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnConfig
            // 
            this.btnConfig.Image = global::DataModeling.Properties.Resources.CfgSettings;
            this.btnConfig.Location = new System.Drawing.Point(45, 199);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(28, 26);
            this.btnConfig.TabIndex = 20;
            this.btnConfig.Text = ".";
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.btnConfig_Click);
            // 
            // btnShowGraph
            // 
            this.btnShowGraph.Image = global::DataModeling.Properties.Resources.EditScript;
            this.btnShowGraph.Location = new System.Drawing.Point(11, 199);
            this.btnShowGraph.Name = "btnShowGraph";
            this.btnShowGraph.Size = new System.Drawing.Size(28, 26);
            this.btnShowGraph.TabIndex = 20;
            this.btnShowGraph.Text = ".";
            this.btnShowGraph.UseVisualStyleBackColor = true;
            this.btnShowGraph.Click += new System.EventHandler(this.btnShowGraph_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(18, 66);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(84, 13);
            this.label9.TabIndex = 30;
            this.label9.Text = "Arguments (opt):";
            // 
            // tboxArgs
            // 
            this.tboxArgs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tboxArgs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.tboxArgs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tboxArgs.Location = new System.Drawing.Point(108, 66);
            this.tboxArgs.Name = "tboxArgs";
            this.tboxArgs.Size = new System.Drawing.Size(303, 13);
            this.tboxArgs.TabIndex = 31;
            // 
            // ModelTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 232);
            this.Controls.Add(this.tboxArgs);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.cboxScript);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnShowGraph);
            this.Controls.Add(this.btnConfig);
            this.Controls.Add(this.lblMapSize);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblLearningEpochs);
            this.Controls.Add(this.lblUpdateTime);
            this.Controls.Add(this.lblLearningTarget);
            this.Controls.Add(this.lblVariables);
            this.Controls.Add(this.lblDatasetName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboxModelNames);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ModelTest";
            this.Text = "Model Evaluation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ComboBox cboxModelNames;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblDatasetName;
        private System.Windows.Forms.Label lblVariables;
        private System.Windows.Forms.Label lblLearningTarget;
        private System.Windows.Forms.Label lblUpdateTime;
        private System.Windows.Forms.Label lblLearningEpochs;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblMapSize;
        private System.Windows.Forms.Button btnConfig;
        private System.Windows.Forms.ComboBox cboxScript;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnShowGraph;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tboxArgs;
    }
}