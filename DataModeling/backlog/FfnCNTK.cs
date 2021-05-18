using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace VisuMap.DataModeling {
    public class FfnCNTK : IffEngine {
        DataLink link;        
        string name;
        string workDir;
        int maxEpochs;
        int logLevel;
        int refreshFreq;
        Process cmdProc;

        public FfnCNTK() {
            workDir = DataModeling.workDir;
        }

        #region Properties
        public int MaxEpochs {
            get { return maxEpochs; }
            set { maxEpochs = value; }
        }

        public string Name {
            get { return name; }
            set { name = value; }
        }

        public DataLink Link {
            get { return link; }
            set { link = value; }
        }

        public int LogLevel {
            get { return logLevel; }
            set { logLevel = value; }
        }

        public int RefreshFreq {
            get { return refreshFreq; }
            set { refreshFreq = value; }
        }
        #endregion

        public bool SetTrainingData(double[][] inputData, double[][] outputData) {
            link.WriteMatrix("inData", inputData);
            link.WriteMatrix("outData", outputData);
            return true;
        }

        public bool SetValidationData(double[][] valiationData) {
            link.WriteMatrix("validationData", valiationData);
            return true;
        }

        public bool StartTraining() {
            string argv = (link.IsClassifier ? "cls" : "reg")
            + " \"" + name + "\""
            + " " + maxEpochs
            + " " + logLevel
            + " " + refreshFreq;
            cmdProc = ScriptUtil.StartCmd("python", "FeedForwardNetwork.py " + argv, logLevel >= 2);
            return true;
        }

        public void StopTraining() {
            if ((cmdProc != null) && !cmdProc.HasExited) {
                cmdProc.Kill();
            }
            cmdProc = null;
        }

        public void WaitForCompletion() {
            if ((cmdProc != null) && !cmdProc.HasExited) {
                while (!cmdProc.WaitForExit(100))
                    Application.DoEvents();
            }
            cmdProc = null;
        }

        public double[][] Evaluate(double[][] testData) {
            link.WriteMatrix("testData", testData);
            ScriptUtil.CallCmd("python", "EvalModel.py " + name, logLevel >= 2);
            return ReadOutput();
        }

        public double[][] ReadOutput() {
            return link.ReadMatrix("predData");
        }
    }
}
