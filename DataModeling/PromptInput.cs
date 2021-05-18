using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VisuMap.DataModeling {
    public partial class PromptInput : Form {
        private PromptInput(string title, string msg, string defaultValue)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

            this.Text = title;
            msgLabel.Text = msg;
            if ( defaultValue != null ) {
                inputTextBox.Text = defaultValue;
            }
        }

        public static string PromptString(string title, string msg, string defaultValue, int winWidth) {
            PromptInput prompt = new PromptInput(title, msg, defaultValue);
            if (winWidth > 0) {
                prompt.Width = winWidth;
            }
            string str = null;
            if (prompt.ShowDialog() == DialogResult.OK) {
                str = prompt.inputTextBox.Text;
            }
            prompt.Dispose();
            return str;
        }

        private void btnOK_Click(object sender, System.EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
