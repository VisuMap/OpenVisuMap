using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using EntityInfo = System.Tuple<int, string, string, int>;
using System.IO;
using VisuMap.Script;

namespace VisuMap {
    partial class SeqModeling {
        string pdbTitle = null;
        List<IBody> heteroChains = null;
        Dictionary<string, string> acc2chain = null;
        List<EntityInfo> entityTable = null;
        public List<EntityInfo> EntityTable { get => entityTable; }

        public void SaveChain(string cacheFile, IList<IBody> bodyList) {
            using (StreamWriter sw = new StreamWriter(cacheFile)) {
                foreach (var b in bodyList) {
                    sw.WriteLine($"{b.Id}|{b.Name}|{b.Type}|{b.X:f2}|{b.Y:f2}|{b.Z:f2}");
                }
            }
        }

        public List<IBody> LoadChain3D(string cacheFile) {
            string[] lines = File.ReadAllLines(cacheFile);
            IBody[] bList = new IBody[lines.Length];
            MT.Loop(0, lines.Length, lineIdx => {
                string line = lines[lineIdx];
                if (line == null)
                    return;
                string[] fs = line.Split('|');
                Body b = new Body(fs[0]);
                b.Name = fs[1];
                b.Type = short.Parse(fs[2]);
                b.X = float.Parse(fs[3]);
                b.Y = float.Parse(fs[4]);
                b.Z = float.Parse(fs[5]);
                bList[lineIdx] = b;
            });
            return bList.ToList();
        }

        public string LoadChainSeq(string cacheFile) {
            StringBuilder sb = new StringBuilder();
            using (TextReader tr = new StreamReader(cacheFile)) {
                while (true) {
                    string line = tr.ReadLine();
                    if (line == null)
                        break;
                    int idx = line.IndexOf('|');
                    char c = line[idx + 1];  // The first char of the Name field.
                    if ((c == 'r') || (c == 'd'))
                        c = char.ToLower(line[idx + 3]);
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }


        public List<IBody> LoadCif(string fileName, List<string> chainNames) {
            List<IBody> bList = null;
            HashSet<int> betaSet = new HashSet<int>();
            HashSet<int> helixSet = new HashSet<int>();
            entityTable = null;
            using (TextReader tr = new StreamReader(fileName)) {
                string L = tr.ReadLine();
                if (!L.StartsWith("data_"))
                    return null;
                while (true) {
                    L = tr.ReadLine();
                    if (L == null)
                        break;
                    if (L.StartsWith("_struct_sheet_range.end_auth_seq_id")) {
                        LoadBetaSheet(tr, betaSet);
                    } else if (L.StartsWith("_struct_conf.pdbx_PDB_helix_length")) {
                        LoadHelix(tr, helixSet);
                    } else if (L.StartsWith("_struct_conf.conf_type_id")) {
                        if (L.TrimEnd().EndsWith("HELX_P"))
                            LoadHelix2(tr, helixSet);
                    } else if (L.StartsWith("_struct.title")) {
                        pdbTitle = GetPDBTitle(L, tr);
                    } else if (L.StartsWith("_struct_ref_seq.align_id")) {
                        //acc2chain = GetAcc2Chain(tr);
                    } else if (L.StartsWith("_atom_site.")) {
                        bList = LoadAtoms(tr, helixSet, betaSet, chainNames);
                        break;
                    } else if (L.StartsWith("_entity.details")) {
                        //LoadEnityTable(tr);
                    }
                }
            }
            return bList;
        }

        public List<List<IBody>> SplitByChainName(List<IBody> bodyList) {
            if ((bodyList == null) || (bodyList.Count == 0))
                return null;
            List<List<IBody>> chainList = new List<List<IBody>>();
            int iBegin = 0;
            string curName = bodyList[0].Name.Split('.')[2];
            for (int iEnd = 1; iEnd < bodyList.Count; iEnd++) {
                string chName = bodyList[iEnd].Name.Split('.')[2];
                if (chName != curName) {
                    chainList.Add(bodyList.GetRange(iBegin, iEnd - iBegin));
                    iBegin = iEnd;
                    curName = chName;
                }
            }
            if (iBegin < bodyList.Count)
                chainList.Add(bodyList.GetRange(iBegin, bodyList.Count - iBegin));
            return chainList;
        }

        public string ToSequence(List<IBody> bList) {
            if ((bList == null) || (bList.Count == 0))
                return "";
            StringBuilder sb = new StringBuilder();
            string[] fs = bList[0].Name.Split('.');
            int chIdx = (fs[0] == "r") || (fs[0] == "d") ? 2 : 0;
            foreach (var b in bList)
                sb.Append(b.Name[chIdx]);
            return sb.ToString();
        }

        public List<EntityInfo> GetEntityTable(string fileName) {
            entityTable = null;
            using (TextReader tr = new StreamReader(fileName)) {
                string L = tr.ReadLine();
                if (!L.StartsWith("data_"))
                    return null;
                while (true) {
                    L = tr.ReadLine();
                    if (L == null)
                        break;
                    if (L.StartsWith("_entity.details")) {
                        LoadEnityTable(tr);
                        break;
                    }
                }
            }
            return entityTable;
        }

        void LoadEnityTable(TextReader tr) {
            entityTable = new List<EntityInfo>();
            List<string> fs = new List<string>();
            while (true) {
                string L = tr.ReadLine();
                if (L[0] == '#')
                    break;
                if (L[0] == ';') {
                    continue;
                }
                string[] bs = L.Split(new char[] { '\'', '\"' });
                fs.AddRange(L.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            // Merge spices of quoated string togehter.
            List<string> fList = new List<string>();
            string parF = "";
            bool inWord = false;
            char quota = '\'';

            foreach (string f in fs) {
                if (inWord) {
                    if (f[f.Length - 1] == quota) {
                        fList.Add(parF + " " + f.Substring(0, f.Length - 1));
                        inWord = false;
                    } else {
                        parF += " " + f;
                    }
                } else {
                    if ((f[0] == '\'') || (f[0] == '\"')) {
                        if (f[f.Length - 1] == f[0]) {
                            fList.Add(f.Substring(1, f.Length - 2));
                            inWord = false;
                        } else {
                            parF = f.Substring(1);
                            inWord = true;
                            quota = f[0];
                        }
                    } else
                        fList.Add(f);
                }
            }

            try {
                for (int k = 0; k < fList.Count; k += 10) {
                    if ((k + 5) < fList.Count) {
                        int entId = int.Parse(fList[k]);
                        string entType = fList[k + 1];
                        string entDesc = fList[k + 3];
                        int entCnt = char.IsDigit(fList[k + 5][0]) ? int.Parse(fList[k + 5]) : 1;
                        if (entId > 0)
                            entityTable.Add(new EntityInfo(entId, entType, entDesc, entCnt));
                    }
                }
            } catch (Exception) {
                // The entity section sometimes has wild format which still causes SOME TROUBERS.
            }
        }

        void LoadBetaSheet(TextReader tr, HashSet<int> betaSet) {
            while (true) {
                string L = tr.ReadLine();
                if (L[0] == '#')
                    break;
                string[] fs = L.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int idx0 = int.Parse(fs[4]) - 1;
                int idx1 = int.Parse(fs[8]) + 1;
                for (int i = idx0; i < idx1; i++)
                    betaSet.Add(i);
            }
        }

        void LoadHelix(TextReader tr, HashSet<int> helixSet) {
            while (true) {
                string L = tr.ReadLine();
                if (L[0] == '#')
                    break;
                if (L[0] == ';')
                    continue;
                string[] fs = L.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (fs.Length < 10)
                    continue;
                int idx0 = int.Parse(fs[5]);
                int idx1 = int.Parse(fs[9]) + 1;
                for (int i = idx0; i < idx1; i++)
                    helixSet.Add(i);
            }
        }

        void LoadHelix2(TextReader tr, HashSet<int> helixSet) {
            int idx0 = -1;
            int idx1 = -1;
            while (true) {
                string L = tr.ReadLine();
                if (L[0] == '#')
                    break;
                string[] fs = L.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (L.StartsWith("_struct_conf.beg_label_seq_id"))
                    idx0 = int.Parse(fs[1]);
                if (L.StartsWith("_struct_conf.end_label_seq_id"))
                    idx1 = int.Parse(fs[1]) + 1;
            }
            if ((idx0 >= 0) && (idx1 >= 0)) {
                for (int i = idx0; i < idx1; i++)
                    helixSet.Add(i);
            }

        }

        Dictionary<string, string> GetAcc2Chain(TextReader tr) {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            while (true) {
                string L = tr.ReadLine();
                if (L[0] == '#')
                    break;
                if (L[0] == '_')
                    continue;
                string[] fs = L.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if ((fs.Length >= 9) && !dict.ContainsKey(fs[8]))
                    dict[fs[8]] = fs[3];
            }
            return dict;
        }

        string GetPDBTitle(string L, TextReader tr) {
            int idx = L.IndexOf('\'');
            if (idx < 0) {
                L = tr.ReadLine();
                if (L[0] == ';')
                    return L.Substring(1).Trim();
                else if (L[0] == '\'')
                    return L.Trim().Trim('\'');
                else
                    return "";
            }
            string s = L.Substring(idx).Trim().Trim('\'');
            return s;
        }

        public string GetTitle() {
            return this.pdbTitle;
        }

        public List<IBody> GetHeteroChains() {
            return heteroChains;
        }

        public Dictionary<string, string> GetAccession2ChainTable() {
            return acc2chain;
        }

        Dictionary<string, char> P3 = new Dictionary<string, char>()
        {
            {"ALA", 'A' },
            {"ARG", 'R' },
            {"ASN", 'N' },
            {"ASP", 'D' },
            {"CYS", 'C' },
            {"GLU", 'E' },
            {"GLN", 'Q' },
            {"GLY", 'G' },
            {"HIS", 'H' },
            {"ILE", 'I' },
            {"LEU", 'L' },
            {"LYS", 'K' },
            {"MET", 'M' },
            {"PHE", 'F' },
            {"PRO", 'P' },
            {"SER", 'S' },
            {"THR", 'T' },
            {"TRP", 'W' },
            {"TYR", 'Y' },
            {"VAL", 'V' },
            {"UNK", 'x' }
        };

        const int maxChainIndex = 144; // 4x36 of "36 Clusters" 
        List<IBody> LoadAtoms(TextReader tr, HashSet<int> helixSet, HashSet<int> betaSet, List<string> chainNames) {
            Dictionary<string, int> ch2idx = new Dictionary<string, int>() {
                { "HOH", maxChainIndex + 3 },
                { "NAG", maxChainIndex + 11 } };
            List<IBody> bsList = vv.New.BodyList();
            List<IBody> bsList2 = vv.New.BodyList();
            int rsIdxPre = -1;
            var setRNA = new HashSet<string>() { "A", "U", "G", "C" };
            var setDNA = new HashSet<string>() { "DA", "DT", "DG", "DC" };
            char[] fSeparator = new char[] { ' ' };
            char[] dbQuoats = new char[] { '"' };

            //
            //  Read the head section _atom_site.
            //
            int C_ATOM_ID = -1;
            int C_COMP_ID = -1;
            int C_ENTITY_ID = -1;
            int C_SEQ_ID = -1;
            int C_CARTN_X = -1;
            int C_CARTN_Y = -1;
            int C_CARTN_Z = -1;
            int C_ASYM_ID = -1;
            int C_MODEL_NUM = -1;
            int idxF = 1;
            int cntF = -1;
            Dictionary<string, int> colName2Idx = new Dictionary<string, int>();
            string nextLineBuf = null; // temporarily hold the next line.

            while (true) {
                string L = tr.ReadLine();
                if ((L == null) || (L[0] == '#'))
                    break;
                //
                // Process the _atom_site.* lines
                //
                if (L.StartsWith("_atom_site.")) {
                    string cName = L.Substring(L.IndexOf('.') + 1).TrimEnd();
                    colName2Idx[cName] = idxF++;
                } else {
                    nextLineBuf = L;
                    break;
                }
            }

            try {
                C_ATOM_ID = colName2Idx["label_atom_id"];  //3
                C_COMP_ID = colName2Idx["label_comp_id"];  // 5
                C_ENTITY_ID = colName2Idx["label_entity_id"];  // 7
                C_SEQ_ID = colName2Idx["label_seq_id"];  // 8
                C_CARTN_X = colName2Idx["Cartn_x"];  // 10
                C_CARTN_Y = colName2Idx["Cartn_y"];  // 11
                C_CARTN_Z = colName2Idx["Cartn_z"];  // 12
                C_ASYM_ID = colName2Idx["auth_asym_id"]; // 18
                C_MODEL_NUM = colName2Idx["pdbx_PDB_model_num"]; // 20
                cntF = colName2Idx.Count + 1;
            } catch (Exception ex) {
                vv.LastError = "Invalid ATOM sections" + ex.ToString();
                return null;
            }

            float xC4 = 0;  // used to store C3 temporarily for DNA and RNA seq.
            float yC4 = 0;
            float zC4 = 0;

            // Help variable to handle RNA chains which just record one position (the phosphate) to
            // represents a peptide.
            bool singlePepRNA_checked = false;
            bool singlePepRNA = false;
            string curChName = null;  // the chain name of the last processed ATOM line.

            while (true) {
                string L = null;
                if (nextLineBuf == null)
                    L = tr.ReadLine();
                else {
                    L = nextLineBuf;
                    nextLineBuf = null;
                }
                if ((L == null) || (L[0] == '#'))
                    break;

                //
                // Process the ATOM and HETATM lines
                // 
                // Ignore comments withing ATOM rows.
                if (L[0] == '_')
                    continue;

                string[] fs = L.Split(fSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (fs.Length < cntF) { // The row extends to the next line.
                    L = tr.ReadLine();
                    var fs2 = fs.ToList();
                    fs2.AddRange(L.Split(fSeparator, StringSplitOptions.RemoveEmptyEntries));
                    fs = fs2.ToArray();
                    if (fs.Length < cntF)
                        continue;
                }

                string chName = fs[C_ASYM_ID] + "_" + fs[C_MODEL_NUM];
                string atName = fs[C_ATOM_ID].Trim(dbQuoats);
                string rsName = fs[C_COMP_ID];
                int entityId = int.Parse(fs[C_ENTITY_ID]);
                string secType = "x";
                string p1 = "x";
                string bId = null;

                if (fs[0] == "ATOM") {
                    int rsIdx = int.Parse(fs[C_SEQ_ID]) - 1;
                    if (rsIdx == rsIdxPre)
                        continue;
                    if (P3.ContainsKey(rsName) && ((atName == "CA") || (atName == "C2"))) {
                        p1 = P3[rsName].ToString();
                        if (helixSet.Contains(rsIdx))
                            secType = "h";
                        else if (betaSet.Contains(rsIdx))
                            secType = "b";
                        rsName = "";
                    } else if (setRNA.Contains(rsName) && (atName[0] == 'P') ) {
                        if (chName != curChName) { // initialize the tracing variables at the begin of a RNA chain.
                            singlePepRNA_checked = false;
                            singlePepRNA = false;
                            nextLineBuf = null;
                        }
                        // Some PDB files record RNA polymer with a single atom 'P' for one peptide.
                        if (!singlePepRNA_checked) {
                            string nextLine = tr.ReadLine();
                            string[] nextFS = nextLine.Split(fSeparator, StringSplitOptions.RemoveEmptyEntries);
                            int nextSeqId = int.Parse(nextFS[C_SEQ_ID]) - 1;
                            singlePepRNA = (nextSeqId != rsIdx);
                            singlePepRNA_checked = true;
                            nextLineBuf = nextLine;
                        }
                        if (singlePepRNA) {
                            p1 = "r";
                        } else
                            continue;
                    } else if (setDNA.Contains(rsName) || setRNA.Contains(rsName) ) {
                        // For DNA or RNA we pick the middle point of C3' and C4' on the sugar ring to represent the peptide.
                        // Notice that atom C4' comes before C3' in the PDB file, so we first store C4' in temporary 
                        // variables xC4,yC4 and zC4.
                        if (atName.StartsWith("C4'")) {
                            (xC4, yC4, zC4) = ( float.Parse(fs[C_CARTN_X]), float.Parse(fs[C_CARTN_Y]), float.Parse(fs[C_CARTN_Z]) );
                            continue;
                        } else if (atName.StartsWith("C3'")) {
                            if (setDNA.Contains(rsName)) {
                                rsName = rsName[1].ToString();
                                p1 = "d";
                            } else { // for RNA peptide.
                                p1 = "r";
                            }
                        } else
                            continue;
                    } else
                        continue;
                    bId = $"A{rsIdx}.{bsList.Count}";
                    rsIdxPre = rsIdx;
                } else if (fs[0] == "HETATM") {
                    bId = $"H.{fs[C_ATOM_ID]}.{bsList2.Count}";
                    p1 = fs[C_ATOM_ID];
                } else
                    continue;

                IBody b = vv.New.Body(bId);            
                b.SetXYZ(float.Parse(fs[C_CARTN_X]), float.Parse(fs[C_CARTN_Y]), float.Parse(fs[C_CARTN_Z]));

                // For DNA or RNA we pick the middle point of C3' and C4' to represent the peptide.
                if (!singlePepRNA) {
                    if ((p1[0] == 'd') || (p1[0] == 'r'))
                        b.Add(xC4, yC4, zC4).Mult(0.5);
                }

                b.Name = p1 + '.' + rsName + '.' + chName + '.' + secType;
                b.Type = (short)(entityId - 1);

                if (b.Id[0] == 'H') {
                    bsList2.Add(b);
                } else {
                    if ((b.Name[0] == 'r') || (b.Name[0] == 'd'))
                        b.Hidden = true;
                    bsList.Add(b);
                }

                curChName = chName;
            }

            if (chainNames != null) {
                HashSet<string> selectedChains = new HashSet<string>(chainNames);
                bsList = bsList.Where(b => selectedChains.Contains(b.Name.Split('.')[2])).ToList();
                bsList2 = bsList2.Where(b => selectedChains.Contains(b.Name.Split('.')[2])).ToList();
            }

            heteroChains = bsList2;
            return bsList;
        }
    }
}
