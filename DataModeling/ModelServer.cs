using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Threading;

using VisuMap.Script;
using VisuMap.Lib;
using XSavedAttribute = VisuMap.Lib.SavedAttribute;
using XConfigurableAttribute = VisuMap.Lib.ConfigurableAttribute;

namespace VisuMap.DataModeling {
    public partial class ModelServer : Form {
        VisuMap.Lib.PropertyManager propMan;
        XmlElement pluginRoot;
        LiveModel md;
        string scriptEditor = "notepad";
        public const int SERVER_PORT = 7777;
        public const string SERVER_NAME = "localhost";
        int serverPort = SERVER_PORT;
        string serverName = SERVER_NAME;
        bool startWithTracing = false;
        string commandLine = "";
        bool minimizeServerWindow = false;

        public ModelServer() {
            InitializeComponent();
            Directory.SetCurrentDirectory(DataModeling.workDir);
            RefreshList();
            propMan = new VisuMap.Lib.PropertyManager(this, "Settings", "DataModeling");
            pluginRoot = DataModeling.App.GetPluginDataNode(0, "DataModeling", propMan.NameSpace, true);            
            propMan.LoadProperties(pluginRoot);
            DataModeling.mdScript.CurrentServer = this;
        }

        void RefreshList() {
            cboxScript.Items.Clear();
            cboxModelNames.Items.Clear();
            List<string> pyList = new List<string>();

            foreach (var f in Directory.EnumerateFiles(DataModeling.workDir)) {
                string fnm = f.ToLower();
                FileInfo info = new FileInfo(f);
                if (fnm.EndsWith(".client.js")) {
                    cboxScript.Items.Add(info.Name);
                } else if (fnm.EndsWith(".client.py")) {
                    pyList.Add(info.Name);
                } else if (fnm.EndsWith(".md")) {
                    cboxModelNames.Items.Add(info.Name);
                }
            }

            if (pyList.Count > 0)
                cboxScript.Items.AddRange(pyList.ToArray());

            cboxScript.Items.Add(ModelTraining.newModelName);

            int idx = cboxModelNames.FindString(ModelFileName);
            if ( idx >=0 ) {
                cboxModelNames.SelectedIndex = idx;
            }
        }

        public void SelectModel(string modelName)
        {
            int idx = cboxModelNames.FindString(modelName);
            if (idx >= 0)
                cboxModelNames.SelectedIndex = idx;
        }

        public void SetScript(string scriptName) {
            int idx = cboxScript.FindString(scriptName);
            if (idx >= 0)
                cboxScript.SelectedIndex = idx;
        }


        public void StartServer() {
            this.btnStartServer.PerformClick();
        }

        private void btnEdit_Click(object sender, EventArgs e) {
            string scriptFile = cboxScript.Text;
            if (!string.IsNullOrEmpty(scriptFile)) {
                if (scriptFile.EndsWith("client.js")) {
                    var ed = DataModeling.App.ScriptApp.New.ScriptEditor(DataModeling.workDir + scriptFile);
                    ed.SetParentForm(this, Argument);
                    ed.Show();
                } else { // assuming the script is py script.
                    System.Diagnostics.Process.Start(scriptEditor, "\"" + scriptFile + "\"");
                }
            } else {
                MessageBox.Show("Client script not specified!");
            }
        }

        private void btnStartServer_Click(object sender, EventArgs e) {
            if ( md != null ) {
                if (md.PortNumber == this.ServerPort) {
                    if (md.ModelName != this.ModelName) {
                        md.ShutdownModel();
                        md.Dispose();
                        md = null;
                    } else {
                        return;
                    }
                } else {
                    // we are going to start the model on a different port number.
                    md = null;
                }  
            }

            if ( md == null) {
                md = new LiveModel(this.ModelName, serverPort);
            }
            md.StartModel(true);
            commandLine = md.CommandLine;
            if (minimizeServerWindow)
                md.MinimizeServerWindow();

            if (! md.IsConnected ) {
                md.Connect();
            }
            if (startWithTracing)
                md.SetTracing(true);
        }

        void ConnectToServer() {
            if (md == null) {
                md = new LiveModel(this.ModelName, serverPort, serverName);
            }
            if (!md.IsConnected) {
                md.Connect();
            }
        }

        private void btnStopServer_Click(object sender, EventArgs e) {
            if( (md != null) && md.IsConnected) {
                md.ShutdownModel();
            } else {
                if (md != null)
                    md.Dispose();
                md = new LiveModel(null, serverPort, serverName);
                md.Connect();
            }
            md.ShutdownModel();
            md.Dispose();
            md = null;
        }

        private void btnConfig_Click(object sender, EventArgs e) {
            var cfg = new CfgSettings(this);
            cfg.ShowDialog();
        }

        protected override void OnClosed(EventArgs e) {
            propMan.SaveProperties(pluginRoot);
        }

        #region Properties
        [XConfigurable, XSaved("ServerPort"), Description("Server TCP/IP port number.")]
        public int ServerPort { get => serverPort; set => serverPort = value; }

        [XConfigurable, XSaved("ServerName"), Description("Server name.")]
        public string ServerName { get => serverName; set => serverName = value; }


        [XSaved("MdName")]
        public string ModelFileName { get => cboxModelNames.Text; set => cboxModelNames.Text = value; }

        [XSaved]
        public string ScriptEditor { set => scriptEditor = value; }

        public string ModelName
        {
            get {
                if ( cboxModelNames.Text.EndsWith(".md") ) {
                    return cboxModelNames.Text.Substring(0, cboxModelNames.Text.Length - 3);
                } else {
                    return "";
                }
            }
        }

        [XSaved("CltScript")]
        public string ClientScript { get => cboxScript.Text; set => cboxScript.Text = value; }

        [XSaved("SvrArgument")]
        public string Argument {
            get => tboxArgs.Text;
            set => tboxArgs.Text = value;
        }

        [XConfigurable, XSaved("Tracing"), Description("Start the server with tracing")]
        public bool StartWithTracing { get => startWithTracing; set => startWithTracing = value; }

        [XConfigurable, Description("Command-line to start the server or the last evaluation process.")]
        public string CommandLine { get => commandLine;  }

        [XConfigurable, XSaved("MinWin"), Description("Minimize the server window.")]
        public bool MinimizeWindow { get => minimizeServerWindow; set => minimizeServerWindow = value; }
        #endregion

        private void btnShowGraph_Click(object sender, EventArgs e) {
            ConnectToServer();
            md.ShowGraph();
        }
        private const int SW_SHOWMINIMIZED = 2;

        private void btnStart_Click(object sender, EventArgs e) {
            var app = DataModeling.App.ScriptApp;
            string sf = cboxScript.Text.ToLower();
            if (sf.EndsWith("client.js")) {
                app.RunScript(DataModeling.workDir + cboxScript.Text, app.ToForm(this), Argument);
                commandLine = "JScript " + DataModeling.workDir + cboxScript.Text + " " + this.Name + " " + Argument;
            } else { // assuming it is python script.
                ScriptUtil.StartCmd(DataModeling.pythonProgamm, cboxScript.Text + " " + serverPort + " " + tboxArgs.Text, true);
                commandLine = DataModeling.pythonProgamm + " " + cboxScript.Text + " " + serverPort + " " + tboxArgs.Text;
            }
        }


        private void cboxScript_SelectedIndexChanged(object sender, EventArgs e) {
            if (cboxScript.Text == ModelTraining.newModelName) {
                int oldIndex = Math.Max(0, cboxScript.FindString(ClientScript));
                var dd = new GetScriptName(cboxScript, ClientScript, ".client.js");
                if (dd.ShowDialog() == DialogResult.OK) {
                    File.Copy(dd.InitScriptName, DataModeling.workDir + "\\" + dd.NewScriptName);
                    ClientScript = dd.NewScriptName;
                } else {
                    cboxScript.SelectedIndex = oldIndex;
                    return;
                }
                RefreshList();
                cboxScript.SelectedIndex = cboxScript.FindString(dd.NewScriptName);
                return;
            }
        }

        private void btnWeights_Click(object sender, EventArgs e) {
            if (md == null)
                ConnectToServer();
            var wList = md.ListWeights();
            if (wList != null) {
                VariableListView vv = new VariableListView(md);
                foreach (var op in wList) {
                    string[] fs = op.Split('|');
                    vv.AddRow(fs[0], fs[1]);
                }
                vv.Show();
            }
        }


        private void btnShowActivity_Click(object sender, EventArgs e) {
            if (md == null)
                ConnectToServer();
            var opList = md.ListOperations();
            if ( opList != null) {
                VariableListView vv = new VariableListView(md);
                vv.SetOperationMode();
                foreach(var op in opList) {
                    string[] fs = op.Split('|');
                    vv.AddRow(fs[0], fs[1]);
                }
                vv.Show();
            }
        }

        public InputPanel OpenInputPanel(LiveModel md) {
            var ip = new InputPanel();
            ip.CallBack = md.TestEval;
            ip.Show();
            return ip;
        }
    }
}
