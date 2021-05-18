/// <copyright from="2004" to="2008" company="VisuMap Technologies Inc.">
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

//
// Description: This project implements some metric for numeric data.
//
// Installation: This project produces a DLL module that is installed into
// VisuMap application through the Organizer (opened through main menu
// View>Organizer) through the context menu of the Plugin Modules node.
//

namespace BinaryPatternMetrics {
    /// <summary>
    /// Implementation of a collections of metrics for binary data.
    /// </summary>
    [PluginMain]
    public class BinaryPatternMetrics : IPlugin {

        /// <summary>
        /// Ininitalize the plugin module.
        /// </summary>
        /// <param name="app">The object representing the running VisuMap application.</param>
        public void Initialize(IApplication app) {
            // Install all metrics implemented in this plugin.
            app.InstallMetric(new JaccardDistance());
            app.InstallMetric(new DiceDistance());
            app.InstallMetric(new TanimotoDistance());
            app.InstallMetric(new BinaryHamming());
            app.InstallMetric(new SokalSneath());
            app.InstallMetric(new RussellRao());
            app.InstallMetric(new YuleDistance());
        }

        /// <summary>
        /// Dispose the plugin object.
        /// </summary>
        public void Dispose() {}

        /// <summary>
        /// Gets the name of the plugin module.
        /// </summary>
        public string Name { get { return "Binary Pattern Metrics"; } }
    }
}
