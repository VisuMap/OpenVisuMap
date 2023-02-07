using System.Collections.Generic;
using VisuMap.Script;

namespace ClipRecorder {
    public class FrameSpec {
        public FrameSpec(float width, float height, float depth, short mapType, int size) {
            BodyInfoList = new BodyInfo[size];
            MapWidth = width;
            MapHeight = height;
            MapDepth = depth;
            MapType = mapType;
        }

        public int Size { get => BodyInfoList.Length; }
        public float MapWidth;
        public float MapHeight;
        public float MapDepth;
        public float Timestamp;
        public BodyInfo[] BodyInfoList;
        public short MapType;

        public List<IBody> GetBodyList() {
            var app = ClipRecorder.App.ScriptApp;
            var bsList = app.New.BodyListClone(app.Dataset.BodyListEnabled());
            for(int i=0; i<Size; i++) {
                bsList[i].X = BodyInfoList[i].x;
                bsList[i].Y = BodyInfoList[i].y;
                bsList[i].Z = BodyInfoList[i].z;
                bsList[i].Type = BodyInfoList[i].type;
                RecorderForm.SetFlags(bsList[i], BodyInfoList[i].flags);
            }
            return bsList;
        }
    }
}
