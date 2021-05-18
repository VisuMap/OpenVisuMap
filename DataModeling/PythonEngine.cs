using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using VisuMap.Plugin;
using VisuMap.Script;
using VisuMap.Lib;

namespace VisuMap.DataModeling {
    public class PythonEngine : IScriptPlugin, IDisposable {
        const string scriptPrefix = "@";
        string scriptEditor = "notepad" ;

        public PythonEngine() {
            var propMan = new VisuMap.Lib.PropertyManager(this, "Settings", "DataModeling");
            var pluginRoot = DataModeling.App.GetPluginDataNode(0, "DataModeling", propMan.NameSpace, true);
            propMan.LoadProperties(pluginRoot);
        }

        [Saved]
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
            DataModeling.scriptEngine = this;

            if (scriptPath.StartsWith(scriptPrefix)) {
                string script = scriptPath.Substring(scriptPrefix.Length);
                ScriptUtil.CallCmd(DataModeling.pythonProgamm, "-c \"" + script + "\" ", true);
            } else {
                ScriptUtil.CallCmd(DataModeling.pythonProgamm, "\"" + scriptPath + "\" ", true);
            }

            ParentForm = null;
            ActiveControl = null;
            ScriptArgument = null;
            DataModeling.scriptEngine = null;
            return null;
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
