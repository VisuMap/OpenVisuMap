using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class Bedgraph : IFileImporter {
        struct WigItem {
            public WigItem(long begin, long end, float value) {
                this.begin = begin;
                this.end = end;
                this.value = value;
            }
            public long begin;
            public long end;
            public float value;
        };


        public bool ImportFile(string fileName) {
            IVisuMap app = GeneticAnalysis.App.ScriptApp;
            var fn = fileName.ToLower();
            if (!( fn.EndsWith(".bedgraph") || fn.EndsWith(".wig") ))
                return false;
            FileInfo fInfo = new FileInfo(fileName);
            string shortName = fInfo.Name.Substring(0, fInfo.Name.LastIndexOf('.'));
            string chrName = null;
            List<WigItem> items = new List<WigItem>();

            // aux to handle fixedStep type data.
            bool fixedStep = false;
            List<List<float>> fixedList = new List<List<float>>();
            List<int[]> fixedInfo = new List<int[]>();
            List<float> fixedValues = null;

            string[] fs = null;

            using (StreamReader tr = new StreamReader(fileName)) {
                char[] sp = new char[] { '\t', ' ' };
                while (!tr.EndOfStream) {
                    string line = tr.ReadLine();                    
                    if (line.StartsWith("#") || (line.Length == 0))
                        continue;

                    if (line.StartsWith("fixedStep")) {
                        fs = line.Split(sp);
                        if (chrName == null) { 
                            chrName = fs[1].Substring(6);
                        } else {
                            if (chrName != fs[1].Substring(6))
                                break;
                        }
                        fixedStep = true;
                        
                        int start = int.Parse(fs[2].Substring(6));
                        int step = int.Parse(fs[3].Substring(5));
                        int span = 1;
                        if (fs.Length>=5)
                            span = int.Parse(fs[4].Substring(5));
                        fixedValues = new List<float>();
                        fixedList.Add(fixedValues);
                        fixedInfo.Add(new int[] { start, step, span });
                        continue;
                    }

                    if (fixedStep) {
                        fixedValues.Add(float.Parse(line));
                    } else {
                        fs = line.Split(sp);
                        if (fs.Length != 4)
                            continue;
                        if (chrName == null)
                            chrName = fs[0];
                        if (chrName != fs[0])
                            continue;
                        items.Add(new WigItem(long.Parse(fs[1]), long.Parse(fs[2]), (float)double.Parse(fs[3])));
                    }
                }
            }

            float[] values = null;
            if (fixedStep) {
                int L = fixedInfo.Count - 1;
                int N = fixedInfo[L][0] + fixedList[L].Count * fixedInfo[L][1];
                values = new float[N]; 
                for (int i=0; i<=L; i++) {
                    int start = fixedInfo[i][0];
                    int step = fixedInfo[i][1];
                    int span = fixedInfo[i][2];
                    var vs = fixedList[i];
                    for (int k=0; k<vs.Count; k++)
                        for(int s=0; s<span; s++) 
                            values[start + k * step + s] = vs[k];
                }
            } else {
                int N = (int)(items[items.Count - 1].end);
                values = new float[N];
                foreach (var it in items) {
                    for (long i = it.begin; i < it.end; i++)
                        values[i] = it.value;
                }
            }

            var bbv = app.New.BigBarView(values);
            bbv.Title = "Wig data: " + fileName;
            bbv.Show();            
            return true;
        }
        
        public string FileNameFilter {
            get { return "Bedgraph Files(*.wig,*.bedgraph)|*.wig;*.bedgraph"; }
        }
    }
}
