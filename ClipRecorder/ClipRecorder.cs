using System;
using System.Collections.Generic;
using System.Text;
using VisuMap.Plugin;
using System.Windows.Forms;

namespace ClipRecorder {
    [PluginMain]
    internal class ClipRecorder : IPlugin {
        public static IApplication App;
        public static RecorderForm CurrentRecorder;

        public virtual void Initialize(IApplication app) {
            App = app;
            ToolStripMenuItem miPlugin = App.GetPluginMenu();

            miPlugin.DropDownItems.Add("Clip Recorder", null, OpenRecorder);
            miPlugin.DropDownItems.Add("PCA Tracking", null, PcaTracking);

            App.InstallPluginObject(new ScriptApp());
        }

        void OpenRecorder(object sender, EventArgs e) {
            CurrentRecorder = new RecorderForm(App);
            CurrentRecorder.Show();
        }

        void PcaTracking(object sender, EventArgs e) {
            string pluginHome = App.ScriptApp.ApplicationData + "\\plugins\\Clip Recorder\\";
            App.ScriptApp.New.ScriptEditor(pluginHome + "TrackingPca.js").Show();
        }

        public virtual void Dispose() { }
        public virtual string Name { get { return "ClipRecorder"; } }
    }

}
