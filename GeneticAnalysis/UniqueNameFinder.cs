using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisuMap.GeneticAnalysis {
    public class UniqueNameFinder {
        private HashSet<string> names;
        private int maxIdx = 0;
        private int conflicts = 0;

        public UniqueNameFinder() {
            names = new HashSet<string>();
        }

        public int Count {
            get { return names.Count; }
        }

        public UniqueNameFinder(IEnumerable<string> initList)
            : this() {
            foreach (string name in initList) {
                if (names.Contains(name)) {
                    throw new Exception("Unique Name Error: initial list has duplicate names");
                }
                names.Add(name);
            }
        }

        public bool ContainsName(string name) {
            return names.Contains(name);
        }

        public void RemoveName(string name) {
            if (names.Contains(name)) {
                names.Remove(name);
            }
        }

        public string ChangeName(string oldName, string newName) {
            if (oldName == newName) {
                return oldName;
            }
            if (names.Contains(oldName)) {
                names.Remove(oldName);
            }
            return LookupName(newName);
        }

        public string LookupName(string namePrefix) {
            if (namePrefix == null) {
                namePrefix = "_";
            }

            if (!names.Contains(namePrefix)) {
                names.Add(namePrefix);
                return namePrefix;
            }

            // find upto 5 digital part of the namePrefix.
            int offset = 0;
            for (offset = 1; offset <= 5; offset++) {
                if ((namePrefix.Length - offset) < 0) {
                    break;
                }

                if (!Char.IsDigit(namePrefix[namePrefix.Length - offset])) {
                    break;
                }
            }
            int idx = 1;
            offset--;
            if (offset > 0) {
                idx = int.Parse(namePrefix.Substring(namePrefix.Length - offset)) + 1;
                namePrefix = namePrefix.Substring(0, namePrefix.Length - offset);
            }

            while (true) {
                string newName = namePrefix + idx;
                maxIdx = Math.Max(maxIdx, idx);
                if (!names.Contains(newName)) {
                    names.Add(newName);
                    return newName;
                } else {
                    conflicts++;
                    idx++;
                }
                if (conflicts > 1000) {
                    idx = maxIdx + 1;
                }
            }
        }
    }

}
