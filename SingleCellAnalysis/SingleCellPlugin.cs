using System;
using VisuMap.Plugin;
using System.Windows.Forms;

namespace VisuMap.SingleCell {
    [PluginMain]
    public class SingleCellPlugin : IPlugin {
        public static IApplication App;
        public virtual void Initialize(IApplication app) {
            App = app;
            if (app.ScriptApp.ApplicationBuild < 943) {
                MessageBox.Show("Data Modeling plugin requires VisuMap 5.0.943 or higher!",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            app.InstallMetric(new DualAffinity(MetricMode.CorCor));
            app.InstallMetric(new DualAffinity(MetricMode.EucEuc));
            app.InstallMetric(new DualAffinity(MetricMode.CorEuc));
            app.InstallMetric(new DualAffinity(MetricMode.EucCor));
            app.InstallMetric(new DualPca());
            //app.InstallScriptPlugin(new PythonNetEngine());
            app.PluginPropertyChanged += (s, e) => Root.Data.Map.Metric = null;
            app.ScriptApp.SetObject("SC.Utilities", new Utilities());
        }        

        public virtual void Dispose() { }
        public virtual string Name { get { return "SingleCellPlugin"; } }
    }
}
