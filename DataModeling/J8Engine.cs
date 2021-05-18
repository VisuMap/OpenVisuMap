using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using VisuMap.Plugin;
using VisuMap.Script;

/*
using Microsoft.ClearScript.V8;

namespace VisuMap.DataModeling {
    public class J8Engine : IScriptPlugin, IDisposable {
        const string scriptPrefix = "%";
        string scriptEditor = "notepad" ;
        V8ScriptEngine engine;
        VisuMapImp vv;

        public J8Engine() {
            var propMan = new VisuMap.Lib.PropertyManager(this, "Settings", "DataModeling");
            var pluginRoot = DataModeling.App.GetPluginDataNode(0, "DataModeling", propMan.NameSpace, true);
            propMan.LoadProperties(pluginRoot);

            engine = new V8ScriptEngine();
            vv = VisuMapImp.GetVisuMapImp();
            engine.AddHostObject("vv", vv);
            engine.AddHostObject("New", vv.New);
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
            if (scriptPath == null)
                return false;
            return scriptPath.StartsWith(scriptPrefix) || scriptPath.EndsWith(".js8");
        }

        public Form ParentForm { get; set; }

        public object ActiveControl { get; set; }

        public object ScriptArgument { get; set; }

        public object RunScript(string scriptPath, Form parentForm, object activeControl, object argument) {
            ParentForm = parentForm;
            ActiveControl = activeControl;
            ScriptArgument = argument;
            DataModeling.scriptEngine = this;


            engine.AddHostObject("pp", vv.ToForm(parentForm));
            string script = scriptPath.StartsWith(scriptPrefix) ? scriptPath.Substring(scriptPrefix.Length) : File.ReadAllText(scriptPath);
            engine.Execute(script);

            ParentForm = null;
            ActiveControl = null;
            ScriptArgument = null;
            DataModeling.scriptEngine = null;
            return null;
        }

        public void Dispose() {
            engine.Dispose();
        }

        public string FileFilter
        {
            get { return "|JavaScript V8 (*.js8)|*.js8"; }
        }
    } 
}
*/