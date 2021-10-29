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
using MT = VisuMap.MT;

namespace ClipRecorder {
    public partial class RecorderForm : Form, IForm {
        static IApplication app;
        List<FrameSpec> frameList;
        int currentFrame = -1;  // The first frame. 
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
            int n = e.Delta / 120;
            PlaySteps(-Math.Sign(n) * (n * n));
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
                if (playTarget != null) {
                    playTarget.TheForm.Disposed += (s, e) => { playTarget = null; };
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

        void CopyBodyToFrame(IList<IBody> bodies, FrameSpec frame) {
            MT.Loop(0, bodies.Count, i => {
                ref BodyInfo b = ref frame.BodyInfoList[i];
                IBody body = bodies[i];
                b.x = (float)(body.X);
                b.y = (float)(body.Y);
                b.z = (float)(body.Z);
                b.type = body.Type;
                b.flags = Flags(body);
            });
        }

        void CopyFrameToBody(FrameSpec frame, IList<IBody> bodies) {
            MT.Loop(0, bodies.Count, i => {
                BodyInfo b = frame.BodyInfoList[i];
                IBody body = bodies[i];
                body.X = b.x;
                body.Y = b.y;
                body.Z = b.z;
                body.Type = b.type;
                SetFlags(body, b.flags);
            });
        }

        public int CreateSnapshot(float timestamp = 0) {
            List<IBody> bodyList = app.ScriptApp.Map.BodyList as List<IBody>;
            var frm = NewFrame(bodyList);
            frm.Timestamp = timestamp;
            frameList.Add(frm);
            SetMaximum(frameList.Count);
            return frameList.Count;
        }


        FrameSpec NewFrame(IList<IBody> bodyList) {
            IMap map = app.ScriptApp.Map;
            FrameSpec frame = new FrameSpec(
                (short)map.Width, (short)map.Height, (short)map.Depth, (short)map.MapTypeIndex, bodyList.Count);
            CopyBodyToFrame(bodyList, frame);
            return frame;
        }

        public int AddSnapshot(IList<IBody> bodyList) {
            FrameSpec frame = NewFrame(bodyList);
            frameList.Add(frame);
            SetMaximum(frameList.Count);
            if (currentFrame < 0)
                SetCurrentValue(0);
            return frameList.Count;
        }

        public bool InsertSnapshot(int frameIndex, IList<IBody> bodyList) {
            if ((bodyList == null) || (frameIndex > FrameList.Count))
                return false;
            FrameSpec frame = NewFrame(bodyList);
            frameList.Insert(frameIndex, frame);
            if (frameIndex <= currentFrame)
                SetCurrentValue(currentFrame + 1);
            SetMaximum(frameList.Count);
            return true;
        }

        public void DeleteFrame(int frameIndex = -1) {
            if (frameIndex == -1)
                frameIndex = currentFrame;
            frameList.RemoveAt(frameIndex);
            if (frameIndex < currentFrame)
                currentFrame -= 1;
            else if (frameIndex == currentFrame) {
                currentFrame = Math.Min(currentFrame, frameList.Count - 1);
                if (currentFrame >= 0)
                    ShowFrame(GetBodyList(), frameList[currentFrame]);
            }
            SetCurrentValue(currentFrame);
            SetMaximum(frameList.Count);
        }

        public bool ReplaceFrame(int frameIndex, IList<IBody> bodyList) {
            if (frameList.Count == 0) {
                AddSnapshot(bodyList);
                currentFrame = 0;
                return true;
            }
            if ((bodyList == null) || (frameIndex >= FrameList.Count) || (frameIndex < 0))
                return false;
            FrameSpec frame = NewFrame(bodyList);
            frameList[frameIndex] = frame;
            if (frameIndex == currentFrame)
                ShowFrame(GetBodyList(), frameList[currentFrame]);
            return true;
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
            isPlaying = !isPlaying;
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

        public bool InterpolateClip(int interFrames) {
            List<FrameSpec> newList = new List<FrameSpec>();
            newList.Add(frameList[0]);
            int Rows = frameList[0].BodyInfoList.Length;
            float width = frameList[0].MapWidth;
            float height = frameList[0].MapHeight;
            float depth = frameList[0].MapDepth;
            short mapType = frameList[0].MapType;

            for (int i = 1; i < frameList.Count; i++) {
                FrameSpec f0 = frameList[i - 1];
                FrameSpec f1 = frameList[i];

                for (int k = 1; k <= interFrames; k++) {
                    double p1 = ((double)k) / (interFrames + 1);
                    double p0 = 1.0 - p1;
                    FrameSpec f = new FrameSpec(width, height, depth, mapType, Rows);

                    if ((f.MapType == 100) || (f.MapType == 101)) {
                        MT.Loop(0, Rows, row => {
                            ref BodyInfo b0 = ref f0.BodyInfoList[row];
                            ref BodyInfo b1 = ref f1.BodyInfoList[row];
                            ref BodyInfo b = ref f.BodyInfoList[row];
                            b.type = b1.type;
                            b.flags = b1.flags;

                            double v0, v1;
                            if (Math.Abs(b0.x - b1.x) > 5 * width) {
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
                            b.x = (float)(p0 * v0 + p1 * v1);

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
                            b.y = (float)(p0 * v0 + p1 * v1);

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
                            b.z = (float)(p0 * v0 + p1 * v1);
                        });
                    } else {
                        MT.Loop(0, Rows, row => {
                            ref BodyInfo b0 = ref f0.BodyInfoList[row];
                            ref BodyInfo b1 = ref f1.BodyInfoList[row];
                            ref BodyInfo b = ref f.BodyInfoList[row];
                            b.x = (float)(p0 * b0.x + p1 * b1.x);
                            b.y = (float)(p0 * b0.y + p1 * b1.y);
                            b.z = (float)(p0 * b0.z + p1 * b1.z);
                            b.type = b1.type;
                            b.flags = b1.flags;
                        });
                    }

                    f.Timestamp = (float)(p0 * f0.Timestamp + p1 * f1.Timestamp);
                    newList.Add(f);
                }
                newList.Add(f1);
            }

            frameList.Clear();
            frameList.AddRange(newList);
            ShowFrame(0);
            SetMaximum(frameList.Count);
            return true;
        }

        void ReplayFrameList() {
            if (frameList.Count <= 1)
                return;
            currentFrame = Math.Max(0, currentFrame);
            currentFrame = Math.Min(frameList.Count - 1, currentFrame);
            List<IBody> bodyList = GetBodyList();
            stopFlag = false;
            for (int frameIdx = currentFrame; frameIdx < frameList.Count; frameIdx += replayInterval) {
                SetCurrentValue(frameIdx);
                ShowFrame(bodyList, frameList[frameIdx]);
                Application.DoEvents();
                Thread.Sleep(InterframePause);
                if (stopFlag) {
                    SyncMode();
                    break;
                }
                if (autoReverse && (frameIdx + replayInterval) > (frameList.Count - 1)) {
                    frameIdx = -replayInterval;
                }
            }
            isPlaying = false;
            SyncMode();
        }

        void ShowFrame(List<IBody> bodyList, FrameSpec frame) {
            int count = Math.Min(bodyList.Count, frame.BodyInfoList.Length);
            CopyFrameToBody(frame, bodyList);

            if (playTarget != null) {
                RedrawTarget();
                return;
            }

            IMap map = app.ScriptApp.Map;

            if ((frame.MapWidth * frame.MapHeight) > 0) {
                if (
               ((int)frame.MapWidth != (int)map.Width)
            || ((int)frame.MapHeight != (int)map.Height)
            || ((int)frame.MapDepth != (int)map.Depth)
            || (frame.MapType != (short)map.MapTypeIndex)) {
                    map.Width = frame.MapWidth;
                    map.Height = frame.MapHeight;
                    map.Depth = frame.MapDepth;
                    map.MapTypeIndex = frame.MapType; // this call will trigger a MapConfigured event.
                    app.RaiseMapConfigured(this);
                }
            }
            app.RaiseBodyConfigured(this);
        }

        void SetMaximum(int maxValue) {
            progressBar.Maximum = Math.Max(0, maxValue);
            labelMax.Text = maxValue.ToString();
            this.Refresh();
        }

        void SetCurrentValue(int currentValue) {
            currentFrame = currentValue;
            progressBar.Value = currentValue + 1;
            labelCurrentFrame.Text = (currentValue + 1).ToString();
        }

        void btnRecording_Click(object sender, EventArgs e) {
            isRecording = !isRecording;
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
            this.MouseMove += new MouseEventHandler(RecorderForm_MouseMove);
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
            if (currentFrame < frameList.Count) {
                ShowFrame(GetBodyList(), frameList[currentFrame]);
            }
        }

        void btnToEnd_Click(object sender, EventArgs e) {
            StopPlaying();
            SetCurrentValue(frameList.Count - 1);
            SyncCurrentFrame();
        }

        void btnReset_Click(object sender, EventArgs e) {
            Reset();
        }

        void btnClearAll_Click(object sender, EventArgs e) {
            ClearRecorder();
        }

        string GetFolderFileName() {
            string fn = app.ScriptApp.Folder?.FolderFileName;
            if (fn == null)
                return "";
            return new FileInfo(fn).Name;
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

                string strHead = (ClipTitle == null) ? "" : ClipTitle;
                string fileName = GetFolderFileName();
                string dsName = app.ScriptApp.Dataset?.Name;
                strHead = strHead + ":" + fileName + ":" + dsName;
                writer.Write(strHead);

                for (int i = 0; i < frameList.Count; i++) {
                    FrameSpec frame = frameList[i];
                    writer.Write(frame.MapWidth);
                    writer.Write(frame.MapHeight);
                    float mapDepth = (dimension == 3) ? (float)frame.MapDepth : (float)0;
                    writer.Write(mapDepth);
                    writer.Write(frame.MapType);
                    writer.Write(frame.Timestamp);

                    foreach (BodyInfo b in frame.BodyInfoList) {
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
                    string strHead = reader.ReadString();

                    IDataset ds = app.ScriptApp.Dataset;
                    string[] fs = strHead.Split(':');
                    ClipTitle = fs[0];
                    string dsFile = fs[1];
                    string dsName = fs[2];
                    if ((dsFile != GetFolderFileName()) || (dsName != ds.Name) || (bodies != ds.BodyCount) ) {
                        var ret = MessageBox.Show("The origine of the clips (" + dsFile + ":" + dsName + ":" + bodies + ") doesn't match the current dataset!"
                            + "\nClick Yes to continue or No to cancel.", "Invalid Clip File", MessageBoxButtons.YesNo);
                        if (ret == DialogResult.No)
                            return false;
                    }

                    if (frameList == null)
                        frameList = new List<FrameSpec>(frames);

                    if (!append)
                        frameList.Clear();

                    for (int i = 0; i < frames; i++) {
                        float mapWidth = reader.ReadSingle();
                        float mapHeight = reader.ReadSingle();
                        float mapDepth = reader.ReadSingle();
                        short mapType = reader.ReadInt16();
                        FrameSpec frame = new FrameSpec(mapWidth, mapHeight, mapDepth, mapType, bodies);
                        frame.Timestamp = reader.ReadSingle();
                        for (int j = 0; j < bodies; j++) {
                            ref BodyInfo b = ref frame.BodyInfoList[j];
                            b.x = reader.ReadSingle();
                            b.y = reader.ReadSingle();
                            b.z = reader.ReadSingle();
                            b.type = reader.ReadInt16();
                            b.flags = reader.ReadUInt16();
                        }
                        frameList.Add(frame);
                    }
                    currentFrame = (frameList.Count == 0) ? -1 : 0;
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
            clickedIdx = Math.Max(0, Math.Min(frameList.Count - 1, clickedIdx));
            SetCurrentValue(clickedIdx);
            SyncCurrentFrame();
        }

        bool mouseDown = false;

        void progressPanel_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left)
                return;
            mouseDown = true;
            Cursor.Current = Cursors.Hand;
            StopPlaying();
            MoveFrame(e.X);
            progressBar.MarkerIndex = Control.ModifierKeys.HasFlag(Keys.Control) ? currentFrame : -1;
        }

        void progressPanel_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left)
                return;
            if (!mouseDown) {
                return;
            }
            MoveFrame(e.X);
        }

        void progressPanel_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left)
                return;
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
            foreach (string fileName in fileNames) {
                if (fileName.ToLower().EndsWith(".clip")) {
                    LoadClipFile(fileName, true);
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
                editor.TheForm.FormClosed += new FormClosedEventHandler(delegate (object snd, FormClosedEventArgs args) {
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

        public float Timestamp {
            get {
                if ((currentFrame >= 0) && (currentFrame < frameList.Count)) {
                    var frm = frameList[currentFrame];
                    return frm.Timestamp;
                } else
                    return 0;
            }
            set {
                if ((currentFrame >= 0) && (currentFrame < frameList.Count)) {
                    var frm = frameList[currentFrame];
                    frm.Timestamp = value;
                }
            }
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

        public void Detach() {; }

        public bool AddEventHandler(string eventName, string scriptPath) {
            throw new Exception("AddContextMenu not supported for RecorderForm!");
        }

        public bool RemoveEventHandler(string eventName) {
            throw new Exception("AddContextMenu not supported for RecorderForm!");
        }

        public bool ClickContextMenu(string label) {
            return new FormImp(this).ClickContextMenu(label);
        }

        public bool AddContextMenu(string label, string scriptPath, object srcObj = null, string iconPath = null, string menuTip = null) {
            return new FormImp(this).AddContextMenu(label, scriptPath, srcObj, iconPath, menuTip);
        }

        public Form TheForm
        {
            get => this;
            set {; }
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

        private void miCaptureFrame_Click(object sender, EventArgs e) {
            ReplaceFrame(currentFrame, GetBodyList());
        }

        private void miAppendFrame_Click(object sender, EventArgs e) {
            AddSnapshot(GetBodyList());
        }

        private void miDeleteFrame_Click(object sender, EventArgs e) {
            int mkIdx = progressBar.MarkerIndex;
            if ((mkIdx >= 0) && (mkIdx != currentFrame)) {
                if (mkIdx < currentFrame)
                    frameList.RemoveRange(mkIdx, currentFrame);
                else
                    frameList.RemoveRange(currentFrame, mkIdx);
                SetCurrentValue(0);
                SetMaximum(frameList.Count);
                progressBar.MarkerIndex = -1;
                this.Refresh();
            } else
                DeleteFrame(currentFrame);
        }

        private void miRefreshFrame_Click(object sender, EventArgs e) {
            ShowFrame(GetBodyList(), frameList[currentFrame]);
        }

        private void miInterpolation_Click(object sender, EventArgs e) {
            InterpolateClip(4);
        }

        private void miNewWindow_Click(object sender, EventArgs e) {
            int mkIdx = progressBar.MarkerIndex;
            if ( (mkIdx<0) || (mkIdx==currentFrame) ) {
                MessageBox.Show("No frames selected for the new window");
                return;
            }
            var newForm = new RecorderForm(RecorderForm.app);
            if (mkIdx > currentFrame )
                for (int i = currentFrame; i <= mkIdx; i++)
                    newForm.frameList.Add(this.frameList[i]);
            else
                for (int i = mkIdx; i <= currentFrame; i++)
                    newForm.frameList.Add(this.frameList[i]);
            newForm.SetCurrentValue(0);
            newForm.SetMaximum(newForm.frameList.Count);
            newForm.playTarget = this.playTarget;
            newForm.Show();
        }

        private void miReverseFrames_Click(object sender, EventArgs e) {
            frameList.Reverse();
            if ( (frameList.Count>0) && (currentFrame >= 0) ) 
                SetCurrentValue(frameList.Count - 1 - currentFrame);
        }
    }
}