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
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.IO;
using VisuMap.Lib;

namespace VisuMap.NumericMetrics {
    public class ConfigureSettings {
        const string moduleName = "NumericMetrics";
        const string moduleNamespace = "VisuMap.NumMetrics";
        PropertyManager propMan;

        public ConfigureSettings() {
            propMan = new PropertyManager(this, "Settings", moduleNamespace);
            LoadProperties();
        }

        public void LoadProperties() {
            bool saveBack = false;
            XmlElement cfgNode = NumericMetrics.App.GetPluginDataNode(0, moduleName, moduleNamespace, false);
            if (cfgNode == null) {
                cfgNode = NumericMetrics.App.GetPluginDataNode(0, moduleName, moduleNamespace, true);
                saveBack = true;
            }
            propMan.LoadProperties(cfgNode);
            if (saveBack) {
                propMan.SaveProperties(cfgNode);
            }
        }

        /*
        string testName="Hi there!";
        [Saved]
        public string TestName  {
            get { return testName; }
            set { testName = value; }
        }
        */
    }
}
