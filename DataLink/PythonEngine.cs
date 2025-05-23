﻿using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using VisuMap.Plugin;
using VisuMap.Script;
using VisuMap.Lib;

namespace VisuMap.DataLink {
    public class PythonEngine : IScriptPlugin, IDisposable {
        const string scriptPrefix = "$";
        string scriptEditor = "notepad" ;
        string pythonProgamm;

        public PythonEngine() {
            scriptEditor = DataLink.App.ScriptApp.GetProperty("DataLink.PythonEditor", scriptEditor);
            pythonProgamm = DataLink.App.ScriptApp.GetProperty("DataLink.PythonProg", "python.exe");
            if (pythonProgamm.Length == 0)
                pythonProgamm = "python";
        }

        public string ScriptEditor { get => scriptEditor; set => scriptEditor = value; }

        public string OpenScript(string scriptPath, IForm parentForm) {
            if (scriptPath.StartsWith(scriptPrefix)) {
                string tmp = Path.GetTempFileName();
                using (var sw = new StreamWriter(tmp))
                    sw.Write(scriptPath.Substring(scriptPrefix.Length));                
                var proc = Process.Start(scriptEditor, "\"" + tmp + "\"");
                while (!proc.WaitForExit(100))
                    Application.DoEvents();
                string sString = scriptPrefix + File.ReadAllText(tmp);
                File.Delete(tmp);
                return sString;
            } else {
                System.Diagnostics.Process.Start(scriptEditor, "\"" + scriptPath + "\"");
                return scriptPath;
            }
        }

        public bool ValidateScript(string scriptPath) {
            if (scriptPath == null) return false;
            return scriptPath.StartsWith(scriptPrefix) || scriptPath.EndsWith(".py");
        }

        public Form ParentForm { get; set; }

        public object ActiveControl { get; set; }

        public object ScriptArgument { get; set; }

        public object RunScript(string scriptPath, Form parentForm, object activeControl, object argument) {
            ParentForm = parentForm;
            ActiveControl = activeControl;
            ScriptArgument = argument;
            DataLink.scriptEngine = this;

            if (scriptPath.StartsWith(scriptPrefix)) {
                string script = scriptPath.Substring(scriptPrefix.Length);
                CallCmd(pythonProgamm, "-c \"" + script + "\" ", true);
            } else {
                CallCmd(pythonProgamm, "\"" + scriptPath + "\" ", true);
            }

            ParentForm = null;
            ActiveControl = null;
            ScriptArgument = null;
            DataLink.scriptEngine = null;
            return null;
        }

        static Process StartCmd(string progName, string argList, bool showWindow) {
            ProcessStartInfo info = new ProcessStartInfo(progName, argList) {
                WindowStyle = showWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
            };
            return Process.Start(info);
        }

        static void CallCmd(string progName, string argList, bool showWindow) {
            int idx = progName.IndexOf(' ');
            if ( idx > 0) {
                argList = progName.Substring(idx) + " " + argList;
                progName = progName.Substring(0, idx);
            }

            /*
            var proc = StartCmd(progName, argList, showWindow);
            while (!proc.WaitForExit(100))
                Application.DoEvents();
            */

            var thr = new Thread(() => {StartCmd(progName, argList, showWindow).WaitForExit();});
            thr.Start();
            while (!thr.Join(100)) {
                Thread.Sleep(100);
                Application.DoEvents();
            }
        }

        public void Dispose() {
        }

        public string FileFilter
        {
            get { return "|Python Scripts (*.py)|*.py"; }
        }

        public string CodePrefix => scriptPrefix;
    }
}
