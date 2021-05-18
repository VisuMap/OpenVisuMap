using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

using VisuMap.Script;

namespace VisuMap.DataModeling {
    public class LiveModel : IDisposable {
        UdpClient skt;  // the command UPD socket.

        int inputDim;
        int outputDim;
        int labelDim;
        int portNumber;
        string serverName = "localhost";
        string modelName;
        Process cmdProc;
        DataLink mdInfo;
        string lastError = "";
        int requestTimeout = 60*1000; // in milliseconds

        const int CmdEval = 100;
        const int CmdShutdown = 101;
        const int CmdModelInfo = 102;
        const int CmdExec = 103;
        const int CmdReadVariable = 104;
        const int CmdEvalVariable = 105;
        const int CmdShowGraph = 106;
        const int CmdListWeights = 107;
        const int CmdWriteVariable = 108;
        const int CmdListOperations = 109;
        const int CmdTracing = 110;
        const int CmdUploadMatrix = 111;
        const int CmdEvalVariable2 = 112;
        const int CmdAug2Var = 113;
        const int CmdInAug2Var = 114;
        const int CmdReadString = 115;
        const int CmdSetInput = 116;

        const int CmdSuccess = 99;
        const int CmdFail = 98;

        const int CmdCustomJob = 200;
        string commandLine = "";

        public LiveModel(string modelName = null, int serverPort = 0, string serverName=null) {
            this.modelName = modelName;
            if (serverPort != 0)
                portNumber = serverPort;
            if (! string.IsNullOrEmpty(serverName) ) 
                this.serverName = serverName;
            if (!string.IsNullOrEmpty(modelName)) {
                mdInfo = new DataLink(DataModeling.workDir + modelName + ".md");
            }
        }

        #region properties
        public int InputDimension { get => inputDim; set => inputDim = value; }
        public int OutputDimension { get => outputDim; set => outputDim = value; }
        public int LabelDimension { get => labelDim; set => labelDim = value; }
        public string LastError { get => lastError; set => lastError = value; }
        public int PortNumber { get => portNumber; set => portNumber = value; }
        public string ServerName { get => serverName; set => serverName = value; }

        public DataLink ModelInfo { get => mdInfo; }
        public string ModelName { get => modelName; set => modelName = value; }

        // caching objects for data or function objects.        
        public object X { get; set; }
        public object Y { get; set; }
        public object Z { get; set; }
        public int RequestTimeout { get => requestTimeout; set => requestTimeout = value; }
        #endregion

        #region local methods.
        int GetOutputDimension() {
            int clr = 0;
            string tgt = mdInfo.OutputLabel;

            if (tgt == null)
                return mdInfo.OutputVariables.Count;

            if ((tgt == DataLink.Var2DPosClr) || (tgt == DataLink.Var3DPosClr) || (tgt == DataLink.VarColor)) {
                clr = mdInfo.Type2Index.Count;
            }

            switch (tgt) {
                case DataLink.Var2DPos:
                    return 2;
                case DataLink.Var3DPos:
                    return 3;
                case DataLink.Var3DPosClr:
                    return 3 + clr;
                case DataLink.Var2DPosClr:
                    return 2 + clr;
                case DataLink.VarColor:
                    return clr;
                case DataLink.ColumnList:
                    return mdInfo.OutputVariables.Count;
            }

            return 0;
        }

        //Notice that the second argument is a reader, this becouse there is no 
        //bi-directional binary connection object like the tcp connection.
        void WriteMatrix(double[][] matrix, BinaryReader br, bool applyScaling) {
            Stream tcpStream = br.BaseStream;
            int columns = matrix[0].Length;
            int rows = matrix.Length;
            int row = 0;
            int col = 0;

            byte[] cmdBuf = new byte[8];
            Array.Copy(BitConverter.GetBytes(rows), cmdBuf, 4);
            Array.Copy(BitConverter.GetBytes(columns), 0, cmdBuf, 4, 4);
            tcpStream.Write(cmdBuf, 0, 8);
            tcpStream.Flush();

            int scaleDim = (mdInfo.InputShifts == null) ? 0 : mdInfo.InputShifts.Count;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms)) {
                for (row = 0; row < rows; row++) {
                    for (col = 0; col < columns; col++) {
                        double v = matrix[row][col];
                        if (applyScaling && (col < scaleDim))
                            v = (v - mdInfo.InputShifts[col]) * mdInfo.InputFactors[col];
                        bw.Write((float)v);
                    }
                }
                bw.Flush();
                tcpStream.Write(ms.GetBuffer(), 0, (int)bw.BaseStream.Length);
                tcpStream.Flush();
            }
        }

        double[][] ReadMatrix(BinaryReader br) {
            int rows = br.ReadInt32();
            int columns = br.ReadInt32();

            if (rows * columns == 0) return null;

            return CmdServer.ReadMatrix0(rows, columns, br.BaseStream as NetworkStream);
        }

        bool IsResponseOK() {
            byte[] resp = GetResponse();
            if ((resp == null) || (resp.Length < 4) || (System.BitConverter.ToInt32(resp, 0) != CmdSuccess))
                return false;
            return true;
        }

        NetworkStream GetTcpStream() {
            var sktCnt = new TcpClient();
            sktCnt.Connect(serverName, portNumber);
            sktCnt.ReceiveTimeout = sktCnt.SendTimeout = requestTimeout;
            return sktCnt.GetStream();
        }

        BinaryReader GetTcpBinaryReader() {
            return new BinaryReader(GetTcpStream(), Encoding.UTF8, true);
        }

        void SendCmd(IEnumerable<byte> v) {
            skt.Send(v.ToArray(), v.Count());
        }
        static byte[] BB(int v) { return BitConverter.GetBytes(v); }
        #endregion

        #region Exported methods.
        public LiveModel StartModel(bool conditional = true) {
            // don't do anything if already started.
            if (conditional) {
                // check whether the server port is created.
                if (IsServerRunning())
                    return this;
            }
            cmdProc = ScriptUtil.StartCmd(DataModeling.pythonProgamm, "\"" + DataModeling.homeDir + "ModelServer.py\" " + modelName + " " + portNumber, true);
            commandLine = DataModeling.pythonProgamm + " \"" + DataModeling.homeDir + "ModelServer.py\" " + modelName + " " + portNumber;
            for (int n = 0; n < 100; n++) {
                Thread.Sleep(2000);
                if (IsServerRunning()) {
                    break;
                }
            }
            return this;
        }

        public bool IsConnected
        {
            get { return (skt != null); }
        }

        public bool IsServerRunning() {
            try {
                var pp = new UdpClient(new IPEndPoint(IPAddress.Any, portNumber));
                pp.Close();
            } catch (SocketException) {
                return true;
            }
            return false;
        }

        public void Dispose() {
            if (skt != null) {
                skt.Close();
                skt = null;
            }
        }

        public LiveModel Connect() {
            skt = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            skt.Connect(serverName, portNumber);
            skt.DontFragment = true;
            skt.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, requestTimeout);
            IPEndPoint svrAddr = new IPEndPoint(IPAddress.Any, 0);

            SendCmd(BB(CmdModelInfo));
            byte[] resp = skt.Receive(ref svrAddr);
            int cmd = System.BitConverter.ToInt32(resp, 0);
            if (cmd != CmdSuccess) {
                throw new Exception("Failed to obtain model information");
            }
            inputDim = System.BitConverter.ToInt32(resp, 4);
            outputDim = System.BitConverter.ToInt32(resp, 8);
            labelDim = System.BitConverter.ToInt32(resp, 12);
            string targetModelName = Encoding.UTF8.GetString(resp, 16, resp.Length - 16);

            if (mdInfo == null) {
                modelName = targetModelName;
                mdInfo = new DataLink(DataModeling.workDir + modelName + ".md");
            } else {
                if (targetModelName != this.ModelName)
                    MessageBox.Show("Warning: different server model name: " + targetModelName + " != " + this.ModelName);
                if (inputDim != mdInfo.InputVariables?.Count) {
                    lastError = "Input dimension mismatch";
                    return this;
                }
                if (outputDim != GetOutputDimension()) {
                    lastError = "Output dimension mismatch";
                    return this;
                }
            }
            return this;
        }

        public bool ShutdownModel() {
            SendCmd(BB(CmdShutdown));
            Thread.Sleep(1200); // wait till the server has cleaned up.
            return true;
        }

        public bool Exec(string script) {
            SendCmd(BB(CmdExec).Add(script.Length).Add(script));
            return true;
        }

        public string CommandLine { get => commandLine; }

        public void MinimizeServerWindow() {
            if ( (cmdProc!=null) && (cmdProc.MainWindowHandle != System.IntPtr.Zero) )
                FeedforwardNetwork.ShowWindowAsync(cmdProc.MainWindowHandle, 2); // 1: normal; 2: minimized; 3: maximized.
        }

        public double[][] Eval(double[][] input, bool applyScaling = true, IList<IBody> bodyList = null) {
            bool preLoadedData = false;
            if (input == null) {
                preLoadedData = true;
            } else {
                if ((input.Length == 0) || (input[0].Length != inputDim)) {
                    if (input.Length == 0) {
                        throw new Exception("Invalid input data: zero rows.");
                    } else {
                        throw new Exception("Invalid input dimension: input dim " + input[0].Length + " != " + inputDim);
                    }
                }
            }
            SendCmd(BB(CmdEval).Add(preLoadedData?1:0));

            if (!IsResponseOK()) return null;

            double[][] output = null;
            //int t0 = (int)(DateTime.Now.Ticks / 10000);
            using (var tcpReader = GetTcpBinaryReader()) {
                if (!preLoadedData)
                    WriteMatrix(input, tcpReader, applyScaling);
                output = ReadMatrix(tcpReader);
            }

            if (!IsResponseOK()) return null;

            //int t1 = (int)(DateTime.Now.Ticks / 10000);
            //MessageBox.Show("Time: " + (t1 - t0));

            string outputLabel = mdInfo.OutputLabel;
            if (mdInfo.OutputFactors != null) {
                if (applyScaling) {
                    for (int row = 0; row < output.Length; row++) {
                        var R = output[row];
                        for (int col = 0; col < outputDim; col++) {
                            R[col] = R[col] / mdInfo.OutputFactors[col] + mdInfo.OutputShifts[col];
                        }
                    }
                }
            } else {
                int mapDim = ((outputLabel == DataLink.Var3DPos) || (outputLabel == DataLink.Var3DPosClr)) ? 3 :
                    ((outputLabel == DataLink.Var2DPos) || (outputLabel == DataLink.Var2DPosClr)) ? 2 : 0;

                if (mapDim > 0) {
                    if (applyScaling) {
                        if (bodyList != null) {
                            for (int row = 0; row < output.Length; row++) {
                                bodyList[row].X = (output[row][0] - DataLink.MapMargin) / mdInfo.MapFactor;
                                bodyList[row].Y = (output[row][1] - DataLink.MapMargin) / mdInfo.MapFactor;
                                bodyList[row].Z = (mapDim >= 3) ? (output[row][2] - DataLink.MapMargin) / mdInfo.MapFactor : 0;
                            }
                        } else {
                            for (int row = 0; row < output.Length; row++) {
                                for (int col = 0; col < mapDim; col++) {
                                    output[row][col] -= DataLink.MapMargin;
                                    output[row][col] /= mdInfo.MapFactor;
                                }
                            }
                        }
                    }
                }
                if (mdInfo.HasClassInfo) {
                    var index2Type = mdInfo.GetIndex2Type();
                    if (bodyList != null) {
                        for (int row = 0; row < output.Length; row++)
                            bodyList[row].Type = index2Type[DataLink.MaxIndex(output[row], mapDim) - mapDim];
                    } else {
                        for (int row = 0; row < output.Length; row++) {
                            output[row][mapDim] = index2Type[DataLink.MaxIndex(output[row], mapDim) - mapDim];
                            Array.Resize(ref output[row], mapDim + 1);
                        }
                    }
                }
            }

            return output;
        }

        public double[][] EvalVariable(double[][] input, string variableName, bool applyScaling = false) {
            return EvalVariable0(input, variableName, applyScaling, false);
        }
        public double[][] EvalVariable2(double[][] input, string variableName, bool applyScaling = false) {
            return EvalVariable0(input, variableName, applyScaling, true);
        }

        public double[][] EvalVariable0(double[][] input, string variableName, bool applyScaling = false, bool pushToLabel = false) {
            int preloadData = (input == null) ? 1 : 0;
            if (input != null) {
                if ( (input.Length == 0) || (input[0].Length != inputDim))
                    throw new Exception("Invalid input data!");
            }
            var cmd = pushToLabel ? CmdEvalVariable2 : CmdEvalVariable;
            SendCmd(BB(cmd).Add(preloadData).Add(variableName));
            if (!IsResponseOK())
                return null;

            using (BinaryReader tcpReader = GetTcpBinaryReader()) {
                if (input != null)
                    WriteMatrix(input, tcpReader, applyScaling);
                double[][] output = ReadMatrix(tcpReader);
                return output;
            }
        }

        public double[][] EvalInAug2Var(double[][] augment, string variableName) {
            SendCmd(BB(CmdInAug2Var).Add(variableName));
            if (!IsResponseOK())
                return null;
            using (BinaryReader tcpReader = GetTcpBinaryReader()) {
                WriteMatrix(augment, tcpReader, false);
                double[][] output = ReadMatrix(tcpReader);
                return output;
            }
        }

        public double[][] EvalAug2Var(double[][] augment, string variableName) {
            SendCmd(BB(CmdAug2Var).Add(variableName));
            if (!IsResponseOK())
                return null;

            using (BinaryReader tcpReader = GetTcpBinaryReader()) {
                WriteMatrix(augment, tcpReader, false);
                double[][] output = ReadMatrix(tcpReader);
                return output;
            }
        }

        public float[] EvalAug2VarAsFloat(double[][] augment, string variableName) {
            SendCmd(BB(CmdAug2Var).Add(variableName));
            if (!IsResponseOK())
                return null;

            float[] output = null;
            using (BinaryReader tcpReader = GetTcpBinaryReader()) {
                WriteMatrix(augment, tcpReader, false);
                int rows = tcpReader.ReadInt32();
                int columns = tcpReader.ReadInt32();
                output = new float[rows * columns];
                CmdServer.ReadFloatArray(tcpReader.BaseStream, output);
            }
            return output;
        }


        public double[][] ReadVariable(string variableName) {
            SendCmd(BB(CmdReadVariable).Add(variableName));
            if (!IsResponseOK())
                return null;

            using (var br = GetTcpBinaryReader()) {
                double[][] d = ReadMatrix(br);
                return d;
            }
        }

        public string ReadString(string variableName) {
            SendCmd(BB(CmdReadString).Add(variableName));
            byte[] resp = GetResponse();
            if ((resp == null) || (resp.Length < 4) || (System.BitConverter.ToInt32(resp, 0) != CmdSuccess))
                return null;
            string s = System.Text.Encoding.UTF8.GetString(resp, 4, resp.Length-4);
            return s;
        }

        public bool WriteVariable(string variableName, double[][] values) {
            SendCmd(BB(CmdWriteVariable).Add(variableName));
            if (!IsResponseOK()) return false;

            using (var br = GetTcpBinaryReader()) {
                WriteMatrix(values, br, false);
            }
            return true;
        }

        public bool WriteVariable(string variableName, IList<double> values) {
            SendCmd(BB(CmdWriteVariable).Add(variableName));
            if (!IsResponseOK()) return false;

            using (var br = GetTcpBinaryReader()) {
                WriteMatrix(new double[][] { values.ToArray() }, br, false);
            }
            return true;
        }


        public void ShowGraph() {
            SendCmd(BB(CmdShowGraph));
        }

        public IList<string> ListWeights() {
            SendCmd(BB(CmdListWeights));
            if (!IsResponseOK()) return null;
            using (var br = GetTcpBinaryReader()) {
                VariableListView vv = new VariableListView(this);
                List<string> wList = new List<string>();
                int count = br.ReadInt32();
                for (int i = 0; i < count; i++) {
                    wList.Add(br.ReadString());
                }
                return wList;
            }
        }

        public IList<string> ListOperations() {
            SendCmd(BB(CmdListOperations));
            if (!IsResponseOK()) return null;

            List<string> opList = new List<string>();
            using (var br = GetTcpBinaryReader()) {
                int count = br.ReadInt32();
                for (int i = 0; i < count; i++) {
                    opList.Add(br.ReadString());
                }
            }
            return opList;
        }

        public void SetTracing(bool enable) {
            SendCmd(BB(CmdTracing).Add(enable ? 1 : 0));
        }

        public bool UploadMatrix(double[][] input, bool scaling = false) {
            SendCmd(BB(CmdUploadMatrix));
            if (!IsResponseOK()) return false;
            using (var tcpReader = GetTcpBinaryReader()) {
                WriteMatrix(input, tcpReader, scaling);
            }
            return true;
        }

        public bool UploadBlob(string blobName, int rows, int columns, int offset) {
            IBlob blob = DataModeling.App.ScriptApp.Folder.OpenBlob(blobName, false);
            string[] fs = blob.ContentType.Split(' ');
            int blobSize = int.Parse(fs[2]); // in floats
            int dataSize = rows * columns;
            if (blobSize < (offset+dataSize))
                return false;

            SendCmd(BB(CmdUploadMatrix));
            if (!IsResponseOK()) return false;

            Stream tcpStream = GetTcpStream();
            byte[] cmdBuf = new byte[8];
            Array.Copy(BitConverter.GetBytes(rows), cmdBuf, 4);
            Array.Copy(BitConverter.GetBytes(columns), 0, cmdBuf, 4, 4);
            tcpStream.Write(cmdBuf, 0, 8);
            tcpStream.Flush();

            CmdServer.CopyStream(blob.Stream, tcpStream, offset, dataSize);
            blob.Close();
            tcpStream.Close();
            return true;
        }


        public bool ScaleInput(double[][] input) {
            if ( (input == null) ||(input.Length<1) || (mdInfo == null) )
                return false;
            if ((mdInfo.InputFactors == null) || (mdInfo.InputShifts == null))
                return false;
            int columns = input[0].Length;
            if ((mdInfo.InputFactors.Count != columns) || (mdInfo.InputShifts.Count != columns))
                return false;
            foreach(var R in input) {
                for(int col=0; col<columns; col++) {
                    R[col] = mdInfo.InputFactors[col] * (R[col] - mdInfo.InputShifts[col]);
                }
            }
            return true;
        }

        public object TestEvalInit(double[][] input, IMap3DView map) {
            SendCmd(BB(CmdCustomJob).Add(0));
            if (!IsResponseOK()) return null;

            var br = GetTcpBinaryReader();
            WriteMatrix(input, br, false);
            return br;
        }

        public void TestEval(double a, double b) {
            IMap3DView map = this.Y as IMap3DView;
            BinaryReader br = this.Z as BinaryReader;
            SendCmd(BB(CmdCustomJob).Add(1).Add((float)a).Add((float)b));

            string outputLabel = mdInfo.OutputLabel;
            int mapDim = ((outputLabel == DataLink.Var3DPos) || (outputLabel == DataLink.Var3DPosClr)) ? 3 :
                ((outputLabel == DataLink.Var2DPos) || (outputLabel == DataLink.Var2DPosClr)) ? 2 : 0;
            var bodyList = map.BodyList;
            double[][] output = ReadMatrix(br);
            for (int row = 0; row < output.Length; row++) {
                bodyList[row].X = (output[row][0] - DataLink.MapMargin) / mdInfo.MapFactor;
                bodyList[row].Y = (output[row][1] - DataLink.MapMargin) / mdInfo.MapFactor;
                bodyList[row].Z = (mapDim >= 3) ? (output[row][2] - DataLink.MapMargin) / mdInfo.MapFactor : 0;
            }
            map.Title = "Arg: " + a.ToString("g4") + ", " + b.ToString("g4");
            map.Redraw();
        }

        public void TestEvalClose() {
            BinaryReader br = this.Z as BinaryReader;
            if (br != null) br.Dispose();
            SendCmd(BB(CmdCustomJob).Add(2));
        }

        public byte[] GetResponse() {
            var svrAddr = new IPEndPoint(IPAddress.Any, 0);
            return skt.Receive(ref svrAddr);
        }

        public bool SetInput(string inputHolderName) {
            SendCmd(BB(CmdSetInput).Add(inputHolderName));
            byte[] resp = GetResponse();
            if ((resp == null) || (resp.Length < 8) || (System.BitConverter.ToInt32(resp, 0) != CmdSuccess))
                return false;
            inputDim = System.BitConverter.ToInt32(resp, 4);
            return true;
        }
        #endregion
    }

    public static class BytesExtension {
        public static IEnumerable<byte> Add(this IEnumerable<byte> s, int v) {
            return s.Concat(BitConverter.GetBytes(v));
        }
        public static IEnumerable<byte> Add(this IEnumerable<byte> s, float v) {
            return s.Concat(BitConverter.GetBytes(v));
        }
        public static IEnumerable<byte> Add(this IEnumerable<byte> s, string v) {
            return s.Concat(Encoding.UTF8.GetBytes(v));
        }
    }

}
