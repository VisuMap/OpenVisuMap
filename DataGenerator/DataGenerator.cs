using System;
using System.Windows.Forms;
using System.Collections.Generic;

using VisuMap.Plugin;
using VisuMap.Script;

namespace DataGenerator {
    [PluginMain]
    public class DataGenerator : IPlugin {
        public static IApplication App;
        public virtual void Initialize(IApplication app) {
            App = app;
            ToolStripMenuItem miPlugin = app.GetPluginMenu();
            miPlugin.DropDownItems.Add(new ToolStripMenuItem("New 3D Data", null, Open3DPanel));
            miPlugin.DropDownItems.Add(new ToolStripMenuItem("Synchronize 3D", null, Synchronize3D));
            miPlugin.DropDownItems.Add(new ToolStripMenuItem("Script Data Generator", null, OpenScriptSample));           

            app.InstallPluginObject( new DataGeneratorScript() );
        }

        void Open3DPanel(object sender, EventArgs e) {
            (new Dataset3DPanel()).Show();
        }

        void Synchronize3D(object sender, EventArgs e) {
            Synchronize3D();
        }

        public static void OpenScriptSample(object sender, EventArgs e) {
            App.ScriptApp.New.ScriptEditor(App.ScriptApp.ApplicationData + "\\plugins\\Data Generator\\ScriptSample.js").Show();
        }

        public static void Synchronize3D() {
            IDataset ds = App.ScriptApp.Dataset;
            if ( (ds.Columns != 3) && (ds.Columns!=2) ) {
                MessageBox.Show("The dataset must have 2 or 3 numerical columns!");
                return;
            }

            IList<IBody> bodies = ds.BodyList;
            for(int i=0; i<bodies.Count; i++) {
                IBody b = bodies[i];
                ds.SetDataAt(i, 0, b.X.ToString("g4"));
                ds.SetDataAt(i, 1, b.Y.ToString("g4"));
                if (ds.Columns == 3) {
                    ds.SetDataAt(i, 2, b.Z.ToString("g4"));
                }
            }
            ds.CommitChanges();
        }

        public virtual void Dispose() { }
        public virtual string Name { get { return "DataGenerator"; } }
    }
}
