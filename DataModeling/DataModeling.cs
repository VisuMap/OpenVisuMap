using System;
using System.Windows.Forms;
using System.Reflection;

using VisuMap.Plugin;
using VisuMap.Lib;
using XSavedAttribute = VisuMap.Lib.SavedAttribute;

namespace VisuMap.DataModeling {
    [PluginMain]
    public class DataModeling : IPlugin {
        public static IApplication App;
        public static ModelManager modelManager;
        public static CmdServer cmdServer;
        public static ModelingScript mdScript;
        public static IScriptPlugin scriptEngine;
        public static string workDir = "";
        public static string homeDir = "";
        public static int monitorPort = 8888;
        public static string pythonProgamm = "python";        

        VisuMap.Lib.PropertyManager propMan;
        System.Xml.XmlElement pluginRoot;
        string openForms="";

        public virtual void Initialize(IApplication app) {
            App = app;

            if (app.ScriptApp.ApplicationBuild < 935) {
                MessageBox.Show("Data Modeling plugin requires VisuMap 5.0.935 or higher!",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            app.InstallPluginObject(mdScript = new ModelingScript());

            ToolStripMenuItem miPlugin = App.GetPluginMenu();
            var gm = app.ScriptApp.GuiManager;
            miPlugin.DropDownItems.Add("Model Training", null, (s,e) => gm.ShowForm(new ModelTraining(), true));
            miPlugin.DropDownItems.Add("Model Evaluation", null, (s, e) => gm.ShowForm(new ModelTest(), true));
            miPlugin.DropDownItems.Add("Model Server", null, (s, e) => gm.ShowForm(new ModelServer(), true));
            miPlugin.DropDownItems.Add("Model Manager", null, (s, e) => gm.ShowForm(new ModelManager2(), true));

            app.InstallScriptPlugin(new PythonEngine());
            //app.InstallScriptPlugin(new J8Engine());

            propMan = new VisuMap.Lib.PropertyManager(this, "Settings", "DataModeling");
            pluginRoot = DataModeling.App.GetPluginDataNode(0, "DataModeling", propMan.NameSpace, true);
            propMan.LoadProperties(pluginRoot);
            homeDir = app.ScriptApp.GetProperty("DataModeling.HomeDir", "");
            workDir = app.ScriptApp.GetProperty("DataModeling.WorkDir", "");
            if (!homeDir.EndsWith("\\")) homeDir += "\\";
            if (!workDir.EndsWith("\\")) workDir += "\\";

            SetDefaultWorkDir();
            modelManager = new ModelManager();

            app.InstallFileImporter(new NumpyFileImport());

            cmdServer = new CmdServer();
            cmdServer.Start();
            app.ShuttingDown += App_ShuttingDown;
            app.ApplicationStarted += App_ApplicationStarted;
        }

        private void App_ApplicationStarted(object sender, EventArgs e) {
            var gm = App.ScriptApp.GuiManager;
            if (openForms.IndexOf("A") >= 0) gm.ShowForm(new ModelTraining(), true);
            if (openForms.IndexOf("B") >= 0) gm.ShowForm(new ModelTest(), true);
            if (openForms.IndexOf("C") >= 0) gm.ShowForm(new ModelServer(), true);
        }

        private void App_ShuttingDown(object sender, EventArgs e) {
            cmdServer.Dispose();

            openForms = "";
            if ( (DataModeling.mdScript.CurrentTrainer != null) && (!DataModeling.mdScript.CurrentTrainer.IsDisposed) )
                openForms += "A";
            if ((DataModeling.mdScript.CurrentTester != null) && (!DataModeling.mdScript.CurrentTester.IsDisposed))
                openForms += "B";
            if ((DataModeling.mdScript.CurrentServer != null) && (!DataModeling.mdScript.CurrentServer.IsDisposed))
                openForms += "C";

            propMan.SaveProperties(pluginRoot);
        }

        [XSaved("OpFrms")]
        public string OpenForms
        {
            get => openForms;
            set => openForms = value;
        }


        void SetDefaultWorkDir() {            
            if (string.IsNullOrEmpty(workDir)) {
                string wDir = Assembly.GetExecutingAssembly().Location;
                if (wDir.IndexOf("\\bin\\") < 0) {
                    // Installed plugin.
                    wDir = wDir.Substring(0, wDir.LastIndexOf('\\') + 1);
                } else {
                    wDir = wDir.Substring(0, wDir.LastIndexOf('\\'));
                    wDir = wDir.Substring(0, wDir.LastIndexOf('\\'));
                    wDir = wDir.Substring(0, wDir.LastIndexOf('\\') + 1);
                }
                workDir = wDir;                
            }            
        }

        public virtual void Dispose() {
        }

        public string Name {
            get { return "DataModeling"; }
            set { }
        }
    }
}
