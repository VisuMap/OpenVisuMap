using System;
using VisuMap.Plugin;
using VisuMap.Script;
using System.Windows.Forms;

namespace CustomImporter {
    [PluginMain]
    public class CustomImporter : IPlugin {
        public static IApplication App;
        public virtual void Initialize(IApplication app) {
            if (app.ScriptApp.ApplicationBuild < 884) {
                MessageBox.Show("Cannot initialize custom importer plugin: VisuMap 3.0.884 or higher required",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            App = app;
            ToolStripMenuItem miPlugin = app.GetPluginMenu();
            miPlugin.DropDownItems.Add(new ToolStripMenuItem("Import Script", null, OpenImportScript));

            App.InstallFileImporter(new WekaArff());
            App.InstallFileImporter(new SdfFile());
            App.InstallFileImporter(new FcsCytometry());
            App.InstallFileImporter(new AffyMetrixCel());
            App.InstallFileImporter(new MatrixMarket());

            if (!string.IsNullOrEmpty(app.ScriptApp.GetProperty("CustomImporter.CsvFilter.Extension", ""))) {
                App.InstallFileImporter(new CsvFilter());
            }
        }

        public virtual void Dispose() { }
        public virtual string Name { get { return "CustomImporter"; } }

        public static void OpenImportScript(object sender, EventArgs e) {
            App.ScriptApp.New.ScriptEditor(App.ScriptApp.ApplicationData + "\\plugins\\Custom Importer\\ImportScript.js").Show();
        }

    }
}
