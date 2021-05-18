using System;
using VisuMap.Plugin;
using VisuMap.Script;

namespace ClipRecorder {
    public class ScriptApp : MarshalByRefObject, IPluginObject {
        public RecorderForm NewRecorder() {
            ClipRecorder.CurrentRecorder = new RecorderForm(ClipRecorder.App);
            return ClipRecorder.CurrentRecorder;
        }

        public RecorderForm OpenRecorder(IForm playTarget = null) {
            if( (ClipRecorder.CurrentRecorder == null) || ClipRecorder.CurrentRecorder.IsDisposed) {
                ClipRecorder.CurrentRecorder = new RecorderForm(ClipRecorder.App);
                ClipRecorder.CurrentRecorder.Show();                
            }
            ClipRecorder.CurrentRecorder.PlayTarget = playTarget;
            return ClipRecorder.CurrentRecorder;
        }

        public string Name { 
            get { return "ClipRecorder"; }
            set { }
        }

        public RecorderForm CurrentRecorder
        {
            get => ClipRecorder.CurrentRecorder;
        }

    }
}
