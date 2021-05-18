using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.DataModeling {
    public class ModelingScript : IPluginObject {
        ModelTraining currentTrainer = null;
        ModelTest currentTester = null;
        ModelServer currentServer = null;

        public ModelingScript() {
        }

        public string Name {
            get { return "DMScript"; }
            set { }
        }

        public ModelTraining CurrentTrainer { get => currentTrainer; set => currentTrainer = value; }
        public ModelTest CurrentTester { get => currentTester; set => currentTester = value; }
        public ModelServer CurrentServer { get => currentServer; set => currentServer = value; }

        public ModelTraining NewTrainer() {
            return new ModelTraining();
        }

        public ModelTest NewTester() {
            return new ModelTest();
        }

        public string GetDefaultModelName() {
            string ns = "DataModeling";
            XmlElement pluginRoot = DataModeling.App.GetPluginDataNode(0, "DataModeling", ns, false);
            if (pluginRoot == null)
                return null;

            var nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace("dm", "DataModeling");
            var xMdName = pluginRoot.SelectSingleNode("//dm:ModelName", nsManager);

            if ( ( xMdName == null ) || string.IsNullOrEmpty(xMdName.InnerText) )
                return null;
            return xMdName.InnerText;
        }

        public IList<string> AllModels() {
            List<string> nmList = new List<string>();
            foreach (var lnk in DataModeling.modelManager.GetAllModels()) {
                nmList.Add(lnk.ModelName);
            }
            return nmList;
        }

        public bool DeleteModel(string modelName) {
            foreach (var f in Directory.EnumerateFiles(DataModeling.workDir, modelName + ".*")) {
                File.Delete(f);
            }
            return true;
        }

        public FeedforwardNetwork GetModelInfo(string modelName) {
            return DataModeling.modelManager.GetModel(modelName);
        }

        public LiveModel NewLiveModel(string modelName=null, bool connecting=true, int serverPort=ModelServer.SERVER_PORT, string serverName="localhost") {
            var md = new LiveModel(modelName, serverPort, serverName);
            if (connecting) {
                if ( serverName=="localhost" ) 
                    if (! string.IsNullOrEmpty(modelName))
                        md.StartModel(true);
                md.Connect();
            }
            return md;
        }
    }
}
