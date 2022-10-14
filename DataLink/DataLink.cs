using System;
using System.Reflection;
using System.Windows.Forms;

using VisuMap.Plugin;
using XSavedAttribute = VisuMap.Lib.SavedAttribute;

namespace VisuMap.DataLink {
    [PluginMain]
    public class DataLink : IPlugin {
        public static IApplication App;
        public static CmdServer cmdServer;
        public static IScriptPlugin scriptEngine;

        public virtual void Initialize(IApplication app) {
            App = app;

            if (app.ScriptApp.ApplicationBuild < 935) {
                MessageBox.Show("Data Modeling plugin requires VisuMap 5.0.935 or higher!",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            ToolStripMenuItem miPlugin = App.GetPluginMenu();
            var gm = app.ScriptApp.GuiManager;

            app.InstallScriptPlugin(new PythonEngine());
            app.InstallFileImporter(new NumpyFileImport());
            cmdServer = new CmdServer();
            cmdServer.Start();
            app.ShuttingDown += App_ShuttingDown;
            app.ApplicationStarted += App_ApplicationStarted;
        }

        private void App_ApplicationStarted(object sender, EventArgs e) {
        }

        private void App_ShuttingDown(object sender, EventArgs e) {
            cmdServer.Dispose();
        }

        public virtual void Dispose() {
        }

        public string Name {
            get { return "Data Link"; }
            set { }
        }
    }
}
