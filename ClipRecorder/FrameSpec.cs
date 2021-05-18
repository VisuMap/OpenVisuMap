using System;
using System.Collections.Generic;
using System.Text;

namespace ClipRecorder {
    public class FrameSpec {
        public FrameSpec(float width, float height, float depth, short mapType, int size) {
            BodyInfoList = new BodyInfo[size];
            for (int i = 0; i < size; i++) BodyInfoList[i] = new BodyInfo();
            MapWidth = width;
            MapHeight = height;
            MapDepth = depth;
            MapType = mapType;
        }
        public float MapWidth;
        public float MapHeight;
        public float MapDepth;
        public short MapType;
        public BodyInfo[] BodyInfoList;
    }
}
