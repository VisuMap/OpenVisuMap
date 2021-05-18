using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Drawing.Design;

using VisuMap.Lib;
using VisuMap.Script;
using PropertyManager = VisuMap.Lib.PropertyManager;
using XSavedAttribute = VisuMap.Lib.SavedAttribute;
using XConfigurableAttribute = VisuMap.Lib.ConfigurableAttribute;

namespace VisuMap.DataModeling {
    public partial class ModelTraining : Form {        
        IVisuMap app;
        FeedforwardNetwork ff;
        PropertyManager propMan;
        XmlElement pluginRoot;
        bool readOnly;
        IList<IBody> initBodyList;
        long startTime;
        int timeElapsed;
        int epochTime;
        long repeatStart=0;
        long repeatEnd=0;
        string scriptEditor = "notepad";
        string validationData = "";
        public const string newModelName = "<NewScript>";
        const string noneModelName = "<NotSave>";
        int parallelJobs = 1;
        int openJobs = 0;
        List<Process> paraJobs = new List<Process>();
        string jobArgument = "";
        int jobRepeats = 1;

        public ModelTraining() {
            InitializeComponent();
            Directory.SetCurrentDirectory(DataModeling.workDir);

            app = DataModeling.App.ScriptApp;
            ff = new FeedforwardNetwork{LogLevel = 0,  RefreshFreq = 50, Link=new DataLink()};

            propMan = new VisuMap.Lib.PropertyManager(this, "Settings", "DataModeling");
            pluginRoot = DataModeling.App.GetPluginDataNode(0, "DataModeling", propMan.NameSpace, true);
            propMan.LoadProperties(pluginRoot);

            histograms[0] = new Histogram("Cost History", null);
            DataModeling.cmdServer.AddListener(CmdModelTraining);
            
            foreach (string name in DataModeling.modelManager.GetAllModelNames(app.Dataset)) {
                cboxModelName.Items.Add(name);
            }
            cboxModelName.Items.Add(noneModelName);
            if ( string.IsNullOrEmpty(ModelName) )
                cboxModelName.Text = "ModelA";

            UpdateModelList();
            cboxModelScript.SelectedIndex = cboxModelScript.FindString(ModelScript);

            DataModeling.mdScript.CurrentTrainer = this;
            RefreshSettings();
        }

        #region Methods
       

        public ModelTraining StartTraining() {
            startTime = DateTime.Now.Ticks / 10000;

            // Clear GUI
            labelStatus.Text = "-";

            ff.Name = cboxModelName.Text;
            ff.MaxEpochs = int.Parse(tboxEpochs.Text);

            labelStatus.Text = "Fetch training data...";
            initBodyList = app.Dataset.BodyListEnabled();
            labelStatus.Text = "Start training...";
            ff.JobArgument = jobArgument;

            openJobs = parallelJobs;
            ff.JobIndex = 0;

            startStopButton1.IsRunning = true;

            ff.StopTraining(paraJobs); // stop potential old jobs.
            paraJobs.Clear();
            Cursor.Current = Cursors.WaitCursor;
            var t = new Thread(() =>
            {
                for (int jIdx = 0; jIdx < openJobs; jIdx++) {
                    if (jIdx > 0)
                        System.Threading.Thread.Sleep(1000 * (jIdx + 2));
                    ff.JobIndex = jIdx;
                    paraJobs.Add(ff.StartTraining());
                }
            });
            t.Start();
            while (!t.Join(50))
                Application.DoEvents();

            Cursor.Current = Cursors.Default;

            return this;
        }

        public ModelTraining WaitForCompletion() {
            while (startStopButton1.IsRunning) {
                System.Threading.Thread.Sleep(50);
                Application.DoEvents();
            }
            System.Threading.Thread.Sleep(50);
            Application.DoEvents();
            paraJobs.Clear();
            openJobs = 0;
            return this;
        }

        void SaveModel() {
            if (ff.Link != null) {
                ff.Link.TrainingEpochs = ff.MaxEpochs;
                if (cboxModelName.Text != noneModelName) {
                    string mdName = cboxModelName.Text;
                    if( (parallelJobs>1) && (ff.JobIndex != 0) ) {
                        mdName += ff.JobIndex;
                    }
                    if ( jobRepeats > 1) {
                        if ( ff.JobArgument != "A")
                            mdName += "_" + ff.JobArgument;
                    }
                    ff.Link.SaveModelInfo(mdName, ff.GetType().Name);
                    if (cboxModelName.FindString(mdName, 0) < 0) {
                        cboxModelName.Items.Add(mdName);
                    }
                }
            }
        }

        int DiffTypes(IList<IBody> list1, IList<IBody> list2) {
            return Enumerable.Range(0, list1.Count).Count(i => list1[i].Type != list2[i].Type);
        }

        double DiffL1(IList<IBody> list1, IList<IBody> list2) {
            double diff = 0;
            for (int i = 0; i < list1.Count; i++) {
                diff += Math.Abs(list1[i].X - list2[i].X);
                diff += Math.Abs(list1[i].Y - list2[i].Y);
                if ( app.Map.Dimension == 3 ) {
                    diff += Math.Abs(list1[i].Z - list2[i].Z);
                }
            }
            return diff;
        }

        void UpdateTrainingDataInfo() {
            var desc = ff.Link.TargetDescription();
            int outVars = (desc == DataLink.ColumnList) ? 
                ((ff.Link.OutputVariables==null)?0:ff.Link.OutputVariables.Count) : DataLink.GetTargetDimension(desc);
        }


        void UpdateModelList() {
            cboxModelScript.Items.Clear();
            foreach (var fn in Directory.GetFiles(DataModeling.workDir, "*.md.py")) {
                cboxModelScript.Items.Add(fn.Substring(fn.LastIndexOf('\\') + 1));
            }
            cboxModelScript.Items.Add(newModelName);
        }

        #endregion

        #region Event handler
        bool jobBatchAborted;
        

        private void BtnStart_Click(object sender, EventArgs e) {            
            try {
                repeatStart = DateTime.Now.Ticks / 10000;
                jobBatchAborted = false;
                for (int j = 0; j < jobRepeats; j++) {
                    string jobArgList = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // 26 letters.
                    ff.JobRepeats = this.jobRepeats;
                    this.JobArgument = jobArgList[Math.Min(j, jobArgList.Length-1)].ToString();
                    StartTraining();
                    WaitForCompletion();
                    repeatEnd = DateTime.Now.Ticks / 10000;
                    if (jobBatchAborted)
                        break;
                }
                repeatEnd = DateTime.Now.Ticks / 10000;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnEditScript_Click(object sender, EventArgs e) {
            string scriptFile = TrainingScript;
            if (scriptFile != "None") {
                System.Diagnostics.Process.Start(scriptEditor, "\"" + scriptFile + "\"");
            } else {
                MessageBox.Show("Model script not defined yet!");
            }
        }

        private void StartStopButton1_Stop(object sender, EventArgs e) {
            ff.StopTraining(paraJobs);
            paraJobs.Clear();
            foreach (var h in histograms.Values) {
                if ( h.View != null )
                    h.View.ClearCache();
            }
            jobBatchAborted = true;
        }

        private void BtnConfig_Click(object sender, EventArgs e) {
            var cfg = new CfgSettings(this);
            cfg.SetBounds(0, 0, 400, 500, BoundsSpecified.Size);
            cfg.ShowDialog();
            RefreshSettings();
        }

        private void btnShowGraph_Click(object sender, EventArgs e) {
            var mdName = cboxModelName.Text;
            if ((mdName == noneModelName) || !File.Exists(DataModeling.workDir + mdName + ".md")) {
                MessageBox.Show("Model \"" + mdName + "\" is not present!");
                return;
            }
            ScriptUtil.CallCmd(DataModeling.pythonProgamm, DataModeling.homeDir + "ShowGraph.py " + cboxModelName.Text, false);
        }

        private void cboxModelScript_SelectedIndexChanged(object sender, EventArgs e) {
            string scriptFile = cboxModelScript.Text;
            if (scriptFile == newModelName) {
                int oldIndex = Math.Max(0, cboxModelScript.FindString(ff.ModelScript));
                var dd = new GetScriptName(cboxModelScript, ff.ModelScript, ".md.py");
                if (dd.ShowDialog() == DialogResult.OK) {
                    File.Copy(dd.InitScriptName, DataModeling.workDir + "\\" + dd.NewScriptName, true);
                    scriptFile = dd.NewScriptName;
                } else {
                    cboxModelScript.SelectedIndex = oldIndex;
                    return;
                }
                UpdateModelList();
                cboxModelScript.SelectedIndex = cboxModelScript.FindString(scriptFile);
                return;
            }

            ff.Link.ScanModelScript(scriptFile, cboxModelName.Text);
            ff.ModelScript = scriptFile;
            UpdateTrainingDataInfo();
        }

        protected override void OnClosed(EventArgs e) {
            if (!readOnly) {
                propMan.SaveProperties(pluginRoot);
            }

            ff.StopTraining(paraJobs);

            DataModeling.cmdServer.RemoveListener(CmdModelTraining);

            foreach (var h in histograms.Values)
                if (h.View != null) h.View.Close();

            if (monitorMap != null) monitorMap.Close();
            if (monitorChart != null) monitorChart.Close();
        }
        #endregion

        #region Properties
        [XSaved, XConfigurable, Category("Controls"), Description("The logging level during the training process.")]
        public int LogLevel
        {
            get { return ff.LogLevel;}
            set { ff.LogLevel = value; }
        }

        [XSaved, XConfigurable, Category("Controls"), Description("Refresh training frequency.")]
        public int RefreshFreq
        {
            get { return ff.RefreshFreq; }
            set { ff.RefreshFreq = value; }
        }

        [XConfigurable, Category("Training Info"), Description("Training time in milliseconds.")]
        public string TimeElapsed
        {
            get {
                return timeElapsed.ToString("# ### ###").TrimStart();
            }
        }

        [XConfigurable, Category("Training Info"), Description("Total training time for all repeats in minutes.")]
        public string TotalTime
        {
            get
            {
                if (repeatEnd <= repeatStart)
                    return "-";
                double totalTime = (repeatEnd - repeatStart)/60000.0;
                return totalTime.ToString("# ### ###.#").TrimStart();
            }
        }

        [XConfigurable, Category("Training Info"), Description("Average epoch length in millisecond. For parallel jobs, only the first job will be reported.")]
        public string EpochTime
        {
            get { return epochTime.ToString("# ### ###").TrimStart(); }
        }

        [XConfigurable, Category("Training Info"), Description("Training time of current training jobs in minutes.")]
        public string TrainingTime
        {
            get {
                if (startStopButton1.IsRunning) {
                    double t = (DateTime.Now.Ticks / 10000 - startTime)/60000.0;
                    return t.ToString("f2");
                } else {
                    return "0";
                }
            }
        }

        [XConfigurable, Category("Training Info"), Description("Training start time.")]
        public string TrainingStarted
        {
            get
            {
                var dt = new DateTime(startTime * 10000);
                return dt.ToShortDateString() + " " + dt.ToShortTimeString();
            }
        }

        [XConfigurable, Category("Training Info"),  Description("Command-line to start the training process.")]
        public string CommandLine
        {
            get => ff.CommandLine;
        }

        [XConfigurable, Category("Training Info"), Description("Training model script")]
        public string TrainingScript
        {
            get => DataModeling.workDir + ff.GetScriptFile();
        }


        [XSaved]
        public string ModelName
        {
            get { return cboxModelName.Text; }
            set { cboxModelName.Text = value; }
        }


        [XSaved]
        public int MaxEpochs
        {
            get { return int.Parse(tboxEpochs.Text); }
            set { tboxEpochs.Text = value.ToString(); }
        }

        [XConfigurable, XSaved, Category("Controls"), Description("Data source for validation during the training process in format: [<DatasetName>:]<MapName>")]
        public string ValidationData
        {
            get => validationData;
            set => validationData = value;
        }

        [XSaved]
        public string ModelScript
        {
            get => ff.ModelScript;
            set
            {
                if (File.Exists(DataModeling.workDir + value)) {
                    ff.ModelScript = value;
                    cboxModelScript.Text = value;
                } else {
                    string msg = "Model script \"" + value + "\" not present in working directory.";
                    MessageBox.Show(msg);
                }
            }
        }

        public bool ReadOnly
        {
            get { return readOnly; }
            set { readOnly = value; }
        }

        [XSaved, XConfigurable, Category("Controls"), Description("Script editor.")]
        public string ScriptEditor { get => scriptEditor; set => scriptEditor = value; }

        [XSaved, XConfigurable, Category("Jobs"), Description("Count for repeating training jobs.")]
        public int ParallelJobs { get => parallelJobs; set => parallelJobs = Math.Min(12, Math.Max(1, value)); }

        [XSaved, XConfigurable, Category("Jobs"), Description("Optional job argument")]
        public string JobArgument { get => jobArgument; set => jobArgument = value; }

        [XSaved, XConfigurable, Category("Jobs"), Description("Traing job index.")]
        public int JobIndex
        {
            get => ff.JobIndex;
            set => ff.JobIndex = value;
        }

        [XConfigurable, Category("Jobs"), Description("Job report queue size.")]
        public string ReportQueue
        {
            get
            {
                if (histograms[0].View == null) return "0";

                var costBuffers = histograms[0].View.GetCache();
                if ( (costBuffers != null) && (costBuffers.Length>0) ) {
                    StringBuilder sb = new StringBuilder();
                    foreach(var b in costBuffers) {
                        if (sb.Length > 0) sb.Append(',');
                        sb.Append(b.Count.ToString());
                    }
                    return sb.ToString();
                } else {
                    return "0";
                }
            }
        }
        [XSaved, XConfigurable, Category("Controls"), Description("The input scaling method")]
        public DataLink.ScalingMethods InputScaling
        {
            get => ff.Link.InputScaling;
            set => ff.Link.InputScaling = value;
        }

        [XSaved, XConfigurable, Category("Controls"), Description("The output scaling method, used only for table column outputs.")]
        public DataLink.ScalingMethods OutputScaling
        {
            get => ff.Link.OutputScaling;
            set => ff.Link.OutputScaling = value;
        }

        [XSaved("JobRp"), XConfigurable, Category("Jobs"), Description("Repeats of job batchs.")]
        public int Repeats { get => jobRepeats; set => jobRepeats = Math.Max(1, value); }
        #endregion

        #region Module specifical commands
        IMap3DView monitorMap;
        dynamic clipRecorder = null;  // used to record maps in monitorMap.
        IHeatMap monitorChart;
        double[][] predMatrix = null;
        double[] revScaleFactor;
        double[] revShift;

        class Histogram {
            public string Title;
            public IHistoryView View;
            public int Count;
            public int[] groupSize;
            public Histogram(string title, IHistoryView view, int count=1, int[] groupSize=null) {
                this.Title = title;
                this.View = view;
                this.Count = count;
                this.groupSize = groupSize;
            }

            public void SetGroupColors() {
                if (groupSize != null) {
                    int i0 = 0;
                    for (int i = 0; i < groupSize.Length; i++) {
                        View.SetColors(i0, i0 + groupSize[i], i);
                        i0 += groupSize[i];
                    }
                }
            }
        }
        Dictionary<int, Histogram> histograms = new Dictionary<int, Histogram>();

        Histogram GetHistogram(int idx) {
            if (!histograms.ContainsKey(idx)) return null;
            var h = histograms[idx];
            if ((h.View == null) || h.View.TheForm.IsDisposed) {
                Invoke(new MethodInvoker(delegate () {
                    h.View = app.New.HistoryView(h.Count);
                    h.View.StepSize = 4;
                    h.View.Title = h.Title;
                    h.View.Show();
                    h.SetGroupColors();
                }));
            }
            return h;
        }

        void AddHistogram(int idx, int jobIndex, params double[] v) {
            var h = GetHistogram(idx);
            if ( (v.Length == 1) && (parallelJobs > 1) ) {
                // we need to assemble reports from multiple training jobs.
                if (h.View.CurveNumber != parallelJobs) {
                    h.View.RemoveAllCurves();
                    h.View.AddCurves(parallelJobs);
                    h.View.ClearCache();
                }
                h.View.AddStepOne(v[0], jobIndex);
            } else {
                if ( h.View.CurveNumber != v.Length) {
                    h.View.RemoveAllCurves();
                    h.View.AddCurves(h.Count);
                }
                h.View.AddStep(v);
            }

            if (h.Count == 1) {
                Invoke(new MethodInvoker(delegate () {
                    h.View.Title = h.Title + ": " + v[0].ToString("g4");
                }));
            }
        }

        const int CmdCost = 100;
        const int CmdShowPrediction = 101;
        const int CmdUpdateMap = 103;
        const int CmdSaveModel = 104;
        public const int CmdExited = 107;
        const int CmdExtHistogram = 108;
        const int CmdCfgHistogram = 109;
        const int CmdLogTitle = 113;
        const int CmdSetStatus = 116;
        const int CmdGetPredInfo = 119;
        const int CmdOpenDataset = 123;
        const int CmdShMap = 124;
        const int CmdAddStep = 128;
        const int CmdLdTraining = 130;
        const int CmdUpdateMap2 = 131;
        const int CmdRptStart = 136;

        double errorL1 = 0;
        int mismatches = 0;

        bool CmdModelTraining(System.Net.IPEndPoint sender, int cmd, byte[] data) {
            var serverSkt = DataModeling.cmdServer.ServerSocket;
            switch (cmd) {
                case CmdCost:
                    int ep = System.BitConverter.ToInt32(data, 4);
                    double v = System.BitConverter.ToSingle(data, 8);
                    int jIdx = System.BitConverter.ToInt32(data, 12);
                    if (LogLevel >= 1)
                        AddHistogram(0, jIdx, v);
                    BeginInvoke(new MethodInvoker(delegate () {
                        string msg = "epochs: " + ep.ToString() + "; cost: " + v.ToString("f4");
                        if (parallelJobs > 1)
                            msg += "; j:" + jIdx;
                        labelStatus.Text = msg;
                    }));
                    timeElapsed = (int)(DateTime.Now.Ticks / 10000 - startTime);
                    if (jIdx == 0) {
                        epochTime = (int)(timeElapsed / ep);
                    }
                    Application.DoEvents();
                    break;

                case CmdRptStart: {
                        int jobIdx = System.BitConverter.ToInt32(data, 4);
                        if (jobIdx == 0) {
                            startTime = DateTime.Now.Ticks / 10000;
                            timeElapsed = 0;
                        }
                    }
                    break;

                case CmdExtHistogram: {
                        int idxHist = System.BitConverter.ToInt32(data, 4);
                        jIdx = System.BitConverter.ToInt32(data, 8);
                        var h = GetHistogram(idxHist);
                        int vs = (data.Length - 12) / 4;
                        double[] val = new double[vs];
                        for (int i = 0; i < val.Length; i++)
                            val[i] = System.BitConverter.ToSingle(data, 12 + i * 4);
                        AddHistogram(idxHist, jIdx, val);
                    }
                    break;

                case CmdAddStep: {
                        int idxHist = System.BitConverter.ToInt32(data, 4);
                        int offset = System.BitConverter.ToInt32(data, 8);
                        var h = GetHistogram(idxHist);
                        int vs = (data.Length - 12) / 4;
                        double[] val = new double[vs];
                        for (int i = 0; i < val.Length; i++)
                            val[i] = System.BitConverter.ToSingle(data, 12 + i * 4);
                        h.View.AddStepList(val, offset);
                    }
                    break;

                case CmdCfgHistogram: {
                        int idxHist = System.BitConverter.ToInt32(data, 4);
                        int count = System.BitConverter.ToInt32(data, 8);
                        string title = System.Text.Encoding.UTF8.GetString(data, 12, data.Length - 12);
                        string[] fields = title.Split('|');
                        title = fields[0];
                        int[] groupSize = null;
                        if (fields.Length > 1) {
                            groupSize = fields[1].Split(',').Select(sz => int.Parse(sz)).ToArray();
                        }
                        if (count == 0)
                            count = this.ParallelJobs;
                        if (!histograms.ContainsKey(idxHist)) {
                            histograms.Add(idxHist, new Histogram(title, null, count, groupSize));
                        } else {
                            var hh = histograms[idxHist];
                            if ((hh.View != null) && (hh.View.CurveNumber != count)) {
                                hh.View.RemoveAllCurves();
                                hh.View.AddCurves(count);
                                hh.View.ClearCache();
                            }
                            hh.Count = count;
                            if (hh.Title != title) {
                                hh.Title = title;
                                if (hh.View != null) {
                                    Invoke(new MethodInvoker(delegate () {
                                        hh.View.Title = title;
                                    }));
                                }
                            }                            
                            hh.groupSize = groupSize;
                            hh.SetGroupColors();
                        }
                        GetHistogram(idxHist); // create the view if it is not already created.
                    }
                    break;

                case CmdGetPredInfo:
                    byte[] buf1 = BitConverter.GetBytes((float)errorL1);
                    byte[] buf2 = BitConverter.GetBytes(mismatches);
                    serverSkt.Send(buf1.Concat(buf2).ToArray(), 8, sender);
                    break;

                case CmdShowPrediction:
                    jIdx = System.BitConverter.ToInt32(data, 4);
                    ShowPrediction(jIdx, false);
                    break;

                case CmdShMap:
                    jIdx = System.BitConverter.ToInt32(data, 4);
                    DataModeling.cmdServer.CheckTcpListener();
                    DataModeling.cmdServer.SendBackOK(sender);
                    ShowPrediction(jIdx, true);
                    break;


                case CmdSaveModel:
                    ff.JobIndex = System.BitConverter.ToInt32(data, 4);
                    Invoke(new MethodInvoker(delegate () {
                        SaveModel();
                    }));
                    break;

                case CmdExited: // The pyhton job exited
                    int exitCode = System.BitConverter.ToInt32(data, 4);
                    if (exitCode != 0)
                        jobBatchAborted = true;

                    openJobs--;
                    if (openJobs == 0) {
                        Invoke(new MethodInvoker(delegate () {
                            startStopButton1.IsRunning = false;
                        }));
                        paraJobs.Clear();
                    }
                    timeElapsed = (int)(DateTime.Now.Ticks / 10000 - startTime);
                    epochTime = (ff.MaxEpochs > 0) ? (int)(timeElapsed / ff.MaxEpochs) : 0;
                    break;

                case CmdLogTitle:
                    string tgt = ff.Link.TraningTarget;
                    var monitorWin = ((tgt == DataLink.tt.Var) || (tgt == DataLink.tt.Mdl)) ? (IForm) monitorChart : monitorMap;
                    DataModeling.cmdServer.AddLogMessage(((monitorWin == null) ? "N/A" : monitorWin.Title) + "\n");
                    break;

                case CmdUpdateMap: {
                        ff.JobIndex = System.BitConverter.ToInt32(data, 4);
                        UpdateMap(ff.JobIndex);
                    }
                    break;

                case CmdUpdateMap2: {
                        ff.JobIndex = System.BitConverter.ToInt32(data, 4);
                        UpdateMap(ff.JobIndex, sender);
                    }
                    break;

                case CmdSetStatus:
                    var status = Encoding.UTF8.GetString(data, 4, data.Length - 4);
                    Invoke(new MethodInvoker(delegate () {
                        labelStatus.Text = status;
                    }));
                    break;

                case CmdOpenDataset: {
                        string msg = System.Text.Encoding.UTF8.GetString(data, 4, data.Length - 4);
                        string[] fs = msg.Split('|');
                        DataLink lnk = ff.Link;
                        string mapName = fs[0];
                        string dsName = fs[1];
                        int dataGroup = int.Parse(fs[2]);
                        if (fs[3] != "None") {
                            // this info will be used when saving the *.md file.
                            lnk.TraningTarget = fs[3];
                            lnk.OutputLabel = lnk.TargetDescription();
                        }

                        bool openFailed = false;
                        app.TheForm.Invoke(new MethodInvoker(delegate () {
                            if (!string.IsNullOrEmpty(dsName) && (dsName != app.Dataset.Name))
                                if (app.Folder.OpenDataset(dsName) == null)
                                    openFailed = true;
                            if (!string.IsNullOrEmpty(mapName) && (mapName != app.Map.Name))
                                if (app.Dataset.OpenMap(mapName) == null)
                                    openFailed = true;
                            initBodyList = app.Dataset.BodyListEnabled();
                        }));

                        if (openFailed) { 
                            DataModeling.cmdServer.SendBackFail(sender);
                            return true;
                        }

                        if (dataGroup >0) {

                            DataModeling.cmdServer.CheckTcpListener();
                            DataModeling.cmdServer.SendBackOK(sender);
                            var tcpCnt = DataModeling.cmdServer.TcpListener.AcceptTcpClient();

                            using (var bw = new BinaryWriter(new BufferedStream (tcpCnt.GetStream()) )) {
                                if ((dataGroup & 0x1) != 0) {
                                    var ntInput = lnk.GetNumberTable("", true);
                                    DataModeling.cmdServer.WriteMatrix((double[][])ntInput.Matrix, bw);
                                }
                                if ((dataGroup & 0x2) != 0) {
                                    var ntOutput = lnk.GetNumberTable(lnk.TargetDescription(), false);
                                    DataModeling.cmdServer.WriteMatrix((double[][])ntOutput.Matrix, bw);
                                }
                                bw.Flush();
                            }
                            tcpCnt.Close();
                        } else {
                            DataModeling.cmdServer.SendBackOK(sender);
                        }
                    }
                    break;


                case CmdLdTraining: {
                        string msg = System.Text.Encoding.UTF8.GetString(data, 4, data.Length - 4);
                        string[] fs = msg.Split('@');
                        string mdInput = fs[0];
                        string mdOutput = fs[1];

                        if (mdOutput == "Mdl") {
                            ff.Link.TraningTarget = ff.Link.ReadModelTarget();
                        } else {
                            ff.Link.TraningTarget = fs[1];
                            ff.Link.UnpackModelTargets(mdInput, mdOutput);
                        }
                        ff.Link.OutputLabel = ff.Link.TargetDescription();

                        int mapDim = ff.Link.MapDimension;
                        int dtFlag = 0;
                        var dtList = ff.Link.CreateTrainingDataNew(ff, validationData);
                        if (dtList.Item1 != null) dtFlag |= 1;
                        if (dtList.Item2 != null) dtFlag |= 2;
                        if (dtList.Item3 != null) dtFlag |= 4;
                        buf1 = BitConverter.GetBytes(mapDim);
                        buf2 = BitConverter.GetBytes(dtFlag);
                        serverSkt.Send(buf1.Concat(buf2).ToArray(), 8, sender);
                        if (dtFlag != 0) {
                            DataModeling.cmdServer.CheckTcpListener();
                            var tcpCnt = DataModeling.cmdServer.TcpListener.AcceptTcpClient();
                            using (var bw = new BinaryWriter(new BufferedStream(tcpCnt.GetStream()))) {
                                if (dtList.Item1 != null)
                                    DataModeling.cmdServer.WriteMatrix(dtList.Item1, bw);
                                if (dtList.Item2 != null)
                                    DataModeling.cmdServer.WriteMatrix(dtList.Item2, bw);
                                if (dtList.Item3 != null)
                                    DataModeling.cmdServer.WriteMatrix(dtList.Item3, bw);
                                bw.Flush();
                            }
                            tcpCnt.Close();
                        }
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        double[][] GetPrediction(int jIdx, bool tcpDataSource) {
            if ( tcpDataSource ) {
                var tcpCnt = DataModeling.cmdServer.TcpListener.AcceptTcpClient();
                var matrix = DataModeling.cmdServer.ReadMatrix(0, 0, tcpCnt.GetStream());
                tcpCnt.Close();
                return matrix;
            } else {
                return ff.ReadJobOutput(jIdx);
            }
        }

        // Return values indicates success.
        void ShowPrediction(int jIdx, bool tcpDataSource) {
            string tgt = ff.Link.TraningTarget;
            errorL1 = 0;
            mismatches = 0;

            double[][] predResult = null;

            if (tgt == DataLink.tt.Nul) {
                return;
            } else if ((tgt == DataLink.tt.Var) || (tgt == DataLink.tt.Mdl)) {
                if (monitorChart == null) {
                    INumberTable nt = app.Dataset.GetNumberTableEnabled();
                    if (tgt == DataLink.tt.Var) {
                        nt = nt.SelectColumnsById(ff.Link.OutputVariables);
                        revScaleFactor = ff.Link.OutputFactors.Select(f => 1.0 / f).ToArray();
                        revShift = ff.Link.OutputShifts.ToArray();
                    } else { // tgt == "Mdl", The column list is loaded from the *.md file and stored in the link.
                        nt = nt.SelectColumnsById(ff.Link.OutputVariables);
                        revScaleFactor = ff.Link.OutputFactors.Select(f => 1.0 / f).ToArray();
                        revShift = ff.Link.OutputShifts.ToArray();
                    }
                    Invoke(new MethodInvoker(delegate () {
                        monitorChart = app.New.HeatMap(nt);
                        monitorChart.Show();
                        monitorChart.ShowFrame = true;
                    }));
                }

                if (monitorChart != null && !monitorChart.TheForm.IsDisposed) {
                    double[][] result = predResult = GetPrediction(jIdx, tcpDataSource);
                    double[][] vwMtx = (double[][])monitorChart.GetNumberTable().Matrix;
                    if (predMatrix == null) {
                        predMatrix = app.Dataset.GetNumberTableEnabled().SelectColumnsById(ff.Link.OutputVariables).Matrix as double[][];
                    }
                    if ((result != null) && (result.Length == vwMtx.Length) && (result[0].Length == vwMtx[0].Length)) {
                        int rows = result.Length;
                        int cols = result[0].Length;
                        if (revScaleFactor.Length != result[0].Length) {
                            monitorChart.Close(); // The output label dimension have been changed.                                     
                            monitorChart = null;  // Recreate the window when it is called the next time.
                            predMatrix = null;
                            return;
                        }
                        for (int row = 0; row < rows; row++)
                            for (int col = 0; col < cols; col++) {
                                vwMtx[row][col] = revScaleFactor[col] * result[row][col] + revShift[col];
                                errorL1 += Math.Abs(vwMtx[row][col] - predMatrix[row][col]);
                            }
                        errorL1 /= rows;
                    }
                    Invoke(new MethodInvoker(delegate () {
                        monitorChart.Redraw();
                        monitorChart.Title = "Job: " + jIdx + ", L1 Error: " + errorL1.ToString("g7");
                    }));
                }
                ShowValidation(jIdx);
            }

            if (monitorMap == null) {
                Invoke(new MethodInvoker(delegate () {
                    var bs = app.Dataset.BodyListEnabled().Select(b => b.Clone()).ToList();
                    monitorMap = app.New.Map3DView(bs, app.Map);
                    monitorMap.ReadOnly = true;
                    monitorMap.ResetView(0);
                    monitorMap.ShowBoundingBox = true;
                    monitorMap.Show();

                    if (LogLevel >= 5) {
                        dynamic rp = app.FindPluginObject("ClipRecorder");
                        if (rp != null) {
                            clipRecorder = rp.NewRecorder().Show();
                            clipRecorder.PlayTarget = monitorMap;
                        }
                    } else {
                        clipRecorder = null;
                    }
                }));
            }
            if (!monitorMap.TheForm.IsDisposed) {
                double[][] result = (predResult!=null) ? predResult: GetPrediction(jIdx, tcpDataSource);

                if (monitorMap.BodyList.Count != initBodyList.Count) {
                    // The current dataset has been changed, we re-initialized the body list.
                    monitorMap.BodyList.Clear();
                    foreach (var b in initBodyList)
                        monitorMap.BodyList.Add(b.Clone());
                }
                var bodyList = monitorMap.BodyList.Where(b => !b.Disabled).ToList();

                DataLink.LinkResult(result, bodyList, ff.Link);

                if (ff.Link.HasClassInfo) {
                    var idList = new List<string>();
                    for (int i = 0; i < bodyList.Count; i++) {
                        bodyList[i].Hidden = bodyList[i].Type != initBodyList[i].Type;
                        if (bodyList[i].Hidden) {
                            idList.Add(bodyList[i].Id);
                        }
                    }
                    app.EventManager.RaiseItemsSelected(idList);
                }

                Invoke(new MethodInvoker(delegate () {
                    string msg = (parallelJobs > 1) ? (jIdx + ": ") : "";

                    if (ff.Link.HasClassInfo) {
                        mismatches = DiffTypes(bodyList, initBodyList);
                        msg += "Mismatches: " + mismatches + " out of " + result.Length + "; ";
                        // AddHistogram(2, misses);
                    }
                    if (ff.Link.HasPosInfo) {
                        errorL1 = (DiffL1(bodyList, initBodyList) / result.Length);
                        msg += "L1-Error: " + errorL1.ToString("f3");
                        // AddHistogram(3, err);
                    }
                    monitorMap.Title = msg;
                    monitorMap.RedrawAll();
                }));

                if( (clipRecorder != null) && ! clipRecorder.IsDisposed ) {
                    this.Invoke(new MethodInvoker(delegate () {
                        clipRecorder.AddSnapshot(monitorMap.BodyList);
                    }));
                }
            }
            ShowValidation(jIdx);
        }

        public static void UpdateMap(int jIdx, System.Net.IPEndPoint sender = null) {
            var app = DataModeling.App.ScriptApp;
            var ds = app.Dataset;
            double mapSize = Math.Min(app.Map.Width, app.Map.Height);
            double[][] result = null;
            if (sender == null) {
                result = DataLink.ReadMatrix0(DataModeling.workDir + "predData" + ".csv");
            } else { // read data from the socket.
                DataModeling.cmdServer.CheckTcpListener();
                DataModeling.cmdServer.SendBackOK(sender);
                var tcpCnt = DataModeling.cmdServer.TcpListener.AcceptTcpClient();
                result = DataModeling.cmdServer.ReadMatrix(0, 0, tcpCnt.GetStream());
                tcpCnt.Close();
            }

            int mapDim = result[0].Length;
            var bodyList = ds.BodyListEnabled();
            if (result.Length == bodyList.Count) {
                for (int i = 0; i < result.Length; i++) {
                    IBody b = bodyList[i];
                    b.X = mapSize * result[i][0];
                    b.Y = mapSize * result[i][1];
                    b.Z = (mapDim == 3) ? mapSize * result[i][2] : 0;
                }
                app.EventManager.RaiseBodyMoved();
            }
            app.TheForm.Invoke(new MethodInvoker(delegate () {
                app.Title = "Job: " + jIdx;
            }));
        }

        void ShowValidation(int jIdx) {
            var app = DataModeling.App.ScriptApp;
            var vb = ff.Link.ValidationBodies;
            if (vb == null) return;
            var bList = vb.Select(b => app.New.Body(b.Id)).ToList();
            var result = ff.Link.ReadMatrix("validationOut");
            if (result == null) return;

            if ((ff.Link.TraningTarget == DataLink.tt.Var) || (ff.Link.TraningTarget == DataLink.tt.Mdl)) {
                double errL1 = 0;
                int cols = result[0].Length;
                IList<double> factors = ff.Link.OutputFactors;
                for(int row=0; row<result.Length; row++) {
                    var R0 = result[row];
                    var R1 = ff.Link.ValidationOutput[row];
                    for (int col = 0; col < cols; col++)
                        errL1 += Math.Abs(R0[col] / factors[col] - R1[col]);
                }
                errL1 /= result.Length;

                if (histograms.ContainsKey(1)) {
                    AddHistogram(1, jIdx, errL1);
                }
            } else {
                DataLink.LinkResult(result, bList, ff.Link);
                double errL1 = DiffL1(bList, vb) / result.Length;

                if (ff.Link.HasPosInfo && histograms.ContainsKey(1) ) {
                    AddHistogram(1, jIdx, errL1);
                }

                if (ff.Link.HasClassInfo && !monitorMap.TheForm.IsDisposed) {
                    Invoke(new MethodInvoker(delegate () {
                        monitorMap.Title += "; Validation Mises: " + DiffTypes(vb, bList);
                    }));
                }
            }
        }
        #endregion

        private void cboxModelName_SelectedIndexChanged(object sender, EventArgs e) {
            if (cboxModelName.Text != noneModelName) {
                ff.Link.LoadModelInfo(DataModeling.workDir + cboxModelName.Text + ".md");
                UpdateTrainingDataInfo();
            }
        }

        private void cboxJobs_SelectedIndexChanged(object sender, EventArgs e) {
            this.ParallelJobs = 1 + cboxJobs.SelectedIndex;
        }

        private void cbLogLevel_SelectedIndexChanged(object sender, EventArgs e) {
            this.LogLevel = cbLogLevel.SelectedIndex;
        }

        private void tbxLogFreq_TextChanged(object sender, EventArgs e) {
            string s = tbxLogFreq.Text;
            if (string.IsNullOrEmpty(s))
                return;
            int freq = 0;
            if ( int.TryParse(s, out freq) ) {
                RefreshFreq = freq;
            } else {
                MessageBox.Show("Please enter integer as log frequency!");
            }
        }

        void RefreshSettings() {
            tbxLogFreq.Text = RefreshFreq.ToString();
            cbLogLevel.SelectedIndex = this.LogLevel;
            cboxJobs.SelectedIndex = this.ParallelJobs - 1;
        }
    }
}
