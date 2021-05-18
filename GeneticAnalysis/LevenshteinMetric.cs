using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class LevenshteinMetric : IMetric {
        string[] motifs;
        public LevenshteinMetric() {
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            motifs = new string[dataset.Rows];
            for (int row = 0; row < dataset.Rows; row++) {
                motifs[row] = dataset.GetDataAt(row, 0);
            }
            maxLength = motifs.Max(s => s.Length);
            mem1 = null;  // This is only for the single threaded execution.
        }

        static void Swap<T>(ref T arg1,ref T arg2) {
            T temp = arg1;
            arg1 = arg2;
            arg2 = temp;
        }

        int maxLength;  // the maximal string length allowd
        [ThreadStatic] static int[] mem1, mem2, mem3;
        void InitThreadMemory(int maxi) {
            if (mem1 == null) {
                mem1 = new int[maxLength+1];
                mem2 = new int[maxLength+1];
                mem3 = new int[maxLength+1];
            } else {
                Array.Clear(mem1, 0, maxi + 1);
                Array.Clear(mem2, 0, maxi + 1);
                Array.Clear(mem3, 0, maxi + 1);
            }
        }

        //
        // The following implementation is obtained from:
        // http://stackoverflow.com/questions/9453731/how-to-calculate-distance-similarity-measure-of-given-2-strings
        //
        public double Distance(int idxI, int idxJ) {
            string source = motifs[idxI];
            string target = motifs[idxJ];
            int length1 = source.Length;
            int length2 = target.Length;

            // Ensure arrays [i] / length1 use shorter length 
            if (length1 > length2) {
                Swap(ref target, ref source);
                Swap(ref length1, ref length2);
            }

            int maxi = length1;
            int maxj = length2;

            InitThreadMemory(maxi);

            int[] dCurrent = mem1;
            int[] dMinus1 = mem2;
            int[] dMinus2 = mem3;

            int[] dSwap;

            for (int i = 0; i <= maxi; i++) { dCurrent[i] = i; }

            int jm1 = 0, im1 = 0, im2 = -1;

            for (int j = 1; j <= maxj; j++) {

                // Rotate
                dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;

                // Initialize
                int minDistance = int.MaxValue;
                dCurrent[0] = j;
                im1 = 0;
                im2 = -1;

                for (int i = 1; i <= maxi; i++) {

                    int cost = source[im1] == target[jm1] ? 0 : 1;

                    int del = dCurrent[im1] + 1;
                    int ins = dMinus1[i] + 1;
                    int sub = dMinus1[im1] + cost;

                    //Fastest execution for min value of 3 integers
                    int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                    if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                        min = Math.Min(min, dMinus2[im2] + cost);

                    dCurrent[i] = min;
                    if (min < minDistance) { minDistance = min; }
                    im1++;
                    im2++;
                }
                jm1++;
            }

            return dCurrent[maxi];
        }

        public string Name {
            get { return "Seq.Levenshtein"; }
            set { ; }
        }

        public bool IsApplicable(IDataset dataset) {
            if ((dataset.Columns > 0) && (dataset.ColumnSpecList[0].DataType == 'e'))
                return true;
            else
                return false;
        }

        public bool IsApplicable(IDataset dataset, XmlElement filterNode) {
            return false;
        }

        public IFilterEditor FilterEditor {
            get { return null; }
        }

    }
}
