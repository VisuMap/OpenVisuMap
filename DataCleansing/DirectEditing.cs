using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace VisuMap.DataCleansing {
    public partial class DirectEditing : Form {
        char visibleSeparator = '|';
        bool sepRelaced;

        public DirectEditing() {
            InitializeComponent();
            miWrapLines.Checked = dataPanel.WordWrap;
            dataPanel.Text = ReadClipBoardText();
            dataPanel.Focus();
            dataPanel.AllowDrop = true;
            dataPanel.DragEnter += new DragEventHandler(dataPanel_DragEnter);
            dataPanel.DragDrop += new DragEventHandler(dataPanel_DragDrop);
            dataPanel.Select();
        }


        private string ReadClipBoardText() {
            TextReader tr = null;
            IDataObject data = Clipboard.GetDataObject();

            //
            // Try unicode csv format. This for instance the case when we copy
            // and paste tables with chinese content from Exel.
            //
            if (data.GetDataPresent(DataFormats.UnicodeText)) {
                string strData = data.GetData(DataFormats.UnicodeText).ToString();
                if (!string.IsNullOrEmpty(strData)) {
                    tr = new StringReader(data.GetData(DataFormats.UnicodeText).ToString());
                }
            } else if (data.GetDataPresent(DataFormats.Text)) {
                string strData = data.GetData(DataFormats.Text).ToString();
                if (!string.IsNullOrEmpty(strData)) {
                    tr = new StringReader(data.GetData(DataFormats.Text).ToString());
                }
            }

            if (tr == null) {
                return "";
            }

            return tr.ReadToEnd();
        }

        private void miTrim_Click(object sender, EventArgs e) {
            StringBuilder sb = new StringBuilder();
            StringReader sr = new StringReader(dataPanel.Text);
            while(true) {
                string line = sr.ReadLine();
                if (line == null) {
                    break;
                }
                line = line.TrimEnd();
                if (sepRelaced) {
                    line = line.TrimEnd(visibleSeparator);
                }
                if (line.Length > 0) {
                    sb.AppendLine(line);
                }
            } 
            dataPanel.Text = sb.ToString();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e) {
            dataPanel.Undo();
        }

        private void cutToolStripMenuItem2_Click(object sender, EventArgs e) {
            dataPanel.Cut();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e) {
            dataPanel.Copy();
        }

        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e) {
            string pasteText = ReadClipBoardText();
            object originalClbData = null;
            if (!string.IsNullOrEmpty(pasteText)) {
                originalClbData = Clipboard.GetDataObject();
                Clipboard.SetData(DataFormats.UnicodeText, pasteText);
            }
            
            dataPanel.Paste();

            if (originalClbData != null) {
                Clipboard.SetDataObject(originalClbData);
            }
        }

        public const uint EM_LINEINDEX = 0xbb;
        public const uint EM_LINEFROMCHAR = 0x00C9;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr handleToWindow, uint message, UIntPtr wParam, IntPtr lParam);


        private void cutLineToolStripMenuItem_Click(object sender, EventArgs e) {
            uint lineNr = (uint)SendMessage(
                dataPanel.Handle,
                EM_LINEFROMCHAR,
                new UIntPtr((uint)dataPanel.SelectionStart),
                new IntPtr(0)).ToInt32();
            if (lineNr < 0) {
                return;
            }
            int posBegin = SendMessage(
                dataPanel.Handle,
                EM_LINEINDEX,
                new UIntPtr(lineNr),
                new IntPtr(0)).ToInt32();
            int posEnd = SendMessage(
                dataPanel.Handle,
                EM_LINEINDEX,
                new UIntPtr(lineNr + 1),
                new IntPtr(0)).ToInt32();

            if (posEnd < 0) {
                posBegin--;  //
                if (posBegin < 0) {
                    posBegin = 0;
                }
                posEnd = dataPanel.Text.Length;
            }
            try {
                dataPanel.SelectionStart = posBegin;
                dataPanel.SelectionLength = posEnd - posBegin;
                dataPanel.Cut();
            } catch (ArgumentException) { }
        }

        RichTextBoxFinder textFinder;
        private void miFind_Click(object sender, System.EventArgs e) {
            if (textFinder != null) {
                textFinder.Dispose();
            }
            textFinder = new RichTextBoxFinder(dataPanel);
            textFinder.Show();
        }

        private void miFindNext_Click(object sender, System.EventArgs e) {
            if (textFinder == null) {
                miFind_Click(null, null);
            }
            textFinder.FindNext();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            FileDialog openFileDialog = (FileDialog)new OpenFileDialog();

            openFileDialog.Filter = "CSV files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != DialogResult.OK) {
                openFileDialog.Dispose();
                return;
            }

            LoadFile(openFileDialog.FileName);
        }

        void LoadFile(string fileName) {
            using (StreamReader sr = new StreamReader(fileName)) {
                dataPanel.Text = sr.ReadToEnd();
            }
        }

        private void miWrapLines_Click(object sender, EventArgs e) {
            dataPanel.WordWrap = ! dataPanel.WordWrap;
            miWrapLines.Checked = dataPanel.WordWrap;
        }

        private void miImport_Click(object sender, EventArgs e) {
            string data = dataPanel.SelectedText;
            if (string.IsNullOrEmpty(data)) {
                data = dataPanel.Text;
            }
            Clipboard.SetData(DataFormats.UnicodeText, data);
            DataCleansing.App.MainForm.Activate();
            DataCleansing.App.MainForm.Select();
            //SendKeys.Send("^V");
            SendKeys.Send("%fip");
        }

        private void miLargeText_Click(object sender, EventArgs e) {
            dataPanel.ZoomFactor = 1.5f;
            miLargeText.Checked = true;
            miNormalText.Checked = false;
            miSmallText.Checked = false;
        }

        private void miNormalText_Click(object sender, EventArgs e) {
            dataPanel.ZoomFactor = 1.0f;
            miLargeText.Checked = false;
            miNormalText.Checked = true;
            miSmallText.Checked = false;
        }

        private void miSmallText_Click(object sender, EventArgs e) {
            dataPanel.ZoomFactor = 0.75f;
            miLargeText.Checked = false;
            miNormalText.Checked = false;
            miSmallText.Checked = true;
        }

        //This method enables undo operation, but is urgly.
        void RepalceText(string newText) {
            object originalClbData = Clipboard.GetDataObject();
            Clipboard.SetText(newText);
            dataPanel.SelectAll();
            dataPanel.Paste();
            if (originalClbData != null) Clipboard.SetDataObject(originalClbData);
        }

        private void miShowSeparator_Click(object sender, EventArgs e) {
            dataPanel.Text = dataPanel.Text.Replace('\t', visibleSeparator);
            sepRelaced = true;
        }

        private void miReplace_Click(object sender, EventArgs e) {
            RepalceForm rf = new RepalceForm();
            if (rf.ShowDialog() == DialogResult.OK) {
                dataPanel.Text = dataPanel.Text.Replace(rf.FindText, rf.ReplaceText);
            }
        }

        void dataPanel_DragDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (fileNames.Length > 0) {
                    LoadFile(fileNames[0]);
                }
            }
        }

        void dataPanel_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (fileNames.Length > 0) {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }
    }
}