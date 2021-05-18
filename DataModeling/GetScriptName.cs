using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VisuMap.DataModeling {
    public partial class GetScriptName : Form {
        string newscriptName = "";
        string scriptExtension = "";

        public GetScriptName(ComboBox parent, string currentScript, string extension) {
            this.scriptExtension = extension;
            InitializeComponent();
            this.Text = "New Script Settings";
            this.DialogResult = DialogResult.Cancel;

            foreach (var item in parent.Items) {
                string nm = item.ToString();
                if ( nm != ModelTraining.newModelName)
                    lbInitialScript.Items.Add(nm);
            }

            if (DataModeling.homeDir != DataModeling.workDir) {
                foreach (var f in System.IO.Directory.EnumerateFiles(DataModeling.homeDir)) {
                    if (f.EndsWith(extension))
                        lbInitialScript.Items.Add( "~" + Path.GetFileName(f));
                }
            }
            int idx = lbInitialScript.FindString(currentScript);
            lbInitialScript.SetSelected(Math.Max(0, idx), true);
        }

        string ScriptExtention
        {
            get => scriptExtension;
        }

        public string NewScriptName { get => newscriptName; }

        public string InitScriptName{
            get
            {
                string sName = lbInitialScript.SelectedItem.ToString();
                return (sName[0] == '~') ?
                    (DataModeling.homeDir + sName.Substring(1)) : (DataModeling.workDir + "\\" + sName);
            }
        }

        private void btnSave_Click(object sender, EventArgs e) {
            if (lbInitialScript.SelectedIndex < 0) lbInitialScript.SelectedIndex = 0;
            string mdName = this.textBox1.Text;
            if ( string.IsNullOrEmpty(mdName) ) {
                MessageBox.Show("Please enter a name (without extension) for the new script!");
                return;
            }
            if (!mdName.EndsWith(ScriptExtention))
                mdName += ScriptExtention;

            if (  File.Exists(DataModeling.workDir + mdName) ) {
                var ret = MessageBox.Show("The script " + mdName + " already exists.\nDo you want to overwrite it?", "Overwritting Exsting Script?", MessageBoxButtons.YesNo);
                if ( ret == DialogResult.No) 
                    return;
            }
            newscriptName = mdName;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
