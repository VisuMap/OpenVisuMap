namespace DataGenerator {
    partial class SingleCell {
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tboxRows = new System.Windows.Forms.TextBox();
            this.tboxColumns = new System.Windows.Forms.TextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Rows:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(166, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Columns:";
            // 
            // tboxRows
            // 
            this.tboxRows.Location = new System.Drawing.Point(57, 13);
            this.tboxRows.Name = "tboxRows";
            this.tboxRows.Size = new System.Drawing.Size(59, 20);
            this.tboxRows.TabIndex = 1;
            this.tboxRows.Text = "1000";
            // 
            // tboxColumns
            // 
            this.tboxColumns.Location = new System.Drawing.Point(222, 13);
            this.tboxColumns.Name = "tboxColumns";
            this.tboxColumns.Size = new System.Drawing.Size(59, 20);
            this.tboxColumns.TabIndex = 1;
            this.tboxColumns.Text = "2000";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(222, 79);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 2;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // SingleCell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(313, 133);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.tboxColumns);
            this.Controls.Add(this.tboxRows);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "SingleCell";
            this.Text = "Single Cell Simulation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tboxRows;
        private System.Windows.Forms.TextBox tboxColumns;
        private System.Windows.Forms.Button btnGenerate;
    }
}