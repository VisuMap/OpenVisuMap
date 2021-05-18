using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ClipRecorder {
    public partial class AboutClipRecord : Form {
        public AboutClipRecord() {
            InitializeComponent();
            LoadRichText(global::ClipRecorder.Properties.Resources.AboutClipRecorder);
        }

        void LoadRichText(string txt) {
            byte[] bytes = new byte[txt.Length];
            Encoding.UTF8.GetBytes(txt, 0, txt.Length, bytes, 0);
            MemoryStream ms = new MemoryStream(bytes);
            this.richTextBox1.LoadFile(ms, RichTextBoxStreamType.RichText);
        }
    }
}