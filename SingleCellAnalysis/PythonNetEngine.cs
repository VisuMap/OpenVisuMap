using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using VisuMap.Plugin;
using VisuMap.Script;
using Python.Runtime;

namespace VisuMap.SingleCell {
    public class PythonNetEngine : IScriptPlugin, IDisposable {
        const string scriptPrefix = "@";
        string scriptEditor = "notepad";

        public PythonNetEngine() {
            ;
        }

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

        public object RunScript(string scriptPath, Form parentForm, object activeControl, object argument) {
            string code = scriptPath.StartsWith(scriptPrefix)
                ? scriptPath.Substring(scriptPrefix.Length) 
                : File.ReadAllText(scriptPath);

            using (Py.GIL())
            using (var pyScope = Py.CreateScope()) {
                var vv = VisuMapImp.GetVisuMapImp();
                pyScope.Set("vv", vv.ToPython());
                pyScope.Set("New", vv.New.ToPython());
                pyScope.Set("pp", vv.ToForm(parentForm).ToPython());                
                    pyScope.Exec(code);
            }
            return null;
        }

        public bool ValidateScript(string scriptPath) {
            if (scriptPath == null) return false;
            return scriptPath.StartsWith(scriptPrefix) || scriptPath.EndsWith(".py");
        }

        public Form ParentForm { get; set; }

        public object ActiveControl { get; set; }

        public object ScriptArgument { get; set; }

        public void Dispose() {
        }

        public string FileFilter
        {
            get { return "|PythonNet Scripts (*.py)|*.py"; }
        }

        public string CodePrefix => scriptPrefix;
    }
}