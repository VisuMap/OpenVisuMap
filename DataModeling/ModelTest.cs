using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

using VisuMap.Plugin;
using VisuMap.Script;
using VisuMap.Lib;
using XSavedAttribute = VisuMap.Lib.SavedAttribute;
using XConfigurableAttribute = VisuMap.Lib.ConfigurableAttribute;

namespace VisuMap.DataModeling {
    public partial class ModelTest : Form {
        IVisuMap app;
        VisuMap.Script.IDataset dataset;
        string currentModelName;
        VisuMap.Lib.PropertyManager propMan;
        XmlElement pluginRoot;
        int logLevel = 0;
        string scriptEditor = "notepad";
        FeedforwardNetwork ff;
        string argument="";
        AutoCompleteStringCollection argHistory = new AutoCompleteStringCollection();
        bool partialData = true;
        bool readOnly;

        public ModelTest() {
            InitializeComponent();
            Directory.SetCurrentDirectory(DataModeling.workDir);
            app = DataModeling.App.ScriptApp;
            dataset = app.Dataset;
            ff = new FeedforwardNetwork();

            propMan = new VisuMap.Lib.PropertyManager(this, "Settings", "DataModeling");
            pluginRoot = DataModeling.App.GetPluginDataNode(0, "DataModeling", propMan.NameSpace, true);
            propMan.LoadProperties(pluginRoot);
            tboxArgs.Text = Argument;

            tboxArgs.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tboxArgs.AutoCompleteCustomSource = argHistory;
            tboxArgs.AutoCompleteSource = AutoCompleteSource.CustomSource;

            UpdateModelList();
            UpdateEvalScriptList();
            DataModeling.cmdServer.AddListener(CmdModelTest);
            DataModeling.mdScript.CurrentTester = this;
        }

        void UpdateModelList() {
            cboxModelNames.Items.Clear();
            foreach (string name in DataModeling.modelManager.GetAllModelNames(app.Dataset, partialData)) {
                cboxModelNames.Items.Add(name);
            }
            int idx = cboxModelNames.FindString(currentModelName);
            if (idx >= 0) {
                cboxModelNames.SelectedIndex = idx;
            } else {
                if (cboxModelNames.Items.Count > 0) {
                    cboxModelNames.SelectedIndex = 0;
                    currentModelName = cboxModelNames.Items[0].ToString();
                } else {
                    cboxModelNames.SelectedIndex = -1;
                    cboxModelNames.Text = currentModelName = "";
                }
            }
        }

        void UpdateEvalScriptList() {
            cboxScript.Items.Clear();
            foreach (var fn in Directory.GetFiles(DataModeling.workDir, "*.ev.py")) {
                cboxScript.Items.Add(fn.Substring(fn.LastIndexOf('\\') + 1));
            }
            cboxScript.Items.Add(ModelTraining.newModelName);
            cboxScript.SelectedIndex = cboxScript.FindString(ff.EvalScript);
        }

        private void BtnStart_Click(object sender, EventArgs e) {
            Argument = tboxArgs.Text;
            argHistory.Add(Argument);
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control) {
                ScriptUtil.CallCmd("cmd.exe", "/K echo " + ff.CommandLine + " && " + ff.CommandLine, true);
            } else {
                DoPrediction();
            }
        }

        #region Properties
        [XSaved]
        public string ModelName
        {
            get { return currentModelName; }
            set { currentModelName = value; }
        }

        [XConfigurable, XSaved("LogLevelPred"), Description("Logging level to run the evulation script.")]
        public int LogLevel { get => logLevel; set => logLevel = value; }

        [XConfigurable, Description("The command to run the evaluation script")]
        public string CommandLine
        {
            get => ff.CommandLine;
        }

        [XSaved("EvalArg"), Description("Optional argument to the evaluation script.")]
        public string Argument
        {
            get => argument;
            set => argument = value;
        }

        [XSaved, XConfigurable, Description("Script editor.")]
        public string ScriptEditor { get => scriptEditor; set => scriptEditor = value; }

        [XSaved]
        public string EvalScript
        {
            get => ff.EvalScript;
            set
            {
                if (File.Exists(DataModeling.workDir + value)) {
                    ff.EvalScript = value;
                    cboxScript.Text = value;
                } else {
                    string msg = "Evaluation script \"" + value + "\" not present in working directory.";
                    MessageBox.Show(msg);
                }
            }
        }

        [XSaved("ArgHist")]
        public string ArgHistory
        {
            get {
                return string.Join("|", argHistory.Cast<string>().Take(15).ToArray());
            }
            set {
                argHistory.AddRange(value.Split('|'));
            }
        }

        long jobStart = 0;
        long jobEnd = 0;
        [XConfigurable, Description("The command to run the evaluation script")]
        public string ExecutionTime {
            get => ((int)(jobEnd - jobStart) / 1000).ToString("# ### ###");
        }

        [XConfigurable, XSaved("Pdt"), Description("Enable using partial dataset.")]
        public bool PartialData { get => partialData; set => partialData = value; }

        [XConfigurable, Description("Flag to avoid saving session configuration.")]
        public bool ReadOnly
        {
            get { return readOnly; }
            set { readOnly = value; }
        }

        #endregion


        public void DoPrediction() {
            this.Cursor = Cursors.WaitCursor;
            jobStart = DateTime.Now.Ticks / 10000;

            string mdName = cboxModelNames.Text;

            if (!string.IsNullOrEmpty(mdName)) {
                if (!ff.LoadModel(cboxModelNames.Text)) {
                    MessageBox.Show("Cannot obtain model: " + cboxModelNames.Text);
                    this.Cursor = Cursors.Default;
                    return;
                }
            }

            var cfg = GetLinkConfig(cboxScript.Text);

            if( (ff.Link == null) && (cfg != LinkConfig.None) ) {
                MessageBox.Show("No data model specified!");
                this.Cursor = Cursors.Default;
                return;
            }

            switch (cfg) {
                case LinkConfig.SelectedData:
                    File.Delete(DataModeling.workDir + "predData.csv");
                    ff.Link.WriteMatrix("testData", ff.Link.GetTestInputData(true, partialData) );
                    break;

                case LinkConfig.TestData:
                    File.Delete(DataModeling.workDir + "predData.csv");
                    ff.Link.WriteMatrix("testData", ff.Link.GetTestInputData(false, partialData));
                    break;

                case LinkConfig.None:
                    break;

                default:
                    break;
            }

            string cmdArgs = cboxScript.Text + " " + mdName;
            if (! string.IsNullOrEmpty(argument) )
                cmdArgs += " " + argument;
            ScriptUtil.CallCmd(DataModeling.pythonProgamm, cmdArgs, LogLevel >= 2);
            ff.CommandLine = cmdArgs;
            this.Cursor = Cursors.Default;
            jobEnd = DateTime.Now.Ticks / 10000;
        }

        enum LinkConfig {
            None,
            SelectedData,
            TestData,
        };

        // Check whether the evaluation scripts requires the caller to preduce and consume data.
        LinkConfig GetLinkConfig(string scriptFile) {
            using (StreamReader sr = new StreamReader(DataModeling.workDir + scriptFile)) {
                while (true) {
                    string line = sr.ReadLine();
                    if (line == null) break;
                    if (line.StartsWith("#Direction")) {
                        string direction = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];
                        if ( direction == "GetSelectedData") {
                            return LinkConfig.SelectedData;
                        } else if (direction == "GetTestData") {
                            return LinkConfig.TestData;
                        } else if (direction == "NoData") {
                            return LinkConfig.None;
                        }
                    }
                }
            }
            return LinkConfig.None;
        }

        public bool SelectModel(string modelName) {
            int idx = cboxModelNames.FindString(modelName);
            if (idx < 0) {
                return false;
            } else {
                cboxModelNames.SelectedIndex = idx;
                return true;
            }
        }

        protected override void OnClosed(EventArgs e) {
            Argument = tboxArgs.Text;
            if (!readOnly)
                propMan.SaveProperties(pluginRoot);
            DataModeling.cmdServer.RemoveListener(CmdModelTest);
        }

        private void CboxModelNames_SelectedValueChanged(object sender, EventArgs e) {
            ModelName = cboxModelNames.Text;
            string fileName = DataModeling.workDir + ModelName + ".md";
            var lnk = new DataLink(fileName);
            lblDatasetName.Text = lnk.TrainingDatasetName;
            string tgtInfo = lnk.TrainingMapName + ":" + lnk.OutputLabel;
            if (tgtInfo.Length > 100)
                tgtInfo = tgtInfo.Substring(0, 100) + "...";
            lblLearningTarget.Text = tgtInfo;

            if (lnk.InputVariables != null) {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < lnk.InputVariables.Count; i++) {
                    if (i > 0) sb.Append(',');
                    sb.Append(lnk.InputVariables[i]);
                }
                lblVariables.Text = sb.ToString().Substring(0, Math.Min(40, sb.Length));
                if (sb.Length > 40) lblVariables.Text += "...";
            }

            lblLearningEpochs.Text = lnk.TrainingEpochs.ToString();
            lblUpdateTime.Text = lnk.LastUpdate.ToString();
            lblMapSize.Text = lnk.MapWidth + ", " + lnk.MapHeight + ", " + lnk.MapWidth;
        }

        private void btnConfig_Click(object sender, EventArgs e) {
            var cfg = new CfgSettings(this);
            cfg.ShowDialog();
            UpdateModelList();
        }

        private void btnEdit_Click(object sender, EventArgs e) {
            string scriptFile = cboxScript.Text;
            if (!string.IsNullOrEmpty(scriptFile)) {
                System.Diagnostics.Process.Start(scriptEditor, "\"" + scriptFile + "\"");
            } else {
                MessageBox.Show("evalution script not defined!");
            }
        }

        private void cboxScript_SelectedIndexChanged(object sender, EventArgs e) {
            if (cboxScript.Text == ModelTraining.newModelName) {
                int oldIndex = Math.Max(0, cboxScript.FindString(ff.EvalScript));
                var dd = new GetScriptName(cboxScript, ff.EvalScript, ".ev.py");
                if (dd.ShowDialog() == DialogResult.OK) {
                    File.Copy(dd.InitScriptName, DataModeling.workDir + "\\" + dd.NewScriptName);
                    ff.EvalScript = dd.NewScriptName;
                } else {
                    cboxScript.SelectedIndex = oldIndex;
                    return;
                }
                UpdateEvalScriptList();
                cboxScript.SelectedIndex = cboxScript.FindString(ff.EvalScript);
                return;
            }
            ff.EvalScript = cboxScript.Text;

        }

        private void btnShowGraph_Click(object sender, EventArgs e) {
            ScriptUtil.CallCmd(DataModeling.pythonProgamm, "ShowGraph.py " + cboxModelNames.Text, LogLevel >= 2);
        }

        const int CmdUpdateMap = 103;
        const int CmdAppMapping = 115;
        const int CmdAppMapping2 = 139;

        bool CmdModelTest(System.Net.IPEndPoint sender, int cmd, byte[] data) {
            switch (cmd) {
                case CmdUpdateMap:
                    ModelTraining.UpdateMap(System.BitConverter.ToInt32(data, 4));
                    break;

                case CmdAppMapping:
                case CmdAppMapping2:
                    Invoke(new MethodInvoker(delegate () {
                        double[][] output = null;
                        if (cmd == CmdAppMapping) {
                            output = ff.ReadOutput();
                        } else {
                            DataModeling.cmdServer.CheckTcpListener();
                            DataModeling.cmdServer.SendBackOK(sender);
                            var tcpCnt = DataModeling.cmdServer.TcpListener.AcceptTcpClient();
                            output = DataModeling.cmdServer.ReadMatrix(0, 0, tcpCnt.GetStream());
                            tcpCnt.Close();
                        }
                        var ret = ff.Link.LinkResult(output);
                        if (ret != null) 
                            DataModeling.App.MainForm.Text = "Model Evaluation: Mismatches: " + ret.Item1 + "; Av L1: " + ret.Item2.ToString("g3");
                    }));
                    break;                

                default:
                    return false;
            }
            return false;
        }
    }
}