using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using VisuMap.Script;

namespace VisuMap.DataModeling {
    public class ModelManager {
        public ModelManager() {            
        }

        public IList<string> GetAllModelNames(VisuMap.Script.IDataset dataset, bool allowPartialData = false) {
            List<string> modelNames = new List<string>();
            foreach (var f in System.IO.Directory.EnumerateFiles(DataModeling.workDir)) {
                if (f.EndsWith(".md")) {
                    if (!f.EndsWith("readme.md", StringComparison.CurrentCultureIgnoreCase)) {
                        modelNames.Add(Path.GetFileNameWithoutExtension(f));
                    }
                }
            }
            return modelNames;
        }

        public IList<DataLink> GetAllModels() {
            List<DataLink> modelList = new List<DataLink>();
            foreach (var f in System.IO.Directory.EnumerateFiles(DataModeling.workDir)) {
                if (f.EndsWith(".md")) {
                    modelList.Add( new DataLink(f) );
                }
            }
            return modelList;
        }


        public FeedforwardNetwork GetModel(string name) {
            DataLink lnk = new DataLink(DataModeling.workDir + name + ".md");

            if (lnk.ModelType.EndsWith("FeedforwardNetwork")) {
                var model = new FeedforwardNetwork {
                    Link = lnk,
                    Name = name
                };
                return model;
            } else {
                return null;
            }
        }
    }
}
