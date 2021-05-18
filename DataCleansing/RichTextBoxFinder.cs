using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace VisuMap.DataCleansing {
    /// <summary>
    /// Summary description for RichTextBoxFinder.
    /// </summary>
    public class RichTextBoxFinder : System.Windows.Forms.Form {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button findButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox matchCaseCB;
        private System.Windows.Forms.RadioButton downRadioButton;
        private System.Windows.Forms.RadioButton upRadioButon;
        private string lastText = "";
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private RichTextBox richTextBox;

        public RichTextBoxFinder(RichTextBox richTextBox) {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.richTextBox = richTextBox;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing ) {
            if( disposing ) {
                if(components != null) {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        protected override void OnClosing(CancelEventArgs e) {
            lastText = textBox.Text;
            base.OnClosing (e);
        }


        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.label1 = new System.Windows.Forms.Label();
            this.textBox = new System.Windows.Forms.TextBox();
            this.findButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.matchCaseCB = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.downRadioButton = new System.Windows.Forms.RadioButton();
            this.upRadioButon = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Fi&nd what:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // textBox
            // 
            this.textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.textBox.Location = new System.Drawing.Point(80, 12);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(240, 22);
            this.textBox.TabIndex = 1;
            this.textBox.Text = "";
            this.textBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyUp);
            // 
            // findButton
            // 
            this.findButton.Location = new System.Drawing.Point(328, 13);
            this.findButton.Name = "findButton";
            this.findButton.TabIndex = 2;
            this.findButton.Text = "&Find Next";
            this.findButton.Click += new System.EventHandler(this.findButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(328, 56);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(72, 24);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // matchCaseCB
            // 
            this.matchCaseCB.Location = new System.Drawing.Point(16, 60);
            this.matchCaseCB.Name = "matchCaseCB";
            this.matchCaseCB.Size = new System.Drawing.Size(96, 16);
            this.matchCaseCB.TabIndex = 4;
            this.matchCaseCB.Text = "Match &Case";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.downRadioButton);
            this.groupBox1.Controls.Add(this.upRadioButon);
            this.groupBox1.Location = new System.Drawing.Point(192, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(128, 40);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Direction";
            // 
            // downRadioButton
            // 
            this.downRadioButton.Checked = true;
            this.downRadioButton.Location = new System.Drawing.Point(64, 16);
            this.downRadioButton.Name = "downRadioButton";
            this.downRadioButton.Size = new System.Drawing.Size(56, 16);
            this.downRadioButton.TabIndex = 1;
            this.downRadioButton.TabStop = true;
            this.downRadioButton.Text = "&Down";
            // 
            // upRadioButon
            // 
            this.upRadioButon.Location = new System.Drawing.Point(8, 16);
            this.upRadioButon.Name = "upRadioButon";
            this.upRadioButon.Size = new System.Drawing.Size(40, 16);
            this.upRadioButon.TabIndex = 0;
            this.upRadioButon.Text = "&Up";
            // 
            // FindTextPrompt
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(410, 88);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.matchCaseCB);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.findButton);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindTextPrompt";
            this.Text = "Find";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void label1_Click(object sender, System.EventArgs e) {
            this.textBox.Focus();
        }

        public void FindNext() {
            findButton_Click(null, null);
        }

        private void findButton_Click(object sender, EventArgs e) {
            string  word = textBox.Text;
            if ( this.IsDisposed ) {
                word = lastText;
            }

            if ( string.IsNullOrEmpty(word) ) {
                return;
            }

            int     startIndex = richTextBox.SelectionStart;
            string  script = richTextBox.Text;
            if ( downRadioButton.Checked ) {
                startIndex += richTextBox.SelectionLength;
            }

            if ( (startIndex < 0) || (startIndex>=script.Length) ) {
                MessageBox.Show("Cannot find \"" + textBox.Text + "\"");
                return;
            }

            if (! matchCaseCB.Checked ) {
                // case insenstive find.
                word = word.ToLower();
                script = script.ToLower();
            }

            int wordPos = -1;
            if ( downRadioButton.Checked ) {
                wordPos = script.IndexOf(word, startIndex);
            } else {
                wordPos = script.LastIndexOf(word, startIndex, startIndex+1);
            }

            if ( wordPos >= 0 ) {
                richTextBox.SelectionStart = wordPos;
                richTextBox.SelectionLength = word.Length;
            } else {
                MessageBox.Show("Cannot find \"" + textBox.Text + "\"");
            }
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            Close();
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e) {
            if ( e.KeyCode == Keys.Return ) {
                findButton_Click(null, null);
            }
        }
    }
}