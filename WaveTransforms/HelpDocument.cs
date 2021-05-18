/// <copyright from="2004" to="2010" company="VisuMap Technologies Inc.">
///   Copyright (C) VisuMap Technologies Inc.
/// 
///   Permission to use, copy, modify, distribute and sell this 
///   software and its documentation for any purpose is hereby 
///   granted without fee, provided that the above copyright notice 
///   appear in all copies and that both that copyright notice and 
///   this permission notice appear in supporting documentation. 
///   VisuMap Technologies Company makes no representations about the 
///   suitability of this software for any purpose. It is provided 
///   "as is" without explicit or implied warranty. 
/// </copyright>
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace VisuMap.WaveTransforms {
    public partial class HelpDocument : Form {

        static public void OpenResource(string resourcePath, string windowTitle) {
            HelpDocument doc = new HelpDocument();
            doc.Text = windowTitle;
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream stm = asm.GetManifestResourceStream(resourcePath);            
            doc.rtbMain.LoadFile(stm, RichTextBoxStreamType.RichText);
            stm.Close();

            doc.Show();
        }
        public HelpDocument() {
            InitializeComponent();
        }
    }
}