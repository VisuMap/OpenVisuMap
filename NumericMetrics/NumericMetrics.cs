/// <copyright from="2004" to="2014" company="VisuMap Technologies Inc.">
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

namespace VisuMap.NumericMetrics {
    [PluginMain]
    public class NumericMetrics : IPlugin {
        public static IApplication App;
        public static ConfigureSettings Cfg;

        /// <summary>
        /// Ininitalize the plugin module.
        /// </summary>
        /// <param name="app">The object representing the running VisuMap application.</param>
        public void Initialize(IApplication app) {
            // Setup some global objects for this module.
            if (app.ScriptApp.ApplicationBuild < 860) {
                MessageBox.Show("Cannot initialize Numeric Metrics plugin: VisuMap 3.2.860 or higher required",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            NumericMetrics.App = app;
            Cfg = new ConfigureSettings();

            app.InstallMetric(new HarmonicMean());
            app.InstallMetric(new CityBlock());
            app.InstallMetric(new LogEuclidean());
            app.InstallMetric(new GeodeticDistance());
            app.InstallMetric(new SquareRoot());
            app.InstallMetric(new Covariance());
            app.InstallMetric(new ProductAffinity());
            app.InstallMetric(new EuclideanAffinity());
            app.InstallMetric(new GravitationalAffinity());
        }

        /// <summary>
        /// Dispose the plugin object.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Gets the name of the plugin module.
        /// </summary>
        public string Name { get { return "Numeric Metrics"; } }
    }
}
