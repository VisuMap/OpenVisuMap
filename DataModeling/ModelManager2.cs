using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Packaging;

namespace VisuMap.DataModeling
{
	public partial class ModelManager2: Form {
		public ModelManager2()
		{
			InitializeComponent();
            RefreshList();
            listView.Select();

        }

        void RefreshList() {
            listView.Items.Clear();
            var mdList = DataModeling.modelManager.GetAllModels();
            foreach (DataLink lnk in mdList) {
                var output = lnk.OutputLabel;
                if (output != null ) {
                    if (output.StartsWith("<")) {
                        output = output.Substring(1, lnk.OutputLabel.Length - 2);
                    } else {
                        output = "Var: " + output;
                        if ( output.Length > 30 )
                            output = output.Substring(0, 30) + "...";
                    }
                }
                string[] fields = new string[] {
                    lnk.ModelName,
                    lnk.TrainingDatasetName,
                    lnk.TrainingMapName,
                    output,
                    lnk.TrainingEpochs.ToString(),
                    lnk.LastUpdate.ToShortDateString()};
                listView.Items.Add(new ListViewItem(fields));
            }
            this.Text = "Working Directory: " + DataModeling.workDir.Substring(0, DataModeling.workDir.Length - 1);
        }

        private void btnClose_Click(object sender, EventArgs e) {
            this.Close();
        }

        void DeleteModelFiles(string mdName) {
            foreach (var f in Directory.EnumerateFiles(DataModeling.workDir, mdName + ".*")) {
                if (f.EndsWith(".chk") 
                    || f.EndsWith(".data-00000-of-00001") 
                    || f.EndsWith(".index") 
                    || f.EndsWith(".meta") 
                    || f.EndsWith(".md")
                    || f.EndsWith(".h5") // keras model file.
                    || f.EndsWith(".aug") // learned argument file.
                ) File.Delete(f);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e) {
            foreach (ListViewItem item in listView.SelectedItems) {
                DeleteModelFiles(item.SubItems[0].Text);
            }
            RefreshList();
        }

        const string zipFilter = "Zip files (*.zip)|*.zip";

        private void btnImport_Click(object sender, EventArgs e) {
            string pktFile;
            using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                openFileDialog.Filter = zipFilter;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() != DialogResult.OK) {
                    return;
                }
                pktFile = openFileDialog.FileName;
            }

            bool overwrite = false;

            using (Package package = ZipPackage.Open(pktFile, FileMode.Open, FileAccess.Read)) {
                foreach (PackagePart part in package.GetParts()) {
                    var target = Path.GetFullPath(Path.Combine(DataModeling.workDir, part.Uri.OriginalString.TrimStart('/')));
                    target = target.Replace("%20", " ");
                    var targetDir = target.Remove(target.LastIndexOf('\\'));
                    using (Stream source = part.GetStream(FileMode.Open, FileAccess.Read)) {
                        if (File.Exists(target) && !overwrite) {
                            string mdName = Path.GetFileNameWithoutExtension(target);
                            if (MessageBox.Show("Model " + mdName 
                                + " exists in current context!\nOverwrite all existing models?", "Importing Models",
                                MessageBoxButtons.YesNo)
                                == System.Windows.Forms.DialogResult.No) {
                                    break;
                            }
                            overwrite = true;
                        }
                        FileStream targetFile = File.OpenWrite(target);
                        source.CopyTo(targetFile);
                        targetFile.Close();
                    }
                }
            }
            RefreshList();
        }

        private void btnExport_Click(object sender, EventArgs e) {
            string pkgFile;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog()) {
                saveFileDialog.Filter = zipFilter;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() != DialogResult.OK) {
                    return;
                }
                pkgFile = saveFileDialog.FileName;
            }

            List<string> fileList = new List<string>();
            foreach (ListViewItem item in listView.SelectedItems) {
                string mdName = item.SubItems[0].Text;
                foreach (var f in System.IO.Directory.EnumerateFiles(DataModeling.workDir, mdName + ".*")) {
                    fileList.Add(f);
                }
            }

            if (fileList.Count == 0) {
                MessageBox.Show("No model selected!");
                return;
            }

            using (Package zip = System.IO.Packaging.Package.Open(pkgFile, FileMode.Create)) {
                foreach (string fileToAdd in fileList) {
                    string destFilename = ".\\" + Path.GetFileName(fileToAdd);
                    Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                    if (zip.PartExists(uri)) {
                        zip.DeletePart(uri);
                    }
                    PackagePart part = zip.CreatePart(uri, "", CompressionOption.Normal);
                    using (FileStream fileStream = new FileStream(fileToAdd, FileMode.Open, FileAccess.Read)) {
                        using (Stream dest = part.GetStream()) {
                            CopyStream(fileStream, dest);
                        }
                    }
                }
            }
        }

        private const long BUFFER_SIZE = 4096;

        private static void CopyStream(System.IO.FileStream inputStream, System.IO.Stream outputStream) {
            long bufferSize = inputStream.Length < BUFFER_SIZE ? inputStream.Length : BUFFER_SIZE;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            long bytesWritten = 0;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0) {
                outputStream.Write(buffer, 0, bytesRead);
                bytesWritten += bytesRead;
            }
        }

        private void btnRename_Click(object sender, EventArgs e) {
            if ( listView.SelectedItems.Count == 0) {
                MessageBox.Show("Please select a model to change its name.");
                return;
            }

            string mdName = listView.SelectedItems[0].SubItems[0].Text;

            string newName = PromptInput.PromptString("Rename Model", "Please enter a new model name:", mdName, 200);
            if (newName != null) {
                foreach (var f in Directory.EnumerateFiles(DataModeling.workDir, mdName + ".*"))
                    File.Move(f, f.Replace(mdName, newName));

                string chkFile = null;
                string chkFilePath = DataModeling.workDir + newName + ".chk";
                using (var sr = new StreamReader(chkFilePath))
                    chkFile = sr.ReadToEnd();
                using (var sw = new StreamWriter(chkFilePath))
                    sw.Write(chkFile.Replace(mdName, newName));

                RefreshList();
            }
        }

        int sortColumn = -1;
        private void listView_ColumnClick(object sender, ColumnClickEventArgs e) {
            if (e.Column != sortColumn) {
                sortColumn = e.Column;
                listView.Sorting = SortOrder.Ascending;
            } else {
                if (listView.Sorting == SortOrder.Ascending)
                    listView.Sorting = SortOrder.Descending;
                else
                    listView.Sorting = SortOrder.Ascending;
            }

            listView.Sort();
            listView.ListViewItemSorter = new ListViewItemComparer(e.Column, listView.Sorting);
        }

        class ListViewItemComparer : IComparer {
            int col;
            SortOrder order;
            public ListViewItemComparer() {
                col = 0;
                order = SortOrder.Ascending;
            }
            public ListViewItemComparer(int column, SortOrder order) {
                col = column;
                this.order = order;
            }
            public int Compare(object x, object y) {
                int returnVal = -1;
                returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
                                ((ListViewItem)y).SubItems[col].Text);
                if (order == SortOrder.Descending)
                    returnVal *= -1;
                return returnVal;
            }
        }

        private void listView_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == 1) {
                foreach (var item in listView.Items)
                    (item as ListViewItem).Selected = true;
            }
        }

        public List<string> ModelList() {
            List<string> mdList = new List<string>();
            
            foreach(var item in listView.Items) {
                mdList.Add((item as ListViewItem).SubItems[0].Text);
            }
            return mdList;
        }

        public void DeleteModel(string modelName) {
            DeleteModelFiles(modelName);
            RefreshList();
        }

        private void btnSetWorkDir_Click(object sender, EventArgs e) {
            using (var fb = new FolderBrowserDialog()) {
                fb.ShowNewFolderButton = false;
                fb.SelectedPath = DataModeling.workDir;
                fb.Description = "Selecting Working Directory";
                var ret = fb.ShowDialog();
                if ((ret == DialogResult.OK) && !string.IsNullOrWhiteSpace(fb.SelectedPath)) {
                    DataModeling.workDir = fb.SelectedPath + "\\";
                    DataModeling.App.ScriptApp.SetProperty("DataModeling.WorkDir", DataModeling.workDir, null);
                    RefreshList();
                }
            }
        }
    }
}
