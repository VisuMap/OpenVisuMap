using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    [PluginMain]
    public class GeneticAnalysis : IPlugin {
        public static IApplication App;
        public virtual void Initialize(IApplication app) {
            App = app;
            App.InstallFileImporter(new FastaNt());
            App.InstallFileImporter(new FastQ());
            App.InstallFileImporter(new Bedgraph());
            App.InstallPluginObject(new SeqAnalysis());
            App.InstallPluginObject(new SnpDataReader());

            /*
            App.InstallMetric(new SplicingAffinity2());
            App.InstallMetric(new MotifAffinity());
            */

            //App.InstallMetric(new AcgtMetric());
            App.InstallMetric(new LevenshteinMetric());
            App.InstallMetric(new NeedlemanWunschMetric());
            App.InstallMetric(new SmithWatermanMetric());
        }


        public virtual void Dispose() { }
        public virtual string Name { get { return "CustomImporter"; } }
    }
}
