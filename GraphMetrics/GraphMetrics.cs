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
using VisuMap.Plugin;
using System.Windows.Forms;

namespace VisuMap.GraphMetrics {
    [PluginMain]
    public class GraphMetrics : IPlugin {
        public static IApplication App;

        /// <summary>
        /// Ininitalize the plugin module.
        /// </summary>
        /// <param name="app">The object representing the running VisuMap application.</param>
        public void Initialize(IApplication app) {
            if (app.ScriptApp.ApplicationBuild < 847) {
                MessageBox.Show("Cannot initialize graph metri plugin: VisuMap 3.0.847 or higher required", 
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            // Setup some global objects for this module.
            GraphMetrics.App = app;
            app.InstallMetric(new ResistanceMetric());
            app.InstallMetric(new MinimalPathMetric());
        }

        /// <summary>
        /// Dispose the plugin object.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Gets the name of the plugin module.
        /// </summary>
        public string Name { get { return "Graph Metrics"; } }

    }
}
