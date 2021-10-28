using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
