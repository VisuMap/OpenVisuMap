// Copyright (C) VisuMap Technologies Inc. 2020
using System.Runtime.InteropServices;
using System.Reflection;
using IDisposable = System.IDisposable;
using Array = System.Array;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;

namespace TsneDx {
    public class GpuDevice : IDisposable {
        Device device;
        DeviceContext ctx;

        public DeviceContext Context {
            get { return ctx; }
        }

        public GpuDevice() {
            device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
            ctx = device.ImmediateContext;
        }

        public GpuDevice(Device device, DeviceContext ctx) {
            this.device = device;
            this.ctx = ctx;
        }

        public void Run(int groupNumber=1) {
            ctx.Dispatch(groupNumber, 1, 1);
            ctx.Flush();
        }

        public void Dispose() {
            if (ctx != null) ctx.Dispose();
            if (device != null) device.Dispose();
        }

        #region Buffer creation.

        public class ConstBuffer<T> : System.IDisposable where T : struct {
            public Buffer buffer;
            GpuDevice gpu;
            public T c;
            public void Dispose() {
                if (buffer != null) {
                    buffer.Dispose();
                    buffer = null;
                }
            }

            public void Upload() {
                gpu.Context.UpdateSubresource(ref c, buffer);
            }

            public ConstBuffer(GpuDevice gpu, int slot) {
                buffer = new Buffer(gpu.device, (Marshal.SizeOf(typeof(T)) / 16 + 1) * 16,
                    ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
                gpu.Context.ComputeShader.SetConstantBuffer(slot, buffer);
                this.gpu = gpu;
            }
        }

        public Buffer CreateBufferRO<T>(T[] buffer, int slot, string shaders = "C") where T : struct {
            Buffer buf = CreateBufferRO(buffer.Length, Marshal.SizeOf(typeof(T)), slot, shaders);
            ctx.UpdateSubresource(buffer, buf, 0);
            return buf;
        }

        public Buffer CreateBufferRO(int elements, int elementSize, int slot, string shaders="C") {
            if (elementSize == 1) { // DirectX doesn't like non-4-byte element size, we here just padd the element size to 4.
                elementSize = 4;
                elements = (elements + 3) / 4;
            }
            Buffer buf = new Buffer(device, new BufferDescription
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                SizeInBytes = elements * elementSize,
                StructureByteStride = elementSize
            });

            var srvDesc = new ShaderResourceViewDescription();
            srvDesc.Format = SharpDX.DXGI.Format.Unknown;
            srvDesc.Buffer.ElementCount = elements;
            srvDesc.Buffer.ElementWidth = elements;
            srvDesc.Dimension = ShaderResourceViewDimension.Buffer;

            using (var srv = new ShaderResourceView(device, buf, srvDesc)) {
                if (shaders.Contains("C")) {
                    ctx.ComputeShader.SetShaderResource(slot, srv);
                }
                if (shaders.Contains("V")) {
                    ctx.VertexShader.SetShaderResource(slot, srv);
                }
                if (shaders.Contains("G")) {
                    ctx.GeometryShader.SetShaderResource(slot, srv);
                }
                if (shaders.Contains("P")) {
                    ctx.PixelShader.SetShaderResource(slot, srv);
                }
            }
            return buf;
        }      

        public Buffer CreateBufferRW(int elements, int elementSize, int slot) {
            Buffer buf = new Buffer(device, new BufferDescription
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                SizeInBytes = elements * elementSize,
                StructureByteStride = elementSize
            });
            var uavDesc = new UnorderedAccessViewDescription();
            uavDesc.Dimension = UnorderedAccessViewDimension.Buffer;
            uavDesc.Format = SharpDX.DXGI.Format.Unknown;
            uavDesc.Buffer.ElementCount = elements;

            using (var uav = new UnorderedAccessView(device, buf, uavDesc)) {
                ctx.ComputeShader.SetUnorderedAccessView(slot, uav);
            }
            return buf;
        }

        public ConstBuffer<T> CreateConstantBuffer<T>(int slot) where T : struct {
            return new ConstBuffer<T>(this, slot);
        }

        public Buffer CreateStagingBuffer(Buffer buf) {
            int elementSize = buf.Description.StructureByteStride;
            if (elementSize == 0) elementSize = 4;  // An index buffer always has StructuredByteStrid == 0.
            int elements = buf.Description.SizeInBytes / elementSize;
            return new Buffer(device, new BufferDescription() {
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = elementSize,
                BindFlags = BindFlags.None,
                Usage = ResourceUsage.Staging,
                SizeInBytes = elements * elementSize,
                CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write
            });
        }

        public ConstBuffer<T> CreateConstBuffer<T>(int slot) where T : struct {
            return new ConstBuffer<T>(this, slot);
        }

        #endregion

        #region Read & Write API

        public class ReadDataStream : System.IDisposable {
            DataStream ds;
            Buffer stagingBuffer;
            GpuDevice device;

            public ReadDataStream(GpuDevice device, Buffer dataBuffer, Buffer stagingBuffer) {
                this.device = device;
                this.stagingBuffer = stagingBuffer;
                device.Context.CopyResource(dataBuffer, stagingBuffer);
                device.Context.MapSubresource(stagingBuffer, 0, SharpDX.Direct3D11.MapMode.Read, MapFlags.None, out ds);
                
            }

            public void Reset() {
                ds.Seek(0, System.IO.SeekOrigin.Begin);
            }

            public T Read<T>() where T : struct {
                return ds.Read<T>();
            }

            public int ReadRange<T>(T[] buffer, int offset, int count) where T : struct {
                return ds.ReadRange<T>(buffer, offset, count);
            }

            public void Dispose() {
                device.Context.UnmapSubresource(stagingBuffer, 0);
            }

            public DataStream Stream {
                get { return ds; }
            }
        }

        public class WriteDataStream : DataStream, System.IDisposable {
            DeviceContext ctx;
            Buffer buffer;
            public WriteDataStream(Buffer buffer, DeviceContext ctx) :
                base(buffer.Description.SizeInBytes, true, true) {
                this.ctx = ctx;
                this.buffer = buffer;
            }

            public void Write<T>(params T[] values) where T : struct {
                WriteRange(values);
            }

            public new void Dispose() {
                this.Position = 0;
                var db = new DataBox(DataPointer, 8, (int)this.Length);                
                ctx.UpdateSubresource(db, buffer, 0);
                base.Dispose();
            }
        }

        public void WriteMarix(Buffer buffer, float[][] matrix, bool transpose=false)  {
            using (var ws = NewWriteStream(buffer)) {
                int rows = matrix.Length;
                int columns = matrix[0].Length;
                float[] buf = new float[rows * columns];
                if (transpose) {
                    int idx = 0;
                    for (int col = 0; col < columns; col++)
                        for (int row = 0; row < rows; row++)
                            buf[idx++] = matrix[row][col];
                } else {
                    for (int row = 0; row < rows; row++)
                        Array.Copy(matrix[row], 0, buf, row*columns, columns);
                }
                ws.WriteRange(buf);
            }
        }

        public WriteDataStream NewWriteStream(Buffer buffer) {
            return new WriteDataStream(buffer, ctx);
        }

        public ReadDataStream NewReadStream(Buffer stagingBuffer, Buffer dataBuffer) {
            return new ReadDataStream(this, dataBuffer, stagingBuffer);
        }

        public float ReadFloat(Buffer stagingBuffer, Buffer dataBuffer) {
            float[] buf = new float[1];
            ctx.CopyResource(dataBuffer, stagingBuffer);
            var db = ctx.MapSubresource(stagingBuffer, 0, SharpDX.Direct3D11.MapMode.Read, MapFlags.None);
            Marshal.Copy(db.DataPointer, buf, 0, 1);           
            ctx.UnmapSubresource(stagingBuffer, 0);
            return buf[0];
        }

        public int ReadInt(Buffer stagingBuffer, Buffer dataBuffer) {
            int[] buf = new int[1];
            ctx.CopyResource(dataBuffer, stagingBuffer);
            var db = ctx.MapSubresource(stagingBuffer, 0, SharpDX.Direct3D11.MapMode.Read, MapFlags.None);
            Marshal.Copy(db.DataPointer, buf, 0, 1);
            ctx.UnmapSubresource(stagingBuffer, 0);
            return buf[0];
        }

        public T[] ReadRange<T>(Buffer stagingBuffer, Buffer dataBuffer, int count) where T : struct {
            return ReadRange<T>(new T[count], 0, stagingBuffer, dataBuffer, count);
        }

        public T[] ReadRange<T>(T[] values, int offset, Buffer stagingBuffer, Buffer dataBuffer, int count) where T : struct {
            using (var rs = NewReadStream(stagingBuffer, dataBuffer))
                rs.ReadRange(values, offset, count);
            return values;
        }

        public T[] ReadRange<T>(Buffer dataBuffer, int count=0) where T : struct {
            if (count == 0)
                count = dataBuffer.Description.SizeInBytes / dataBuffer.Description.StructureByteStride;
            T[] values = new T[count];
            using(var staging = CreateStagingBuffer(dataBuffer))
            using(var rs = NewReadStream(staging, dataBuffer))
                rs.ReadRange(values, 0, count);
            return values;
        }

        #endregion

        #region help methods.
        public ComputeShader LoadShader(string shaderPath, Assembly asm=null) {
            if (asm == null)
                asm = Assembly.GetExecutingAssembly();
            using (System.IO.Stream s = asm.GetManifestResourceStream(shaderPath)) {
                if (s == null)
                    throw new System.IO.FileNotFoundException("Cannot find shader resource " + shaderPath);
                byte[] buf = new byte[s.Length];
                s.Read(buf, 0, buf.Length);
                return new ComputeShader(device, buf);
            }
        }

        public void SetShader(ComputeShader shader) {
            ctx.ComputeShader.Set(shader);
        }
        #endregion
    }
    
}
