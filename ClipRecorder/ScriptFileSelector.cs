using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ClipRecorder {
    public class ScriptFileSelector : FileNameEditor {
        protected override void InitializeDialog(OpenFileDialog openFileDialog) {
            base.InitializeDialog(openFileDialog);
            openFileDialog.Filter = "JavaScript Files (*.js)|*.js|All files(*.*)|*.*";
            openFileDialog.Title = "Select JavaScript File";
        }
    }
}
