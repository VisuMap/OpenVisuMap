using System;
using System.Windows.Forms;
using VisuMap.Plugin;
using VisuMap.Script;

namespace CustomGlyphSets {
    [PluginMain]
    public class CustomGlyphSet : IPlugin {
        public static IApplication App;

        public virtual void Initialize(IApplication app) {
            App = app;

            ToolStripMenuItem miPlugin = App.GetPluginMenu();
            miPlugin.DropDownItems.Add("Make Glyph Sets", null, MakeGlyphSets);
            app.InstallPluginObject(new GlyphSetScript());
        }

        void MakeGlyphSets(object sender, EventArgs e) {
            string pluginHome = App.ScriptApp.ApplicationData + "\\plugins\\Custom Glyphs\\";
            App.ScriptApp.New.ScriptEditor(pluginHome + "MakeGlyphSets.js").Show();
        }


        public virtual void Dispose() { }
        public virtual string Name { get { return "CustomGlyphSets"; } }
    }
}
