using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class SeqBlob : IDisposable {
        IBlob blob;
        byte buf = 0;    // the byte buffer.
        int bufIdx = 0;  // a value between 0 and 3
        public const int UKNOWN_LETTER = 4;

        public SeqBlob(string blobName) {
            var app = GeneticAnalysis.App.ScriptApp;
            bool externStorage = app.GetProperty("GeneticAnalysis.ExternStorage", "1").Equals("1");
            if (externStorage) {
                string dataFile = app.Folder.FolderFileName;
                if (string.IsNullOrEmpty(dataFile)) {
                    throw new Exception("Please save the current dataset into a file before importing new data.");
                }
                FileInfo info = new FileInfo(dataFile);
                string blobDir = info.DirectoryName + "\\BlobData";
                if (!Directory.Exists(blobDir)) {
                    Directory.CreateDirectory(blobDir);
                }

                string blobFileName = "$\\BlobData\\" + blobName + ".seq";
                blob = app.Folder.CreateFileBlob(blobName, blobFileName);
            } else {
                blob = app.Folder.CreateBlob(blobName);
            }
        }

        /// <summary>
        /// returns the number of letters in current blob (including the letters in buf).
        /// </summary>
        public long Length {
            get { return blob.Stream.Length * 2 + bufIdx; }
        }

        public void AddLetter(int k) {
            buf |= (byte)(k << (bufIdx * 4));
            bufIdx++;
            if (bufIdx == 2) {
                blob.Stream.WriteByte(buf);
                bufIdx = 0;
                buf = 0;
            }
        }

        public void Add(string seq) {
            foreach (char c in seq) {
                var k = FastaNt.ACGT.IndexOf(c);
                AddLetter((k >= 0) ? k : UKNOWN_LETTER);            
            }
        }

        public void Close() {
            Dispose();
        }

        public void Dispose() {
            blob.ContentType = "SeqACGT/Length " + Length.ToString();
            if (bufIdx > 0) {
                blob.Stream.WriteByte(buf);
            }
            blob.Close();
        }

        public string Name {
            get { return blob.Name; }
        }
    }
}
