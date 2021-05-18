using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;


namespace VisuMap.DataModeling {
    public class FeedforwardNetwork {
        DataLink link;        
        string name;
        int maxEpochs = 1000;
        int logLevel = 2;
        int refreshFreq = 20;
        int jobIndex = 0;
        string jobArgument = "";
        int jobRepeats;   // the job batch repeats

        string cmdLine;
        string evalScript = "Eval.ev.py";
        string modelScript = "FFModel.md.py";
    
        public FeedforwardNetwork() {
        }

        #region properties
        public int MaxEpochs {
            get { return maxEpochs; }
            set { maxEpochs = value; }
        }

        public int LogLevel {
            get { return logLevel; }
            set { logLevel = value; }
        }

        public int RefreshFreq {
            get { return refreshFreq; }
            set { refreshFreq = value; }
        }

        public string Name {
            get { return name; }
            set { name = value; }
        }

        public DataLink Link {
            get { return link; }
            set { link = value; }
        }

        public string EvalScript
        {
            get => evalScript;
            set => evalScript = value;
        }
        #endregion

        public bool LoadModel(string modelName) {
            try {
                DataLink lnk = new DataLink(DataModeling.workDir + modelName + ".md");

                if (lnk.ModelType.EndsWith("FeedforwardNetwork")) {
                    this.Link = lnk;
                    this.name = modelName;
                    return true;
                } else {
                    return false;
                }
            } catch(Exception) {
                return false;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        public Process StartTraining() {
            string scriptFile = GetScriptFile();
            if ( ! File.Exists(scriptFile) ) {
                MessageBox.Show("Model script " + scriptFile + " does not exists in working directory!\nPlease select a valid model script.");
                return null;
            }
            var mdName = name;
            if (jobIndex > 0) mdName += jobIndex;
            if (this.jobRepeats > 1) {
                if (this.JobArgument != "A")
                    mdName += "_" + this.JobArgument;
            }

            cmdLine = scriptFile + " \"" + mdName + "\" " + maxEpochs + " " 
                + logLevel + " " + refreshFreq + " "  +  jobIndex + " \"" + jobArgument + "\"";

            Process cmdProc;
            if (logLevel >= 5) {
                cmdProc = ScriptUtil.StartCmd("cmd.exe", "/K python " + cmdLine, true);
            } else {
                cmdProc = ScriptUtil.StartCmd(DataModeling.pythonProgamm, cmdLine, logLevel >= 2);
            }

            cmdProc.EnableRaisingEvents = true;
            cmdProc.Exited += CmdProc_Exited;

            System.Threading.Thread.Sleep(50);
            if (!cmdProc.HasExited) {
                SetWindowPos(cmdProc.MainWindowHandle, IntPtr.Zero, 10 + jobIndex * 120, 10 + jobIndex * 80, 380, 700, 0x0040);
            }

            return cmdProc;
        }

        private void CmdProc_Exited(object sender, EventArgs e) {
            Process proc = sender as Process;
            if (proc != null) { 
                UdpClient snd = new UdpClient();
                var rcv = new IPEndPoint(IPAddress.Parse("127.0.0.1"), DataModeling.monitorPort);
                int[] data = new int[] { ModelTraining.CmdExited, proc.ExitCode };
                byte[] pkt = new byte[8];
                Buffer.BlockCopy(data, 0, pkt, 0, 8);
                snd.Send(pkt, 8, rcv);
                snd.Close();
                proc = null;
            }
        }

        public string CommandLine
        {
            get { return DataModeling.pythonProgamm + " " + cmdLine; }
            set => cmdLine = value;
        }

        public string GetScriptFile() {
            return ModelScript;
        }

        public string ModelScript {
            get => modelScript;
            set => modelScript = value;
        }
        public int JobIndex { get => jobIndex; set => jobIndex = value; }

        public string JobArgument { get => jobArgument; set => jobArgument = value; }
        public int JobRepeats { get => jobRepeats; set => jobRepeats = value; }

        public void StopTraining(IList<Process> jobs) {
            foreach(var proc in jobs) 
                if ((proc != null) && !proc.HasExited)
                    proc.Kill();
        }

        public double[][] Evaluate(double[][] testData) {
            link.WriteMatrix("testData", testData);
            link.DeleteDataFile("predData");
            cmdLine = evalScript + " \"" + name + "\"";
            ScriptUtil.CallCmd(DataModeling.pythonProgamm, cmdLine, logLevel>=2);
            return ReadOutput();
        }

        public double[][] ReadOutput() {
            return link?.ReadMatrix("predData");
        }

        public double[][] ReadJobOutput(int jobIndex) {
            return link?.ReadMatrix("predData_" + jobIndex);
        }
    }
}
