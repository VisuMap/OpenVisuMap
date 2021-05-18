using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VisuMap.DataCleansing {
    public partial class RepalceForm : Form {
        public RepalceForm() {
            InitializeComponent();
            DialogResult = DialogResult.Cancel;
        }

        public string FindText {
            get { return tboxFindText.Text; }
        }

        public string ReplaceText {
            get { return tboxReplaceText.Text; }
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void btnReplaceAll_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
        }
    }
}