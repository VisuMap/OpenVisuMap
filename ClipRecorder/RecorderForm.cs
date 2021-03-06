using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Threading;

using VisuMap.Lib;
using VisuMap.Plugin;
using VisuMap.Script;


namespace ClipRecorder {
    public partial class RecorderForm : Form, IForm {
        static IApplication app;
        List<FrameSpec> frameList;
        int currentFrame = 0;  // The first frame. 
        bool isRecording = false;
        bool isPlaying = false;
        bool stopFlag;
        int lastX;
        int lastY;
        int interframePause = 0;
        int replayInterval = 1;
        bool autoReverse = true;
        VisuMap.Lib.PropertyManager propMan;
        ProgressBar progressBar;
        bool trackingTrend;
        string scriptPath;
        string clipFilePath = "vmb:ClipRecorder.clip";
        IForm playTarget = null;  // Play to recordered clips into a different window instead of the main window.

        public RecorderForm(IApplication app) {
            InitializeComponent();
            RecorderForm.app = app;
            progressBar = new ProgressBar(this.progressPanel);
            frameList = new List<FrameSpec>();
            app.BodyMoved += new EventHandler<EventArgs>(app_BodyMoved);
            app.MapConfigured += new EventHandler<EventArgs>(app_BodyMoved);
            this.MouseWheel += new MouseEventHandler(RecorderForm_MouseWheel);
            propMan = new VisuMap.Lib.PropertyManager(this, "Settings", "ClipRecorderNs");
            propMan.LoadProperties(PluginRoot);            
            SyncMode();
        }

        void RecorderForm_MouseWheel(object sender, MouseEventArgs e) {
            PlaySteps( -e.Delta / 120 );

        }

        void PlaySteps(int steps) { 
            bool needRedraw = false;
            if (steps > 0) {
                if (currentFrame < (frameList.Count - 1)) {
                    SetCurrentValue(Math.Min(currentFrame + steps, frameList.Count - 1));
                    needRedraw = true;
                }
            } else {
                if (currentFrame > 0) {
                    SetCurrentValue(Math.Max(currentFrame + steps, 0));
                    needRedraw = true;
                }
            }

            if (needRedraw) {
                ShowFrame(GetBodyList(), frameList[currentFrame]);
            }
        }

        public IForm PlayTarget
        {
            get => playTarget;
            set {
                playTarget = value;
                if ( playTarget != null) {
                    playTarget.TheForm.Disposed += (s,e)=> { playTarget = null; };
                }
            }
        }

        XmlElement PluginRoot {
            get { return app.GetPluginDataNode(0, "ClipRecorder", propMan.NameSpace, true); }
        }

        void btnClose_Click(object sender, EventArgs e) {
            stopFlag = true;
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e) {
            propMan.SaveProperties(PluginRoot);
            progressBar.Dispose();
            base.OnClosing(e);
        }

        void app_BodyMoved(object sender, EventArgs e) {
            if (!isRecording) {
                return;
            }

            if (sender.Equals(this)) {
                // The event is triggered by ReplaySnapshot() of this object.
                return;
            }

            CreateSnapshot();
        }


        void CalculateTrendType(FrameSpec b0, FrameSpec b1) {
            if ( b0 == null ) {
                foreach(BodyInfo b in b1.BodyInfoList) {
                    b.type = 0;
                } 
            } else {
                BodyInfo[] bList0 = b0.BodyInfoList;
                BodyInfo[] bList1 = b1.BodyInfoList;
                double[] sizes = new double[bList0.Length];
                double maxSize = double.MinValue;
                for(int i=0; i<bList1.Length; i++) {
                    float dx = bList1[i].x - bList0[i].x;
                    float dy = bList1[i].y - bList0[i].y;
                    sizes[i] = Math.Sqrt(dx * dx + dy * dy);
                    maxSize = Math.Max(sizes[i], maxSize);
                    double angle = Math.Atan2(dy, dx);
                    if (angle < 0) angle += 2*Math.PI; 
                    bList1[i].type = (short)(1 + 32 * angle / (2 * Math.PI));
                }

                for (int i = 0; i < bList1.Length; i++) {
                    bList1[i].type += (short)(32 * (int)(sizes[i]/maxSize*3));
                }
            }
        }

        ushort Flags(IBody body) {
            ushort flags = 0;
            if (body.IsFixed) flags |= 0x0001;
            if (body.Disabled) flags |= 0x0002;
            if (body.Hidden) flags |= 0x0004;
            if (body.Highlighted) flags |= 0x0008;
            if (body.ShowId) flags |= 0x0010;
            if (body.ShowName) flags |= 0x0020;
            return flags;
        }

        void SetFlags(IBody body, ushort flags) {
            body.IsFixed = ((flags & 0x0001) != 0);
            body.Disabled = ((flags & 0x0002) != 0);
            body.Hidden = ((flags & 0x0004) != 0);
            body.Highlighted = ((flags & 0x0008) != 0);
            body.ShowId = ((flags & 0x0010) != 0);
            body.ShowName = ((flags & 0x0020) != 0);
        }


        public int CreateSnapshot() {
            IMap map = app.ScriptApp.Map;
            List<IBody> bodyList = map.BodyList as List<IBody>;
            FrameSpec frame = new FrameSpec((short)map.Width, (short)map.Height, (short)map.Depth,
                (short)map.MapTypeIndex, bodyList.Count);

            for (int i = 0; i < bodyList.Count; i++) {
                BodyInfo b = frame.BodyInfoList[i];
                IBody body = bodyList[i];
                b.x = (float)(body.X);
                b.y = (float)(body.Y);
                b.z = (float)(body.Z);
                b.type = body.Type;
                b.flags = Flags(body);
            }

            if (trackingTrend) {
                FrameSpec preFrame = (frameList.Count == 0) ? null : frameList[frameList.Count - 1];
                CalculateTrendType(preFrame, frame);
            }

            frameList.Add(frame);
            SetMaximum(frameList.Count);
            return frameList.Count;
        }

        public int AddSnapshot(IList<IBody> bodyList) {
            FrameSpec frame = new FrameSpec(0, 0, 0, 0, bodyList.Count);
            for (int i = 0; i < bodyList.Count; i++) {
                BodyInfo b = frame.BodyInfoList[i];
                IBody body = bodyList[i];
                b.x = (float)(body.X);
                b.y = (float)(body.Y);
                b.z = (float)(body.Z);
                b.type = body.Type;
                b.flags = Flags(body);
            }
            frameList.Add(frame);
            SetMaximum(frameList.Count);
            return frameList.Count;
        }

        List<IBody> GetBodyList() {
            if (playTarget == null) {
                return app.ScriptApp.Dataset.BodyList as List<IBody>;
            } else {
                if (playTarget is IMapSnapshot) {
                    return (playTarget as IMapSnapshot).BodyList as List<IBody>;
                } else if (playTarget is IMap3DView) {
                    return (playTarget as IMap3DView).GetBodyList() as List<IBody>;
                } else {
                    return null;
                }
            }
        }

        void RedrawTarget() {
            if (playTarget is IMapSnapshot)
                (playTarget as IMapSnapshot).RedrawAll();
            else if (playTarget is IMap3DView) {
                (playTarget as IMap3DView).RedrawAll();
            }
        }

        public bool ShowFrame(int frameIndex) {
            if ((frameIndex < 0) || (frameIndex >= frameList.Count)) return false;

            SetCurrentValue(frameIndex);
            Application.DoEvents();
            ShowFrame(GetBodyList(), frameList[frameIndex]);

            return true;
        }

        void btnPlayStop_Click(object sender, EventArgs e) {
            isRecording = false;
            isPlaying = ! isPlaying;
            SyncMode();

            if (isPlaying) {
                ReplayFrameList();
            } else {
                StopPlaying();
            }
        }

        void StopPlaying() {
            stopFlag = true;
        }

        public void Play() {
            isRecording = false;
            isPlaying = true;            
            ReplayFrameList();
        }

        public bool Transform(float[] matrix) {
            if (matrix.Length != 16) {
                return false;
            }

            int i = 0; 
            float[] m = matrix;
            float m11 = m[i++]; float m21 = m[i++]; float m31 = m[i++]; float m41 = m[i++];
            float m12 = m[i++]; float m22 = m[i++]; float m32 = m[i++]; float m42 = m[i++];
            float m13 = m[i++]; float m23 = m[i++]; float m33 = m[i++]; float m43 = m[i++];
            float m14 = m[i++]; float m24 = m[i++]; float m34 = m[i++]; float m44 = m[i++];

            foreach (FrameSpec frame in frameList) {
                foreach (BodyInfo b in frame.BodyInfoList) {
                    float x = b.x ;
                    float y = b.y ;
                    float z = b.z ;

                    float wr = 1/(m41 * x + m42 * y + m43 * z + m44);

                    b.x = (float)((m11 * x + m12 * y + m13 * z + m14) * wr);
                    b.y = (float)((m21 * x + m22 * y + m23 * z + m24) * wr);
                    b.z = (float)((m31 * x + m32 * y + m33 * z + m34) * wr);
                }
            }

            if (trackingTrend) {
                for (int idx = 1; idx < frameList.Count; idx++) {
                    CalculateTrendType(frameList[idx - 1], frameList[idx]);
                }
            }

            return true;
        }

        public bool InterpolateClip(int stepSize) {
            List<FrameSpec> newList = new List<FrameSpec>();
            newList.Add(frameList[0]);
            int Rows = frameList[0].BodyInfoList.Length;
            float width = frameList[0].MapWidth;
            float height = frameList[0].MapHeight;
            float depth = frameList[0].MapDepth;
            short mapType = frameList[0].MapType;

            for (int i = 1; i < frameList.Count; i++ ) {
                FrameSpec f0 = frameList[i - 1];
                FrameSpec f1 = frameList[i];

                double sum2 = 0;
                for (int row = 0; row < Rows; row++) {
                    BodyInfo b0 = f0.BodyInfoList[row];
                    BodyInfo b1 = f1.BodyInfoList[row];
                    double dx = 10 * (b0.x - b1.x);
                    double dy = 10 * (b0.y - b1.y);
                    double dz = 10 * (b0.z - b1.z);

                    if ((f0.MapType == 100) || (f0.MapType == 101)) {
                        if (dx > 5 * width) {
                            dx = 10 * width - dx;
                        } else if (dx < -5 * width) {
                            dx = 10 * width + dx;
                        }

                        if (dy > 5 * height) {
                            dy = 10 * height - dy;
                        } else if (dy < -5 * height) {
                            dy = 10 * height + dy;
                        }

                        if (dz > 5 * depth) {
                            dz = 10 * depth - dz;
                        } else if (dz < -5 * depth) {
                            dz = 10 * depth + dz;
                        }
                    }
                    sum2 += dx * dx + dy * dy + dz * dz;
                }
                sum2 /= Rows;
                sum2 = Math.Sqrt(sum2);
                int K = (int)(sum2/stepSize);
                K = Math.Min(100, K);

                for (int k = 1; k <=K; k++) {
                    float p1 = ((float)k) / (K+1);
                    float p0 = 1.0f - p1;
                    FrameSpec f = new FrameSpec(width, height, depth, mapType, Rows);

                    if ((f.MapType == 100) || (f.MapType == 101)) {
                        for (int row = 0; row < Rows; row++) {
                            BodyInfo b0 = f0.BodyInfoList[row];
                            BodyInfo b1 = f1.BodyInfoList[row];
                            BodyInfo b = f.BodyInfoList[row];
                            b.type = b1.type;

                            double v0, v1;
                            if (Math.Abs(b0.x - b1.x) > 5 * width ) {
                                if (b0.x > b1.x) {
                                    v0 = b0.x;
                                    v1 = b1.x + 10 * width;
                                } else {
                                    v0 = b0.x + 10 * width;
                                    v1 = b1.x;
                                }
                            } else {
                                v0 = b0.x;
                                v1 = b1.x;
                            }
                            b.x = (short)(p0 * v0 + p1 * v1);

                            if (Math.Abs(b0.y - b1.y) > 5 * height) {
                                if (b0.y > b1.y) {
                                    v0 = b0.y;
                                    v1 = b1.y + 10 * height;
                                } else {
                                    v0 = b0.y + 10 * height;
                                    v1 = b1.y;
                                }
                            } else {
                                v0 = b0.y;
                                v1 = b1.y;
                            }
                            b.y = (short)(p0 * v0 + p1 * v1);

                            if (Math.Abs(b0.z - b1.z) > 5 * depth) {
                                if (b0.z > b1.z) {
                                    v0 = b0.z;
                                    v1 = b1.z + 10 * depth;
                                } else {
                                    v0 = b0.z + 10 * depth;
                                    v1 = b1.z;
                                }
                            } else {
                                v0 = b0.z;
                                v1 = b1.z;
                            }
                            b.z = (short)(p0 * v0 + p1 * v1);

                        }
                    } else {
                        for (int row = 0; row < Rows; row++) {
                            BodyInfo b0 = f0.BodyInfoList[row];
                            BodyInfo b1 = f1.BodyInfoList[row];
                            BodyInfo b = f.BodyInfoList[row];
                            b.x = (short)(p0 * b0.x + p1 * b1.x);
                            b.y = (short)(p0 * b0.y + p1 * b1.y);
                            b.z = (short)(p0 * b0.z + p1 * b1.z);
                            b.type = b1.type;
                        }
                    }

                    newList.Add(f);
                }
                newList.Add(f1);
            }


            frameList.Clear();
            frameList.AddRange(newList);
            ShowFrame(currentFrame * ( 1 + stepSize));
            SetMaximum(frameList.Count);
            return true;
        }

        
        void ReplayFrameList() {
            if (currentFrame < 0) return;
            List<IBody> bodyList = GetBodyList();
            stopFlag = false;
            for (int frameIdx = currentFrame; frameIdx < frameList.Count; frameIdx+=replayInterval) {
                SetCurrentValue(frameIdx);
                ShowFrame(bodyList, frameList[frameIdx]);
                Application.DoEvents();
                Thread.Sleep(InterframePause);
                if (stopFlag) {
                    SyncMode();
                    break;
                }
                if (autoReverse && (frameIdx+replayInterval) > (frameList.Count-1)) {
                    frameIdx = -replayInterval;
                }
            }
            isPlaying = false;
            SyncMode();
        }

        void ShowFrame(List<IBody> bodyList, FrameSpec frame) {
            int count = Math.Min(bodyList.Count, frame.BodyInfoList.Length);
            for (int i = 0; i < count; i++) {
                BodyInfo b = frame.BodyInfoList[i];
                IBody body = bodyList[i];

                body.X = b.x;
                body.Y = b.y;
                body.Z = b.z;
                body.Type = b.type;
                SetFlags(body, b.flags);
            }

            if ( playTarget != null) {
                RedrawTarget();
                return;
            }

            IMap map = app.ScriptApp.Map;

            if ( (frame.MapWidth * frame.MapHeight) > 0) {
                    if (
                   ((int)frame.MapWidth != (int)map.Width)
                || ((int)frame.MapHeight != (int)map.Height)
                || ((int)frame.MapDepth != (int)map.Depth)
                || (frame.MapType != (short)map.MapTypeIndex)) {
                        map.Width = frame.MapWidth;
                        map.Height = frame.MapHeight;
                        map.Depth = frame.MapDepth;
                        map.MapTypeIndex = frame.MapType; // this call will trigger a MapConfigured event.
                } else {
                    app.RaiseBodyMoved(this);
                }
            } else {
                app.RaiseBodyMoved(this);
            }
        }

        void SetMaximum(int maxValue) {
            progressBar.Maximum = Math.Max(0, maxValue);
            labelMax.Text = maxValue.ToString();
            this.Refresh();
        }

        void SetCurrentValue(int currentValue) {
            currentFrame = currentValue;
            progressBar.Value = currentValue + 1;
            labelCurrentFrame.Text = (currentValue+1).ToString();
            this.Refresh();
        }

        void btnRecording_Click(object sender, EventArgs e) {
            isRecording = ! isRecording;
            SyncMode();
        }

        void SyncMode() {
            this.btnRecording.Image = isRecording ?
                global::ClipRecorder.Properties.Resources.Recording :
                global::ClipRecorder.Properties.Resources.NotRecording;

            this.btnPlayStop.Image = isPlaying ?
                global::ClipRecorder.Properties.Resources.Stop :
                global::ClipRecorder.Properties.Resources.Play;
        }

        void RecorderForm_MouseDown(object sender, MouseEventArgs e) {
            lastX = e.X;
            lastY = e.Y;
            this.MouseMove+=new MouseEventHandler(RecorderForm_MouseMove);
        }

        void RecorderForm_MouseMove(object sender, MouseEventArgs e) {
            int dx = e.X - lastX;
            int dy = e.Y - lastY;
            lastX = e.X;
            lastY = e.Y;
            if ((dx != 0) || (dy != 0)) {
                SetBounds(Left + dx, Top + dy, 0, 0, BoundsSpecified.Location);
            }
            //
            // The following two stmts prevents endless events triggered 
            // by SetBounds() call itself. I found the work-around
            // at http://www.thescripts.com/forum/thread225672.html suggested by Jeremy Wilde.
            //
            lastX = e.X - dx;
            lastY = e.Y - dy;
        }

        void RecorderForm_MouseUp(object sender, MouseEventArgs e) {
            this.MouseMove -= new MouseEventHandler(RecorderForm_MouseMove);
        }

        void SyncCurrentFrame() {
            if (currentFrame < 0) {
                return;
            }
            if ( currentFrame < frameList.Count) {
                ShowFrame(GetBodyList(), frameList[currentFrame]);
            }
        }

        void btnToEnd_Click(object sender, EventArgs e) {
            StopPlaying();
            SetCurrentValue(frameList.Count-1);
            SyncCurrentFrame();
        }

        void btnReset_Click(object sender, EventArgs e) {
            Reset();
        }

        void btnClearAll_Click(object sender, EventArgs e) {
            ClearRecorder();
        }

        bool SaveClip() {
            using (BlobStream blob = new BlobStream(clipFilePath, true))
            using (BinaryWriter writer = new BinaryWriter(blob.Stream)) {
                int dimension = app.ScriptApp.Dataset.CurrentMap.Dimension;
                writer.Write(frameList.Count);
                if (frameList.Count == 0) {
                    writer.Write(0);
                } else {
                    writer.Write(frameList[0].BodyInfoList.Length);
                }
                
                writer.Write((ClipTitle == null) ? "" : ClipTitle);

                for (int i = 0; i < frameList.Count; i++) {
                    FrameSpec frame = frameList[i];
                    writer.Write(frame.MapWidth);
                    writer.Write(frame.MapHeight);
                    float mapDepth = (dimension == 3) ? (float)frame.MapDepth : (float)0;
                    writer.Write(mapDepth);
                    writer.Write(frame.MapType);

                    for (int j = 0; j < frame.BodyInfoList.Length; j++) {
                        BodyInfo b = frame.BodyInfoList[j];
                        writer.Write(b.x);
                        writer.Write(b.y);
                        writer.Write(b.z);
                        writer.Write(b.type);
                        writer.Write(b.flags);
                    }
                }
            }
            return true;
        }

        bool LoadClipFile(string filePath, bool append) {
            try {
                using (BlobStream blob = new BlobStream(filePath, false))
                using (BinaryReader reader = new BinaryReader(blob.Stream)) {
                    int frames = reader.ReadInt32();
                    int bodies = reader.ReadInt32();
                    if (bodies != app.ScriptApp.Dataset.BodyCount) {
                        return false;
                    }

                    ClipTitle = reader.ReadString();

                    if (frameList == null) {
                        frameList = new List<FrameSpec>(frames);
                    }

                    if (!append) {
                        frameList.Clear();
                    }

                    for (int i = 0; i < frames; i++) {
                        float mapWidth = reader.ReadSingle();
                        float mapHeight = reader.ReadSingle();
                        float mapDepth = reader.ReadSingle();
                        short mapType = reader.ReadInt16();
                        FrameSpec frame = new FrameSpec(mapWidth, mapHeight, mapDepth, mapType, bodies);
                        for (int j = 0; j < bodies; j++) {
                            BodyInfo b = frame.BodyInfoList[j];
                            b.x = reader.ReadSingle();
                            b.y = reader.ReadSingle();
                            b.z = reader.ReadSingle();
                            b.type = reader.ReadInt16();
                            b.flags = reader.ReadUInt16();
                        }
                        frameList.Add(frame);
                    }
                    currentFrame = 0;
                    SetMaximum(frameList.Count);
                    SyncCurrentFrame();
                }
            } catch (Exception ex) {
                MessageBox.Show("Cannot open clip file: " + ex.Message);
            }

            return true;
        }


        class BlobStream : IDisposable {
            Stream stream;
            IBlob blob;
            public BlobStream(string path, bool forWrite) {
                if (path.StartsWith("vmb:")) {
                    string blobName = path.Substring(4);
                    blob = RecorderForm.app.ScriptApp.Folder.OpenBlob(blobName, forWrite);
                    if (blob == null) {
                        throw new Exception("Cannot open blob: " + blobName);
                    }
                    stream = blob.Stream;
                    blob.ContentType = "video/clip";
                } else {
                    if (forWrite) {
                        stream = File.Create(path);
                    } else {
                        stream = File.OpenRead(path);
                    }
                }
            }

            public void Dispose() {
                if (blob != null) {
                    blob.Close();
                } else {
                    if (stream != null) {
                        stream.Close();
                        stream.Dispose();
                    }
                }
            }

            public Stream Stream {
                get { return stream; }
            }
        }

        void btnSave_Click(object sender, EventArgs e) {
            var opFile = new SaveFileDialog();
            opFile.AddExtension = true;
            opFile.RestoreDirectory = true;
            opFile.Filter = "Map Clip Files (*.clip)|*.clip";
            opFile.Title = "Select clip file to save";
            opFile.CheckFileExists = false;
            opFile.CheckPathExists = true;
            if (File.Exists(clipFilePath)) {
                FileInfo fo = new FileInfo(clipFilePath);
                opFile.FileName = fo.Name;
                opFile.InitialDirectory = fo.DirectoryName;
            }

            if (opFile.ShowDialog() == DialogResult.OK) {
                clipFilePath = opFile.FileName;
                SaveClip();
            }
        }


        void btnLoad_Click(object sender, EventArgs e) {
            var opFile = new OpenFileDialog();
            opFile.Filter = "Map Clip Files (*.clip)|*.clip";
            opFile.Title = "Select clip file to load";
            opFile.CheckFileExists = true;
            opFile.RestoreDirectory = true;
            if (File.Exists(clipFilePath)) {
                FileInfo fo = new FileInfo(clipFilePath);
                opFile.FileName = fo.Name;
                opFile.InitialDirectory = fo.DirectoryName;
            }

            if (opFile.ShowDialog() == DialogResult.OK) {
                clipFilePath = opFile.FileName;
                LoadClipFile(clipFilePath, false);
            }
        }

        bool Recording {
            get { return isRecording; }
            set { isRecording = value; SyncMode(); }
        }

        void btnConfigure_Click(object sender, EventArgs e) {
            propMan.StartEditor("Configure Clip Recorder", 300, 250);
        }

        void btnInformation_Click(object sender, EventArgs e) {
            Form frm = new AboutClipRecord();
            frm.Show();
        }

        #region Properties
        [Configurable, Saved, Description("Rewind automatically when the end of the clip has been reached.")]
        public bool AutoReverse {
            get { return autoReverse; }
            set { autoReverse = value; }
        }

        [Configurable, Saved, Description("Pause time in milliseconds between frames during the replay.")]
        public int InterframePause {
            get { return interframePause; }
            set { interframePause = value; }
        }

        [Configurable, Saved, Description("Replay interval length in frames.")]
        public int ReplayInterval {
            get { return replayInterval; }
            set { replayInterval = Math.Max(1, value); }
        }

        [Configurable, Description("Title for saved clip file.")]
        public string ClipTitle {
            get { return clipTitle.Text; }
            set { clipTitle.Text = value; }
        }
        
        [Configurable, Saved, Description("Set the body types to track the movement trend.")]
        public bool TrackingTrend {
            get { return trackingTrend; }
            set { trackingTrend = value; }
        }

        [Configurable, Saved, Description("Script file path."), Editor(typeof(ScriptFileSelector), typeof(UITypeEditor))]
        public string ScriptPath {
            get { return scriptPath; }
            set { scriptPath = value; }
        }

        public class ClipFileSelector : System.Windows.Forms.Design.FileNameEditor {
            protected override void InitializeDialog(OpenFileDialog openFileDialog) {
                base.InitializeDialog(openFileDialog);
                openFileDialog.Filter = "Map Clip Files (*.clip)|*.clip";
                openFileDialog.Title = "Select Clip File";
                openFileDialog.CheckFileExists = false;
            }
            
        }

        [Configurable, Saved, Description("Clip file path."), Editor(typeof(ClipFileSelector), typeof(UITypeEditor))]
        public string ClipFilePath {
            get { return clipFilePath; }
            set { clipFilePath = value; }
        }
        #endregion

        void MoveFrame(int x) {
            float delta = 0.5f * progressBar.Width / progressBar.Maximum;
            int clickedIdx = x * frameList.Count / progressBar.Width;
            SetCurrentValue(clickedIdx);
            SyncCurrentFrame();
        }

        bool mouseDown = false;

        void progressPanel_MouseDown(object sender, MouseEventArgs e) {
            mouseDown = true;
            Cursor.Current = Cursors.Hand;
            StopPlaying();
            MoveFrame(e.X);
        }

        void progressPanel_MouseMove(object sender, MouseEventArgs e) {
            if (!mouseDown) {
                return;
            }
            MoveFrame(e.X);
        }

        void progressPanel_MouseUp(object sender, MouseEventArgs e) {
            mouseDown = false;
            Cursor.Current = Cursors.Default;
        }

        void RecorderForm_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                if ((fileNames.Length >= 1) && (fileNames[0].ToLower().EndsWith(".clip"))) {
                    e.Effect = DragDropEffects.Copy;
                } else {
                    e.Effect = DragDropEffects.None;
                }
            } else {
                e.Effect = DragDropEffects.None;
            }
        }

        void RecorderForm_DragDrop(object sender, DragEventArgs e) {
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            frameList.Clear();
            foreach(string fileName in fileNames) {
                if ( fileName.ToLower().EndsWith(".clip")) {
                    LoadClipFile(fileName, true );
                }
            }
        }

        void btnScript_Click(object sender, EventArgs e) {
            IVisuMap vv = app.ScriptApp;
            
            if (string.IsNullOrEmpty(scriptPath) || (Control.ModifierKeys & Keys.Shift) == Keys.Shift) {
                if (string.IsNullOrEmpty(scriptPath)) {
                    scriptPath = "!vv.Message(pp.Name);";
                }
                IScriptPathEditor editor = vv.New.ScriptPathEditor(scriptPath, this, "");
                editor.TheForm.FormClosed += new FormClosedEventHandler(delegate(object snd, FormClosedEventArgs args) {
                    if (editor.TheForm.DialogResult == DialogResult.OK) {
                        scriptPath = editor.ScriptPath;
                    }
                });

                editor.Show();
            } else {
                vv.RunScript(scriptPath, this, sender);
            }
        }

        public Bitmap Snapshot() {
            return null;
        }

        #region Some Script Interfaces
        public void ClearRecorder() {
            frameList.Clear();
            SetMaximum(0);
            SetCurrentValue(0);
        }

        public void Reset() {
            StopPlaying();
            SetCurrentValue((frameList.Count > 0) ? 0 : -1);
            SyncCurrentFrame();
        }

        public bool LoadClip(string filePath) {
            return LoadClipFile(filePath, false);
        }

        public int CurrentFrame {
            get { return currentFrame; }
        }

        public List<FrameSpec> FrameList {
            get { return frameList; }
        }
        #endregion

        #region Implementation of the IForm interface
        public new IForm Resize(int left, int top, int width, int height) {
            SetBounds(left, top, width, height, BoundsSpecified.All);
            return this;
        }

        public string Title {
            get { return this.Text; }
            set { this.Text = value; }
        }

        public IForm Normalize() {
            this.WindowState = FormWindowState.Normal;
            return this;
        }

        public IForm Minimize() {
            this.WindowState = FormWindowState.Minimized;
            return this;
        }

        public IForm Maximize() {
            this.WindowState = FormWindowState.Maximized;
            return this;
        }

        public new IForm Show() {
            base.Show();
            return this;
        }

        public new bool ShowDialog() {
            return (base.ShowDialog() != System.Windows.Forms.DialogResult.Cancel);
        }

        public virtual IForm Show2() {
            return this.Show();
        }

        public void Detach() { ; }

        public bool AddEventHandler(string eventName, string scriptPath) {
            return true;
        }

        public bool RemoveEventHandler(string eventName) {
            return true;
        }

        public bool ClickContextMenu(string label) {
            return false;
        }

        public Form TheForm {
            get { return this; }
            set { ; }
        }
        #endregion

        //private void RecorderForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            switch (keyData) {
                case Keys.Left:
                    PlaySteps(-1);
                    return true;

                case Keys.Right:
                    PlaySteps(1);
                    return true; 
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}