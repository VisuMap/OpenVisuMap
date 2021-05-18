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
using System.Windows.Forms;
using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.WaveTransforms {
    [PluginMain]
    public class WaveTransforms : IPlugin {
        public static IApplication App;
        public virtual void Initialize(IApplication app) {
            App = app;
            ToolStripMenuItem helpMenu = App.MainForm.MainMenuStrip.Items.Find(
                "helpToolStripMenuItem", false)[0] as ToolStripMenuItem;
            if (helpMenu != null) {
                helpMenu.DropDownItems.Add("About WaveTransforms...", null, new EventHandler(OpenHelpDocument));
            }
            App.InstallPluginObject(new TransformsScript());
        }

        void OpenHelpDocument(object sender, EventArgs e) {
            HelpDocument.OpenResource("WaveTransforms.WaveTransforms.rtf", "Wave Transforms Plugin");
        }

        public virtual void Dispose() { }
        public virtual string Name { get { return "WaveTransforms"; } }
    }
}
