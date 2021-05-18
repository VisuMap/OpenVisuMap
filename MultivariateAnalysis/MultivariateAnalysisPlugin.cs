/// <copyright from="2004" to="2011" company="VisuMap Technologies Inc.">
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
using System.Reflection;
using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.MultivariateAnalysis {
    /// <summary>
    /// This plugin main class for the MultivariateAnalysis plugin.
    /// </summary>
    [PluginMain]
    public class MultivariateAnalysisPlugin : IPlugin {
        /// <summary>
        /// A reference to the VisuMap application.
        /// </summary>
        public static IApplication App;

        /// <summary>
        /// Intiializes the plugin.
        /// </summary>
        /// <param name="app">A reference to the VisuMap application.</param>
        /// <remarks>The method is called once when the plugin is loaded by VisuMap application.</remarks>
        public virtual void Initialize(IApplication app) {
            if (app.ScriptApp.ApplicationBuild < 926) {
                MessageBox.Show("Cannot initialize Multivariate Analysis plugin: VisuMap 5.0.926 or higher required",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            App = app;
            ToolStripMenuItem helpMenu = App.MainForm.MainMenuStrip.Items.Find(
                "helpToolStripMenuItem", false)[0] as ToolStripMenuItem;
            if (helpMenu != null) {
                helpMenu.DropDownItems.Add("MVA Plugin...", null, new EventHandler(OpenHelpDocument));
            }

            app.InstallPluginObject( new Mva() );
        }

        /// <summary>
        /// Disposes the plugin object.
        /// </summary>
        public virtual void Dispose() { }

        /// <summary>
        /// Gets or sets the name of this plugin.
        /// </summary>
        public virtual string Name { get { return "MultivariateAnalysisPlugin"; } }

        void OpenHelpDocument(object sender, EventArgs e) {
            string assemlyDir = Assembly.GetExecutingAssembly().Location;
            assemlyDir = assemlyDir.Substring(0, assemlyDir.LastIndexOf('\\'));
            String helpFile = assemlyDir + "\\MultivariateAnalysis.chm";
            try {
                System.Diagnostics.Process.Start(helpFile);
            } catch (Exception ex) {
                MessageBox.Show("Cannot open the help file: \"" + helpFile + "\": " + ex.Message);
            }
        }
    }
}
