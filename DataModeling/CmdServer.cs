using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using VisuMap.Script;

namespace VisuMap.DataModeling {
    
    public class CmdServer : IDisposable {
        IVisuMap app;
        UdpClient serverSkt;
        TcpListener tcpListener = null;
        Form mainForm;
        const int bufSize = 16*1024;

        public delegate bool CmdHandle(IPEndPoint sender, int cmd, byte[] data);
        Thread svrThread;
        List<CmdHandle> cmdHandles = new List<CmdHandle>();

        const int CmdMsg = 102;
        const int CmdLogMsg = 110;
        const int CmdClrLog = 112;
        const int CmdRunScript = 111;
        const int CmdGetExpression = 114;
        const int CmdColorData = 118;
        const int CmdTsne = 200;
        const int CmdPing = 120;
        const int CmdOK = 121;
        const int CmdShMatrix2 = 122;
        const int CmdGetProperty = 125;
        const int CmdSetProperty = 126;
        const int CmdFail = 129;
        const int CmdSaveTable = 133;
        const int CmdLoadBlob = 134;
        const int CmdSaveBlob = 135;
        const int CmdLoadTable0 = 132;
        const int CmdLoadTable = 127;
        const int CmdGetItemIds = 137;
        const int CmdSelectItems = 138;
        const int CmdUpdateLabels = 140;
        const int CmdLoadLabels = 141;
        const int CmdLoadDistances = 142;
        const int CmdSetColumnIds = 143;
        const int CmdLoadMap = 144;

        IInfoPad logger;

        public CmdServer() {
            app = DataModeling.App.ScriptApp;
            mainForm = app.TheForm;
            cmdHandles.Add(MainCmdHandler);
        }    

        public void AddListener(CmdHandle cmdHandle) {
            cmdHandles.Add(cmdHandle);
        }

        public void RemoveListener(CmdHandle cmdHandle) {
            cmdHandles.Remove(cmdHandle);
        }

        public void Start() {
            svrThread = new Thread(ServerProc);
            svrThread.Start();
        }

        public UdpClient ServerSocket
        {
            get => serverSkt;
        }

        public TcpListener TcpListener { get => tcpListener; set => tcpListener = value; }

        public void Dispose() {
            if (( svrThread != null) && svrThread.IsAlive) {
                if (serverSkt != null) {
                    serverSkt.Client.Shutdown(SocketShutdown.Receive);
                    serverSkt.Client.Dispose();
                    serverSkt.Close();
                }

                if (tcpListener != null) {
                    tcpListener.Stop();
                    tcpListener.Server.Close();
                    tcpListener.Server.Dispose();
                }
                svrThread.Abort();
            }
        }

        public void ServerProc() {
            try {
                serverSkt = new UdpClient(new IPEndPoint(IPAddress.Any, DataModeling.monitorPort));
                serverSkt.DontFragment = true;
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

                while (true) {
                    byte[] data = serverSkt.Receive(ref sender);
                    if ((data == null) || (data.Length == 0)) {
                        continue;
                    }
                    try {
                        int cmd = System.BitConverter.ToInt32(data, 0);
                        foreach (var h in cmdHandles) {
                            if (h(sender, cmd, data))
                                break;
                        }
                    } catch(System.Threading.ThreadAbortException ex) {
                        TimedMessage("Server Error: " + ex.Message, "Command Server");
                    } catch (Exception ex) {
                        TimedMessage("Server Error: " + ex.Message, "Command Server");
                    }
                    Application.DoEvents();
                }
            } 
            catch(System.Threading.ThreadAbortException) { }
            catch (System.Net.Sockets.SocketException ex) {
                if (ex.ErrorCode != 10004) {
                    MessageBox.Show("Command Port " + DataModeling.monitorPort + " in use!\nUDP command won't be accepted.", "Command Server");
                }
            }
            catch (Exception) { }
        }

        void TimedMessage(string msg, string caption) {
            var w = new Form() { Size = new System.Drawing.Size(0, 0) };
            Task.Delay(TimeSpan.FromSeconds(5))
                .ContinueWith((t) => w.Close(), TaskScheduler.FromCurrentSynchronizationContext());
            MessageBox.Show(w, msg, caption);
        }

        bool MainCmdHandler(IPEndPoint sender, int cmd, byte[] data) {
            bool ret = true;
            switch (cmd) {
                case CmdRunScript:
                    string script = Encoding.UTF8.GetString(data, 4, data.Length - 4);
                    mainForm.Invoke(new MethodInvoker(delegate () {
                        app.RunScript("!" + script, app.LastView, DataModeling.mdScript);
                    }));
                    Application.DoEvents();
                    break;

                case CmdGetExpression:
                    string expression = Encoding.UTF8.GetString(data, 4, data.Length - 4);
                    mainForm.Invoke(new MethodInvoker(delegate () {
                        var retObj = app.RunScript("!vv.Return(" + expression + ");", app.LastView, DataModeling.mdScript);
                        ReturnMsg(sender, retObj.ToString());
                    }));
                    break;

                case CmdSetProperty: {
                        string[] fs = Encoding.UTF8.GetString(data, 4, data.Length - 4).Split('|');
                        app.SetProperty(fs[0], fs[1], "");
                    }
                    break;

                case CmdGetProperty:
                    string propName = Encoding.UTF8.GetString(data, 4, data.Length - 4);
                    string propValue = app.GetProperty(propName, "");
                    ReturnMsg(sender, propValue);
                    break;

                case CmdPing:
                    SendBackOK(sender);
                    Application.DoEvents();
                    break;

                case CmdMsg:
                    MessageBox.Show(Encoding.UTF8.GetString(data, 4, data.Length - 4), "Training Message");
                    break;

                case CmdLogMsg:
                    AddLogMessage(Encoding.UTF8.GetString(data, 4, data.Length - 4));
                    break;

                case CmdClrLog:
                    OpenLogger();
                    mainForm.Invoke(new MethodInvoker(delegate () {
                        logger.Clear();
                    }));
                    break;

                case CmdShMatrix2:
                    DoShowMatrix(data, sender);
                    break;

                case CmdTsne:
                    CheckTcpListener();
                    serverSkt.Send(BitConverter.GetBytes(CmdTsne), 4, sender);
                    DoTsne();
                    break;

                case CmdSaveTable: {
                        string[] fs = Encoding.UTF8.GetString(data, 4, data.Length - 4).Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                        if ((fs == null) || (fs.Length == 0)) {
                            SendBackFail(sender);
                        } else {
                            CheckTcpListener();
                            SendBackOK(sender);
                            TcpClient tcpCnt = tcpListener.AcceptTcpClient();
                            tcpCnt.SendTimeout = tcpCnt.ReceiveTimeout = 10*1000;  // 1000 seconds.
                            tcpCnt.Client.DontFragment = true;
                            var tcpStream = tcpCnt.GetStream();
                            string dsName = fs[0];
                            double[][] table = ReadMatrix(0, 0, tcpStream);
                            tcpCnt.Close();
                            var nt = app.New.NumberTable(table);
                            string desc = (fs.Length >= 2) ? fs[1] : null;
                            nt.SaveAsDataset(fs[0], desc);
                        }
                    }
                    break;

                case CmdLoadBlob:
                    int offset = System.BitConverter.ToInt32(data, 4); // in floats
                    int size = System.BitConverter.ToInt32(data, 8);
                    try {
                        string blobName = Encoding.UTF8.GetString(data, 12, data.Length - 12);
                        IBlob blob = app.Folder.OpenBlob(blobName, false);
                        string[] fs = blob.ContentType.Split(' ');
                        int blobSize = int.Parse(fs[2]); // in floats
                        int dataSize = (size == 0) ? blobSize : Math.Min(size, blobSize-offset);
                        byte[] buf1 = BitConverter.GetBytes(CmdOK);
                        byte[] buf2 = BitConverter.GetBytes(dataSize);
                        CheckTcpListener();
                        serverSkt.Send(buf1.Concat(buf2).ToArray(), 8, sender);
                        TcpClient tcpCnt = tcpListener.AcceptTcpClient();
                        CopyStream(blob.Stream, tcpCnt.GetStream(), offset, dataSize);
                        blob.Close();
                        tcpCnt.Close();
                    } catch (Exception) {
                        SendBackFail(sender);
                    }
                    break;

                case CmdSaveBlob:
                    int length = System.BitConverter.ToInt32(data, 4); // in floats
                    int baseLocation = System.BitConverter.ToInt32(data, 8); // in floats
                    try {
                        string blobName = Encoding.UTF8.GetString(data, 12, data.Length - 12);
                        CheckTcpListener();
                        SendBackOK(sender);
                        TcpClient tcpCnt = tcpListener.AcceptTcpClient();
                        int toread = 4*length;
                        var st = tcpCnt.GetStream();
                        byte[] buf = new byte[4094*4];

                        //Notice: pre-existing file will be overwritten!
                        string fileName = "$\\BlobData\\" + blobName + ".zfs";
                        IBlob blob = app.Folder.CreateFileBlobCompressed(blobName, fileName);                        
                        blob.ContentType = "Float/Seq " + baseLocation + " " + length + " " + blobName;
                        var bStream = (blob.Stream as System.IO.Compression.GZipStream).BaseStream;
                        using (var bw = new BinaryWriter(blob.Stream)) {
                            while (toread > 0) {
                                int len = st.Read(buf, 0, buf.Length);
                                if (len <= 0)
                                    break;
                                bw.Write(buf, 0, len);
                                toread -= len;
                            }
                        }
                        tcpCnt.Close();
                        bStream.Dispose();
                        blob.Close();
                    } catch(Exception) {
                        SendBackFail(sender);
                    }
                    break;

                case CmdLoadTable: {
                        string msg = System.Text.Encoding.UTF8.GetString(data, 4, data.Length - 4);
                        string[] fs = msg.Split('|');
                        string dsName = fs[0];

                        app.TheForm.Invoke(new MethodInvoker(delegate () {
                            if (!string.IsNullOrEmpty(dsName) && (dsName != app.Dataset.Name))
                                app.Folder.OpenDataset(dsName);
                        }));

                        var dsTable = app.Dataset.GetNumberTableEnabled();
                        CheckTcpListener();
                        SendBackOK(sender);
                        var tcpCnt = TcpListener.AcceptTcpClient();

                        using (var bw = new BinaryWriter(new BufferedStream(tcpCnt.GetStream()))) {
                            WriteMatrix((double[][])dsTable.Matrix, bw);
                            double[][] rowTypes = new double[dsTable.Rows][];
                            for (int row = 0; row < dsTable.Rows; row++) rowTypes[row] = new double[] { dsTable.RowSpecList[row].Type };
                            WriteMatrix(rowTypes, bw);
                            bw.Flush();
                        }
                        tcpCnt.Close();
                    }
                    break;

                case CmdLoadTable0: {
                        string msg = System.Text.Encoding.UTF8.GetString(data, 4, data.Length - 4);
                        string[] fs = msg.Split('|');
                        string dsName = fs[0];
                        INumberTable dsTable = null;
                        if (string.IsNullOrEmpty(dsName) || (dsName == app.Dataset.Name)) {
                            dsTable = app.Dataset.GetNumberTableEnabled();
                        } else if (dsName == "@") { // return the currently selected data in the main window as a table.
                            dsTable = app.GetSelectedNumberTable();
                            if (dsTable.Rows == 0)
                                dsTable = app.Dataset.GetNumberTableEnabled();
                            dsTable = dsTable.ApplyFilter(app.Map.Filter);
                        } else if (dsName == "+") {  // return the selected data as a table.
                            Form parentForm = DataModeling.scriptEngine?.ParentForm;
                            if ((parentForm != null) && (parentForm.IsDisposed))
                                parentForm = app.TheForm;
                            if (parentForm == null) {
                                if (Application.OpenForms.Count > 0)
                                    parentForm = Application.OpenForms[Application.OpenForms.Count - 1];
                            }
                            IForm frm = DataModeling.App.ScriptApp.ToForm(parentForm);
                            if (frm is IExportNumberTable)
                                dsTable = (frm as IExportNumberTable).GetSelectedNumberTable();
                        } else if (dsName == "$") {  // return the X,Y,Z data point coordinates as a table.
                            IList<IBody> bs = app.Map.SelectedBodies;
                            if ((bs == null) || (bs.Count == 0))
                                bs = app.Map.BodyList.Where(b => !b.Disabled).ToList() as IList<IBody>;
                            dsTable = app.New.NumberTable(bs.Count, app.Map.Dimension);
                            var m = dsTable.Matrix;
                            for (var row = 0; row < bs.Count; row++) {
                                m[row][0] = bs[row].X;
                                m[row][1] = bs[row].Y;
                                if (app.Map.Dimension==3)
                                    m[row][2] = bs[row].Z;
                            }
                        } else {
                            dsTable = app.Folder.ReadDataset(dsName).GetNumberTableEnabled();
                        }

                        if (dsTable == null) {
                            SendBackFail(sender);
                            return true;
                        }

                        CheckTcpListener();
                        SendBackOK(sender);
                        var tcpCnt = TcpListener.AcceptTcpClient();

                        using (var bw = new BinaryWriter(new BufferedStream(tcpCnt.GetStream()))) {
                            WriteMatrix((double[][])dsTable.Matrix, bw);
                            bw.Flush();
                        }
                        tcpCnt.Close();
                    }
                    break;

                case CmdLoadDistances: {
                        var mtr = app.Map.GetMetric();
                        if (mtr == null) {
                            SendBackFail(sender);
                        } else {
                            CheckTcpListener();
                            SendBackOK(sender);
                            var tcpCnt = TcpListener.AcceptTcpClient();
                            mtr.Distance(0, 0); // triggle possible pre-calculation.
                            IList<string> selected = app.Map.SelectedItems;
                            if ( (selected == null) || (selected.Count <= 1) )
                                selected = app.Dataset.BodyList.Where(b=>!b.Disabled).Select(b => b.Id).ToList();
                            int[] bsIndexes = app.Dataset.BodyIndicesForIds(selected).ToArray();                        
                            int N = bsIndexes.Length;

                            if (Root.Cfg.UsingGpu && 
                                ((mtr.Name == NameResource.mtrEuclidean) || 
                                (mtr.Name == NameResource.mtrCorrelation))) { 
                                    mtr = new GpuMetric(mtr, bsIndexes);
                                    bsIndexes = Enumerable.Range(0, N).ToArray();
                            }

                            using (var bw = new BinaryWriter(new BufferedStream(tcpCnt.GetStream()))) {
                                bw.Write(N);
                                bw.Write(N);
                                bw.Flush();
                                byte[] buf = new byte[N * 4];
                                float[] R = new float[N];
                                for (int row = 0; row < N; row++) {
                                    int idxRow = bsIndexes[row];
                                    Parallel.For(0, N, col => {
                                        R[col] = (col == row) ?
                                            0.0f : (float)mtr.Distance(idxRow, bsIndexes[col]);
                                    });
                                    Buffer.BlockCopy(R, 0, buf, 0, buf.Length);
                                    bw.Write(buf);
                                }
                                bw.Flush();
                            }
                            tcpCnt.Close();
                        }
                    }
                    break;

                case CmdGetItemIds: {
                        CheckTcpListener();
                        var tcpCnt = TcpListener.AcceptTcpClient();
                        StringBuilder sb = new StringBuilder();
                        var bs = app.Dataset.BodyList;
                        for (int i = 0; i < bs.Count; i++) {
                            if (i > 0)
                                sb.Append('|');
                            sb.Append(bs[i].Id);
                        }
                        ReturnMsgTcp(tcpCnt, sb.ToString());
                    }
                    break;

                case CmdSelectItems: {
                        CheckTcpListener();
                        var tcpCnt = TcpListener.AcceptTcpClient();
                        int[] idxList = ReadIntArray(tcpCnt);
                        tcpCnt.Close();
                        var bsList = app.Dataset.BodyListEnabled();
                        List<string> selected = new List<string>();
                        foreach(int idx in idxList) {
                            if ( (idx>=0) && (idx<bsList.Count) ) {
                                selected.Add(bsList[idx].Id);
                            }
                        }
                        app.EventManager.RaiseItemsSelected(selected);
                    }
                    break;

                case CmdUpdateLabels: {
                        CheckTcpListener();
                        SendBackOK(sender);
                        var tcpCnt = TcpListener.AcceptTcpClient();
                        int[] labels = ReadIntArray(tcpCnt);

                        var bsList = app.Map.SelectedBodies;
                        if (bsList.Count == 0)
                            bsList = app.Dataset.BodyListEnabled();
                        app.GuiManager.RememberCurrentMap();
                        for(int i=0; i<labels.Length; i++) {
                            if (i < bsList.Count) {
                                bsList[i].Type = (short)labels[i];
                                bsList[i].Hidden = (bsList[i].Type == -1);
                            }
                        }
                        app.Map.Redraw();
                        tcpCnt.Close();
                    }
                    break;

                case CmdLoadLabels: {
                        CheckTcpListener();
                        SendBackOK(sender);
                        var tcpCnt = TcpListener.AcceptTcpClient();

                        var bsList = app.Map.SelectedBodies;
                        if (bsList.Count == 0)
                            bsList = app.Dataset.BodyListEnabled();
                        int[] labels = bsList.Select(b => (int) b.Type).ToArray();
                        WriteIntArray(tcpCnt, labels);
                        tcpCnt.Close();
                    }
                    break;

                case CmdLoadMap: {
                        CheckTcpListener();
                        SendBackOK(sender);
                        var tcpCnt = TcpListener.AcceptTcpClient();

                        var bsList = app.Map.SelectedBodies;
                        if (bsList.Count == 0)
                            bsList = app.Dataset.BodyListEnabled();
                        double[][] xyz = bsList.Select(b => new double[] { b.X, b.Y, b.Z }).ToArray();
                        using (var bw = new BinaryWriter(new BufferedStream(tcpCnt.GetStream()))) {
                            WriteMatrix(xyz, bw);
                            bw.Flush();
                        }
                        tcpCnt.Close();
                    }
                    break;

                case CmdSetColumnIds: {
                        int columns = System.BitConverter.ToInt32(data, 4);
                        var ds = app.Dataset;
                        CheckTcpListener();
                        SendBackOK(sender);

                        var tcpCnt = TcpListener.AcceptTcpClient();
                        string[] fs = ReadStringArray(tcpCnt);
                        tcpCnt.Close();

                        if ( (fs==null) ||(fs.Length != ds.Columns) ) {
                            VisuMap.MsgBox.Alert("Id list length mismatch: " + fs.Length + " != " + ds.Columns);
                            return ret;
                        }

                        var uf = new VisuMap.UniqueNameFinder();
                        for (int col = 0; col < fs.Length; col++)
                            if (col < ds.Columns)
                                ds.ColumnSpecList[col].Id = uf.LookupName(fs[col]);
                        ds.CommitChanges();
                    }
                    break;

                default:
                    ret = false;
                    break;
            }
            return ret;
        }

        public bool WriteIntArray(TcpClient tcp, int[] values) {
            var tcpStream = tcp.GetStream();
            using (var bw = new BinaryWriter(new BufferedStream(tcpStream))) {
                bw.Write(values.Length);
                bw.Flush();
                foreach (int v in values)
                    bw.Write(v);
                bw.Flush();
            }
            return true;
        }

        public int[] ReadIntArray(TcpClient tcp) {
            var tcpStream = tcp.GetStream();
            byte[] buf = ReadBytes(tcpStream, 4);
            int len = BitConverter.ToInt32(buf, 0);
            int[] idxList = new int[len];
            buf = new byte[len * 4];
            int remaining = buf.Length;
            while (remaining > 0) {
                int k = tcpStream.Read(buf, 0, remaining);
                if (k == 0)
                    throw new TimeoutException("Receiving data timeouted");
                remaining -= k;
            }
            Buffer.BlockCopy(buf, 0, idxList, 0, len * 4);
            return idxList;
        }

        public string[] ReadStringArray(TcpClient tcpCnt) {
            var tcpStream = tcpCnt.GetStream();
            byte[] buf = ReadBytes(tcpStream, 4);
            int toread = BitConverter.ToInt32(buf, 0);
            int readLen = 0;
            buf = new byte[toread];
            try {
                while (toread > 0) {
                    int len = tcpStream.Read(buf, readLen, buf.Length - readLen);
                    if (len <= 0)
                        break;
                    toread -= len;
                    readLen += readLen;
                }
                tcpStream.Dispose();
            } catch (Exception ex) {
                VisuMap.MsgBox.Alert("Failed to receive id list: " + ex.ToString() + ": " + ex.Message);
                return null;
            }
            if (toread > 0) {
                VisuMap.MsgBox.Alert("Failed to receive the complete id list.");
                return null;
            }

            string pkt = Encoding.UTF8.GetString(buf, 0, buf.Length);
            return pkt.Split('|');
        }

        /// <summary>
        /// Copy a given number of floats from source stream to destination stream.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public static void CopyStream(Stream src, Stream dst, int offset, int size) {
            int bytesSent = 0;
            int toSend = size * 4;           
            BinaryReader br = new BinaryReader(src);
            // skip offset. We cannot can stream.Seek() un compressed stream.
            int toskip = offset * 4;
            while (toskip > 0) {
                byte[] d = br.ReadBytes(Math.Min(toskip, 4 * 4096));
                toskip -= d.Length;
            }

            while (bytesSent < toSend) {
                byte[] d = br.ReadBytes(4 * 4096);
                if (d.Length == 0)
                    break;
                dst.Write(d, 0, d.Length);
                bytesSent += d.Length;
            }
            br.Close();
        }

        public void SendBackOK(IPEndPoint sender) {
            serverSkt.Send(BitConverter.GetBytes(CmdOK), 4, sender);
        }

        public void SendBackFail(IPEndPoint sender) {
            serverSkt.Send(BitConverter.GetBytes(CmdFail), 4, sender);
        }

        void DoShowMatrix(byte[] data, IPEndPoint sender) {
            int viewType = System.BitConverter.ToInt32(data, 4);
            int rows = System.BitConverter.ToInt32(data, 8);
            int columns = System.BitConverter.ToInt32(data, 12);
            int flags = System.BitConverter.ToInt32(data, 16);
            int viewIdx = System.BitConverter.ToInt32(data, 20);
            string[] fs = Encoding.UTF8.GetString(data, 24, data.Length - 24).Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            string winTitle = fs[0];
            string access = fs[1];
            short[] rowTypes = null;
            short[] rowFlags = null;
            CheckTcpListener();
            serverSkt.Send(BitConverter.GetBytes(CmdOK), 4, sender);
            TcpClient tcpCnt = tcpListener.AcceptTcpClient();
            tcpCnt.SendTimeout = tcpCnt.ReceiveTimeout = 10 * 1000;

            try {
                tcpCnt.Client.DontFragment = true;
                var tcpStream = tcpCnt.GetStream();

                if (viewType == 16) {
                    ShowBigBarView(tcpStream, rows * columns, access, viewIdx, winTitle);
                } else {
                    double[][] table = ReadMatrix(rows, columns, tcpStream);

                    if (flags == 1) {
                        byte[] buf = new byte[bufSize];
                        rowTypes = new short[rows];
                        rowFlags = new short[rows];
                        int row = 0;
                        while (row < rows) {
                            int len = tcpStream.Read(buf, 0, buf.Length);
                            if (len <= 0)
                                break;
                            for (int k = 0; k < len / 4; k++) {
                                int v = BitConverter.ToInt32(buf, k * 4);
                                rowTypes[row] = (short)(v & 0xFFFF);
                                rowFlags[row] = (short)(v >> 16);
                                row++;
                                if (row >= rows)
                                    break;
                            }
                        }
                    }
                    ShowTable(table, viewType, winTitle, rowTypes, rowFlags, access, viewIdx);
                }
                tcpCnt.Close();
            } catch ( Exception ex) {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public void CheckTcpListener() {
            if (tcpListener == null) {
                tcpListener = new TcpListener(IPAddress.Any, DataModeling.monitorPort);
                tcpListener.Start();
            }
        }

        public double[][] ReadMatrix(int rows, int columns, NetworkStream stream) {
            if ((rows == 0) && (columns == 0)) {
                byte[] cmdBuf = ReadBytes(stream, 8);
                rows = BitConverter.ToInt32(cmdBuf, 0);
                columns = BitConverter.ToInt32(cmdBuf, 4);
            }
            return ReadMatrix0(rows, columns, stream);
        }

        public static double[][] ReadMatrix0(int rows, int columns, NetworkStream stream) { 
            double[][] input = new double[rows][];
            int row = 0;
            int col = 0;
            for (row = 0; row < rows; row++)
                input[row] = new double[columns];
            row = col = 0;
            byte[] buf = new byte[bufSize];
            long remaining = 4 * (long) rows * columns;

            while (remaining > 0) {
                int len = stream.Read(buf, 0, (int) Math.Min(buf.Length, remaining));
                if (len == 0) 
                    throw new TimeoutException("Receiving data timeouted");
                    
                remaining -= len;
                for (int k = 0; k < len / 4; k++) {                    
                    input[row][col] = BitConverter.ToSingle(buf, k * 4);
                    col++;
                    if (col >= columns) {
                        col = 0;
                        row++;
                    }
                }
            }
            return input;
        }

        public static bool ReadFloatArray(Stream stream, float[] floatBuf) {
            int bufSize = floatBuf.Length * 4;
            const int BUF_SIZE = 1 * 1024 * 1024;
            byte[] buf = new byte[BUF_SIZE];
            int remaining = bufSize;
            int i0 = 0;

            while (i0 < bufSize) {
                int len = stream.Read(buf, 0, Math.Min(BUF_SIZE, bufSize - i0) );
                if (len == 0)
                    return false;
                Buffer.BlockCopy(buf, 0, floatBuf, i0, len);
                i0 += len;
            }
            return true;
        }

        byte[] ReadBytes(NetworkStream stream, int bytes) {
            byte[] cmdBuf = new byte[bytes];
            int len = 0;
            while (len < bytes)
                len += stream.Read(cmdBuf, len, bytes - len);
            return cmdBuf;
        }

        void DoTsne() {
            TcpClient tcpCnt = tcpListener.AcceptTcpClient();
            tcpCnt.Client.DontFragment = true;
            var tcpStream = tcpCnt.GetStream();

            byte[] cmdBuf = ReadBytes(tcpStream, 20);

            int epochs = BitConverter.ToInt32(cmdBuf, 0);
            int mapDim = BitConverter.ToInt32(cmdBuf, 4);
            double perpR = BitConverter.ToSingle(cmdBuf, 8);
            int rows = BitConverter.ToInt32(cmdBuf, 12);
            int columns = BitConverter.ToInt32(cmdBuf, 16);
            double[][] input = ReadMatrix(rows, columns, tcpStream);

            //
            // Calculate output...
            //
            var tsne = app.New.TsneMapCore(input);
            tsne.MaxLoops = epochs;
            tsne.OutDim = mapDim;
            tsne.PerplexityRatio = perpR;
            tsne.StartTraining();
            tsne.WaitForFinish(-1);
            var output = tsne.GetMapCoordinates();
            using (var bw = new BinaryWriter(new BufferedStream(tcpStream))) {
                WriteMatrix(output, bw);
            }
            tcpCnt.Close();
        }

        public void WriteMatrix(double[][] matrix, BinaryWriter bw) {            
            int rows = matrix.Length;
            int columns = (rows>0) ? matrix[0].Length : 0;
            bw.Write(rows);
            bw.Write(columns);
            bw.Flush();

            for (int row = 0; row < rows; row++) {
                for (int col = 0; col < columns; col++) {
                    bw.Write( (float)matrix[row][col] );
                }
            }
            bw.Flush();
        }

        public void WriteMatrix(float[][] matrix, BinaryWriter bw) {
            int rows = matrix.Length;
            int columns = (rows > 0) ? matrix[0].Length : 0;
            bw.Write(rows);
            bw.Write(columns);
            bw.Flush();
            byte[] buf = new byte[matrix[0].Length * 4];
            for (int row = 0; row < rows; row++) {
                Buffer.BlockCopy(matrix[row], 0, buf, 0, buf.Length);
                bw.Write(buf);
            }
            bw.Flush();
        }


        public void SaveTable(string groupName, string fileName) {
            string filePath = DataModeling.workDir + fileName;
            if (File.Exists(filePath))
                File.Delete(filePath);
            var app = DataModeling.App.ScriptApp;
            VisuMap.Script.IDataset dataset = app.Dataset;
            var nt = dataset.GetNumberTableEnabled();
            if (!string.IsNullOrEmpty(groupName) && (groupName != "*")) {
                var cIds = DataLink.ToColumnList(groupName, nt);
                nt = nt.SelectColumnsById(cIds);
            }
            DataLink.WriteMatrix0(filePath, (double[][]) nt.Matrix);
        }

        public void AddLogMessage(string msg) {
            OpenLogger();
            mainForm.Invoke(new MethodInvoker(delegate () {
                logger.AppendText(msg);
            }));
        }

        public void ReturnMsg(IPEndPoint sender, string msg) {
            try {                
                byte[] bufMsg = Encoding.UTF8.GetBytes(msg);
                byte[] lenBuf = BitConverter.GetBytes(bufMsg.Length);
                var buf = lenBuf.Concat(bufMsg).ToArray();
                serverSkt.Send(buf, buf.Length, sender);
            } catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        public void ReturnMsgTcp(TcpClient tcpCnt, string msg) {
            try {
                using (var bw = new BinaryWriter(new BufferedStream(tcpCnt.GetStream()))) {
                    bw.Write(msg.Length);
                    bw.Write(Encoding.UTF8.GetBytes(msg));
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        void OpenLogger() {
            if ((logger == null) || (logger.TheForm.IsDisposed)) {
                var fs = app.FindFormList("InfoPad");
                if (fs.Count > 0) {
                    logger = fs[0] as IInfoPad;
                } else {
                    mainForm.Invoke(new MethodInvoker(delegate () {
                        logger = app.New.InfoPad();
                        logger.Show();
                    }));
                }
            }
        }
        const int MAX_VIEWS = 12;
        IHeatMap[] hmViewList = new IHeatMap[MAX_VIEWS];
        IValueDiagram[] vdViewList = new IValueDiagram[MAX_VIEWS];
        IPcaView[] pcaViewList = new IPcaView[MAX_VIEWS];
        IBarBand[] barBandViewList = new IBarBand[MAX_VIEWS];
        IXyPlot[] xyViewList = new IXyPlot[MAX_VIEWS];
        ISpectrumBand[] spBndViewList = new ISpectrumBand[MAX_VIEWS];
        IMdsCluster[] mdsViewList = new IMdsCluster[MAX_VIEWS];
        IMdsSorter[] sortViewList = new IMdsSorter[MAX_VIEWS];
        IMountainView[] mvViewList = new IMountainView[MAX_VIEWS];
        IDiagramGrid[] dgViewList = new IDiagramGrid[MAX_VIEWS];
        IMapSnapshot[] map2DViewList = new IMapSnapshot[MAX_VIEWS];
        IMap3DView[] map3DViewList = new IMap3DView[MAX_VIEWS];
        IBarView[] barViewList = new IBarView[MAX_VIEWS];
        ISpectrumView[] spectrumViewList = new ISpectrumView[MAX_VIEWS];
        IBigBarView[] bigbarViewList = new IBigBarView[MAX_VIEWS];
        dynamic mapRecorder = null;

        bool NeedNewView(IForm preView, string access) {
            return (access == "n") || (preView == null) || preView.TheForm.IsDisposed;
        }

        delegate IForm CreateView();
        IForm GetPreView (IForm[] preViewList, int viewIdx, string access, INumberTable nt, CreateView newView){
            IForm preView = preViewList[viewIdx];
            if (NeedNewView(preView, access)) {
                preViewList[viewIdx] = newView();
                return preViewList[viewIdx];
            }

            var preNt = (preView as IExportNumberTable).GetNumberTable();
            if ((access == "a") && (preNt.Columns == nt.Columns)) {
                preNt.Append(nt);
            } else { // access == "r"
                preNt.Copy(nt);
            }
            return preView;
        }

        void ShowBigBarView(NetworkStream stream, int length, string access, int viewIdx, string winTitle=null) {
            if ((viewIdx < 0) || (viewIdx >= MAX_VIEWS))
                return;

            IBigBarView bbView = bigbarViewList[viewIdx];
            if ( (access == "n") || (bbView==null) || bbView.TheForm.IsDisposed ) {
                float[] values = new float[length];
                if (!ReadFloatArray(stream, values))
                    return;
                mainForm.Invoke(new MethodInvoker(delegate () {
                    bbView = app.New.BigBarView(values);
                    if (!string.IsNullOrEmpty(winTitle))
                        bbView.Title = winTitle;
                    bbView.Show();
                }));
                bigbarViewList[viewIdx] = bbView;
            } else {
                if (bbView.Values.Length != length)
                    bbView.Values = new float[length];
                if (!ReadFloatArray(stream, bbView.Values))
                    return;
                bbView.ValuesChanged = true;
                mainForm.Invoke(new MethodInvoker(delegate () {
                    if (!string.IsNullOrEmpty(winTitle))
                        bbView.Title = winTitle;
                    bbView.Redraw();
                    bbView.Refresh();
                }));
            }
        }

        void ShowTable(double[][] matrix, int viewType = 0, string winTitle = "", short[] rowTypes = null, short[] rowFlags = null, string access = "n", int viewIdx = 0) {
            var nt = app.New.NumberTable(matrix);
            if (nt == null) {
                MessageBox.Show("No data available: empty table.");
                return;
            }

            if (rowTypes != null) {
                for (int row = 0; row < nt.Rows; row++) {
                    if (row >= rowTypes.Length) break;
                    nt.RowSpecList[row].Type = rowTypes[row];
                }
            }

            if (rowFlags != null) {
                for (int row = 0; row < nt.Rows; row++) {
                    if (row >= rowTypes.Length) break;
                    short flag = rowFlags[row];
                    var rs = nt.RowSpecList[row];
                    rs.ShowId = ((flag&1) != 0);
                    rs.ShowName = ((flag&2) != 0);
                    rs.Hidden = ((flag&4) != 0);
                    rs.Highlighted = ((flag&8) != 0);
                    rs.Disabled = ((flag&16) != 0);
                }
            }

            if ((rowTypes == null) && (rowFlags == null) && (app.Dataset != null)) {                
                
                IForm activeFrm = app.LastView;
                bool typeSet = false;
                if ((activeFrm != null) && (!activeFrm.TheForm.IsDisposed) && (activeFrm is IExportNumberTable)) {
                    var nt2 = (activeFrm as IExportNumberTable).GetSelectedNumberTable();
                    if( (nt2!=null) && (nt2.Rows == nt.Rows)) { 
                        for (int row = 0; row < nt.Rows; row++) 
                            nt.RowSpecList[row].CopyFrom(nt2.RowSpecList[row]);
                        typeSet = true;
                    }
                }

                if (!typeSet) {
                    IList<IBody> bList = app.Dataset.BodyListEnabled();
                    if (nt.Rows == bList.Count) {
                        for (int row = 0; row < nt.Rows; row++)
                            nt.RowSpecList[row].CopyFromBody(bList[row]);
                    } else {
                        bList = app.Map.SelectedBodies;
                        if (nt.Rows == bList.Count) {
                            for (int row = 0; row < nt.Rows; row++)
                                nt.RowSpecList[row].CopyFromBody(bList[row]);
                        }
                    }
                }
            }

            mainForm.Invoke(new MethodInvoker(delegate () {
                UpdateView(nt, viewType, winTitle, access, viewIdx);
            }));
        }

        void UpdateView(INumberTable nt, int viewType, string winTitle, string access, int viewIdx) {
            if( (viewIdx<0) || (viewIdx >= MAX_VIEWS) ) {
                MessageBox.Show("View index too large (> " + MAX_VIEWS + "): " + viewIdx);
                return ;
            } 

            switch (viewType) {
                case 0:
                    var preHm = GetPreView(hmViewList, viewIdx, access, nt, nt.ShowHeatMap) as IHeatMap;
                    preHm.CentralizeColorSpectrum();
                    preHm.Title = winTitle;
                    preHm.Redraw();
                    break;
                case 1:
                    var preVd = GetPreView(vdViewList, viewIdx, access, nt, nt.ShowValueDiagram) as IValueDiagram;
                    preVd.Title = winTitle;
                    preVd.Redraw();
                    break;
                case 2:
                    var pcaView = GetPreView(pcaViewList, viewIdx, access, nt, nt.ShowPcaView) as IPcaView;
                    pcaView.Redraw();
                    pcaView.ResetView();
                    pcaView.Title = winTitle;
                    break;
                case 3:
                    var barBand = GetPreView(barBandViewList, viewIdx, access, nt, nt.ShowAsBarBand) as IBarBand;
                    barBand.Title = winTitle;
                    barBand.Redraw();
                    break;
                case 4:
                    var preXy = GetPreView(xyViewList, viewIdx, access, nt, () => app.New.XyPlot(nt).Show()) as IXyPlot;
                    preXy.Title = winTitle;
                    preXy.Redraw();
                    break;
                case 5: // spectrum band
                    var preSpBnd = GetPreView(spBndViewList, viewIdx, access, nt, () => app.New.SpectrumBand(nt).Show()) as ISpectrumBand;
                    preSpBnd.Title = winTitle;
                    preSpBnd.Redraw();
                    break;
                case 6: // mount view
                    var preMv = GetPreView(mvViewList, viewIdx, access, nt, () => app.New.MountainView(nt).Show()) as IMountainView;
                    preMv.Title = winTitle;
                    preMv.Redraw();
                    break;
                case 7: // digram grid
                    var preDg = GetPreView(dgViewList, viewIdx, access, nt, ()=>app.New.DiagramGrid(nt).Show()) as IDiagramGrid;
                    preDg.Title = winTitle;
                    preDg.Redraw();
                    break;
                case 8: // map rows
                    var preMds = GetPreView(mdsViewList, viewIdx, access, nt, ()=> app.New.MdsCluster(nt).Show()) as IMdsCluster;
                    preMds.Title = winTitle;
                    break;
                case 9: // map colums
                    nt = nt.Transpose2();
                    var preMds2 = GetPreView(mdsViewList, viewIdx, access, nt, () => app.New.MdsCluster(nt).Show());
                    preMds2.Title = winTitle;
                    break;
                case 10: // sort rows
                    var preSort = GetPreView(sortViewList, viewIdx, access, nt, ()=>app.New.MdsSorter(nt, false).Show());
                    preSort.Title = winTitle;
                    break;
                case 11: // sort columns
                    var preSort2 = GetPreView(sortViewList, viewIdx, access, nt, () => app.New.MdsSorter(nt, true).Show());
                    preSort2.Title = winTitle;
                    break;
                case 12:
                    var bs = ToBodyList(nt);
                    var preMap2D = map2DViewList[viewIdx];
                    if (NeedNewView(preMap2D, access)) {
                        preMap2D = app.New.MapSnapshot(bs);
                        if (app.Map != null)
                            preMap2D.GlyphSet = app.Map.GlyphType;
                        preMap2D.Show();
                        map2DViewList[viewIdx] = preMap2D;
                    } else {
                        var mbList = preMap2D.BodyList;
                        if (access == "r") mbList.Clear();
                        foreach (var b in bs) mbList.Add(b);
                        preMap2D.RedrawAll();
                    }
                    preMap2D.Title = winTitle;
                    break;
                case 13:
                    bs = ToBodyList(nt);
                    var preMap3D = map3DViewList[viewIdx];
                    if (NeedNewView(preMap3D, access)) {
                        preMap3D = app.New.Map3DView(bs, null);
                        preMap3D.Show();
                        map3DViewList[viewIdx] = preMap3D;
                    } else {
                        var mbList = preMap3D.BodyList;
                        if ((access == "r") || (access == "R")) {
                            mbList.Clear();
                        }
                        foreach (var b in bs) mbList.Add(b);
                        preMap3D.RedrawAll();
                    }
                    if (access == "R") {
                        if ( (mapRecorder == null) || (mapRecorder.IsDisposed) ) {
                            dynamic cr = app.FindPluginObject("ClipRecorder");
                            if (cr == null) {
                                MessageBox.Show("ClipRecorder plugin not installed!");
                                return;
                            }
                            mapRecorder = cr.OpenRecorder(preMap3D);
                        }
                        mapRecorder.AddSnapshot(preMap3D.BodyList);
                    }
                    preMap3D.Title = winTitle;
                    break;

                case 14: // bar view
                    var preBarView = barViewList[viewIdx];
                    if (NeedNewView(preBarView, access)) {
                        preBarView = app.New.BarView(nt);
                        preBarView.Title = winTitle;
                        preBarView.Show();
                        barViewList[viewIdx] = preBarView;
                    } else {
                        if (access == "r") {
                            preBarView.ItemList.Clear();
                            preBarView.Title = winTitle;
                        }
                        var csList = nt.ColumnSpecList;
                        var R = nt.Matrix[0];
                        for (int col = 0; col < nt.Columns; col++) {
                            preBarView.ItemList.Add(new VisuMap.Lib.ValueItem(csList[col].Id, csList[col].Name, R[col]));
                        }
                    }
                    preBarView.Redraw();
                    break;

                case 15: // spectrum view
                    var preSpectrumView = spectrumViewList[viewIdx];
                    if (NeedNewView(preSpectrumView, access)) {
                        preSpectrumView = app.New.SpectrumView(nt);
                        preSpectrumView.Title = winTitle;
                        preSpectrumView.Show();
                        spectrumViewList[viewIdx] = preSpectrumView;
                    } else {
                        if (access == "r") {
                            preSpectrumView.ItemList.Clear();
                            preSpectrumView.Title = winTitle;
                        }
                        var csList = nt.ColumnSpecList;
                        var R = nt.Matrix[0];
                        for (int col = 0; col < nt.Columns; col++) {
                            preSpectrumView.ItemList.Add(new VisuMap.Lib.ValueItem(csList[col].Id, csList[col].Name, R[col]));
                        }
                    }
                    preSpectrumView.Redraw();
                    break;

                case 17: // The main view map:
                    app.GuiManager.RememberCurrentMap();
                    var bList = app.Dataset.BodyListEnabled();
                    for (int i=0; i<bList.Count; i++) {
                        if ( i < nt.Rows ) {
                            bList[i].X = nt.Matrix[i][0];
                            if ( nt.Columns>=2 )
                                bList[i].Y = nt.Matrix[i][1];
                            if (nt.Columns >= 3)
                                bList[i].Z = nt.Matrix[i][2];
                        }
                    }
                    if (!string.IsNullOrEmpty(winTitle))
                        app.Map.Description = winTitle;
                    app.Folder.DataChanged = true;
                    app.Map.RedrawAll();
                    break;
            }
        }

        IList<IBody> ToBodyList(INumberTable tb) {
            var bs = app.New.BodyList();
            bool has2D = (tb.Columns > 1);
            bool has3D = (tb.Columns > 2);
            for (int row = 0; row < tb.Rows; row++) {
                var b = app.New.Body(tb.RowSpecList[row]);
                var R = tb.Matrix[row];
                b.X = R[0];
                b.Y = has2D ? R[1] : 0;
                b.Z = has3D ? R[2] : 0;
                bs.Add(b);
            }
            return bs;
        }
    }

    // Help class to calculate distance matrix with GPU.
    class GpuMetric : IMetric2 {
        float[][] dMatrix;
        public GpuMetric(IMetric2 mtr, int[] bsIndexes) {
            VectorMetric vMtr = ((Metric2Imp)mtr).Metric as VectorMetric;
            if (vMtr != null) {
                double[][] matrix = new double[bsIndexes.Length][];
                for (int i = 0; i < bsIndexes.Length; i++)
                    matrix[i] = vMtr.NumTable.Matrix[bsIndexes[i]];

                if (mtr.Name == NameResource.mtrEuclidean) {
                    dMatrix = FastDistance.EuclideanMatrix(matrix);
                } else if (mtr.Name == NameResource.mtrCorrelation) {
                    //Notice: matrix is already normalized in CorrelationMetric.NormalizeRows()
                    dMatrix = FastDistance.DotProduct(matrix);
                    MT.Loop(1, matrix.Length, row => {
                        for (int col = 0; col < row; col++)
                            dMatrix[row][col] = 1 - dMatrix[row][col];
                    });
                }
            }
        }

        public double Distance(int index1, int index2) {
            return (index1 == index2) ? 0 : (index2 < index1) ? dMatrix[index1][index2] : dMatrix[index2][index1];
        }

        public string Name { get => null; }
        public string FilterName { get => null; }
        public VisuMap.Script.IFilter Filter { get => null; }
        public double Distance(string id1, string id2) { return 0; }
        public void ReInitialize() { }
        public double[][] Distance2(IList<string> idList1, IList<string> idList2) { return null; }
        public double[][] Distance2(IList<IBody> bodyList1, IList<IBody> bodyList2) { return null; }
    }

}
