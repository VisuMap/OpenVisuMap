namespace VisuMap.DataCleansing {
    partial class RepalceForm {
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
            this.miFindText = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tboxFindText = new System.Windows.Forms.TextBox();
            this.tboxReplaceText = new System.Windows.Forms.TextBox();
            this.btnReplaceAll = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // miFindText
            // 
            this.miFindText.AutoSize = true;
            this.miFindText.Location = new System.Drawing.Point(41, 9);
            this.miFindText.Name = "miFindText";
            this.miFindText.Size = new System.Drawing.Size(54, 13);
            this.miFindText.TabIndex = 0;
            this.miFindText.Text = "Find Text:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Replace with:";
            // 
            // tboxFindText
            // 
            this.tboxFindText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tboxFindText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tboxFindText.Location = new System.Drawing.Point(101, 9);
            this.tboxFindText.Name = "tboxFindText";
            this.tboxFindText.Size = new System.Drawing.Size(232, 13);
            this.tboxFindText.TabIndex = 2;
            // 
            // tboxReplaceText
            // 
            this.tboxReplaceText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tboxReplaceText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tboxReplaceText.Location = new System.Drawing.Point(101, 38);
            this.tboxReplaceText.Name = "tboxReplaceText";
            this.tboxReplaceText.Size = new System.Drawing.Size(232, 13);
            this.tboxReplaceText.TabIndex = 3;
            // 
            // btnReplaceAll
            // 
            this.btnReplaceAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReplaceAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnReplaceAll.Location = new System.Drawing.Point(252, 62);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(81, 34);
            this.btnReplaceAll.TabIndex = 4;
            this.btnReplaceAll.Text = "Replace &All";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new System.EventHandler(this.btnReplaceAll_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(101, 62);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(61, 34);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // RepalceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(352, 103);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnReplaceAll);
            this.Controls.Add(this.tboxReplaceText);
            this.Controls.Add(this.tboxFindText);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.miFindText);
            this.Name = "RepalceForm";
            this.RightToLeftLayout = true;
            this.Text = "Repalce Text";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label miFindText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tboxFindText;
        private System.Windows.Forms.TextBox tboxReplaceText;
        private System.Windows.Forms.Button btnReplaceAll;
        private System.Windows.Forms.Button btnCancel;
    }
}