using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace VisuMap.GeneticAnalysis {

    /// <summary>
    /// Utility class to help implementing multithreading algorithm.
    /// </summary>
    public class ThreadInfo {
        int threadIndex;
        int siblings;     // number of total threads started, used in GetLoopInterval().
        AutoResetEvent resetEvent;
        double resultDouble;
        int resultInt;

        public int ResultInt {
            get { return resultInt; }
            set { resultInt = value; }
        }

        public double ResultDouble {
            get { return resultDouble; }
            set { resultDouble = value; }
        }

        public AutoResetEvent ResetEvent {
            get { return resetEvent; }
            set { resetEvent = value; }
        }

        public int ThreadIndex {
            get { return threadIndex; }
        }

        public int Siblings {
            get { return siblings; }
        }

        public void Finished(double resultDouble) {
            this.resultDouble = resultDouble;
            Finished();
        }

        public void Finished(double resultDouble, int resultInt) {
            this.resultDouble = resultDouble;
            this.resultInt = resultInt;
            Finished();
        }

        public void Finished() {
            if (resetEvent != null) {
                resetEvent.Set();
            }
        }

        public ThreadInfo(int threadIndex, int siblings) {
            this.threadIndex = threadIndex;
            this.siblings = siblings;
        }

        public LoopInterval GetLoopInterval(int loopSize) {
            if (siblings == 1) return new LoopInterval(0, loopSize);

            int stepSize = loopSize / siblings;
            if (loopSize % siblings != 0) {
                stepSize++;
            }
            return new LoopInterval(threadIndex * stepSize, Math.Min(loopSize, (threadIndex +1) * stepSize));
        }

        public static double TotalResultDouble(ThreadInfo[] infoList) {
            double total = 0.0;
            foreach (ThreadInfo info in infoList) {
                total += info.resultDouble;
            }
            return total;
        }

        public static int TotalResultInt(ThreadInfo[] infoList) {
            int total = 0;
            foreach (ThreadInfo info in infoList) {
                total += info.resultInt;
            }
            return total;
        }
        public static long TotalResultInt2(ThreadInfo[] infoList) {
            long total = 0;
            foreach (ThreadInfo info in infoList) {
                total += info.resultInt;
            }
            return total;
        }
    }

    
    /// <summary>
    /// Class to support pipelining style concurrency.
    /// </summary>
    public class Pipeline : IDisposable {
        int threads;  // maximal number of threads.
        Semaphore semaphore;  // semaphore to control the concurrency.

        /// <summary>
        /// Creates a pipeline with default number of threads.
        /// </summary>
        public Pipeline()
            : this(Multithreading.CfgWorkThreads) {
        }

        /// <summary>
        /// Creates a pipeline with given maximal number of threads.
        /// </summary>
        /// <param name="threads"></param>
        public Pipeline(int threads) {
            this.threads = threads;
            if (threads == 1) return;  // No concurrency.
            semaphore = new Semaphore(threads, threads);
        }

        public void Dispose() {
            if (threads == 1) return;

            // Wait till all threads have finished.
            for (int i = threads; i > 0; i--) {
                semaphore.WaitOne();
            }
            semaphore.Close();
        }

        public delegate void Proc<T>(T arg);

        public void Do<T>(T arg, Proc<T> proc) {
            if (threads == 1) {
                proc(arg);
                return;
            }

            // Suspends the caller when the pipeline is full.
            semaphore.WaitOne();

            
            /* For unknown reason the TreadPool version runs much slower than the direct Thread() call.
             * This can be seen with the method, MapGallery.LoadImages() with a folder in that the first 
             * dataset has over 100 maps, then followed by 50 datasets each with one map.
             *
            ThreadPool.QueueUserWorkItem(delegate(object par) {
                try { proc(arg); } finally { semaphore.Release(); }
            });            
            */           

            new Thread(delegate() {
                try { proc(arg); } finally { semaphore.Release(); }
            }).Start();
        }

        /// <summary>
        /// Auxilary method to simplify coding.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="proc"></param>
        public static void ForEach<T>(IList<T> list, Proc<T> proc) {
            using (Pipeline pp = new Pipeline()) {
                foreach (T e in list) {
                    pp.Do(e, proc);
                }
            }
        }
    }

    public class TaskQueue<T> where T : class {
        Queue<T> queue;
        Semaphore semTasks;  // tasks in the queue
        Semaphore semSlots;  // capacity of the queue to accept new tasks.

        public TaskQueue(int maxQueSize) {
            queue = new Queue<T>();
            semTasks = new Semaphore(0, maxQueSize);
            semSlots = new Semaphore(maxQueSize, maxQueSize);
        }

        public void PutTask(T task) {
            semSlots.WaitOne();
            lock (this) {
                queue.Enqueue(task);
            }
            semTasks.Release();
        }

        public T PullTask() {
            semTasks.WaitOne();
            lock (this) {
                if (queue.Peek() == null) {
                    return null;
                } else {
                    T task = queue.Dequeue();
                    semSlots.Release();
                    return task;
                }
            }
        }
    }

    /// <summary>
    /// Spanoff a thread to do the reading part asychroneously.
    /// </summary>
    public class AsynReadStream : Stream {
        Stream inStream;
        byte[] buf;
        int bufSize;
        AutoResetEvent sigDataAdded;
        AutoResetEvent sigBufferAdded;
        int dataBegin;
        int dataLength;
        int bufBegin;
        int bufLength;
        bool isClosed;

        public AsynReadStream(Stream inStream, int bufSize = (1<<22)) {
            this.inStream = inStream;
            this.bufSize = bufSize;
            buf = new byte[bufSize];   // implements a buffer queue and data queue.
            dataBegin = 0;
            dataLength = 0;
            bufBegin = 0;
            bufLength = bufSize;
            sigDataAdded = new AutoResetEvent(false);
            sigBufferAdded = new AutoResetEvent(false);
            isClosed = false;
            ThreadPool.QueueUserWorkItem(Proc);
        }

        void Proc(object arg) {
            while (true) {

                while (bufLength == 0) {
                    sigBufferAdded.WaitOne();
                }

                int bufLength2 = Math.Min(bufLength, bufSize - bufBegin);
                int len = inStream.Read(buf, bufBegin, bufLength2);
                if (len == 0) {
                    isClosed = true;
                    sigDataAdded.Set();  //  isClosed is kind of last "data byte". 
                    inStream.Close();
                    inStream.Dispose();
                    return;
                }

                bufBegin = (bufBegin + len) % bufSize;
                Interlocked.Add(ref bufLength, -len); // without Interlock, the methods will fail for large dataset.
                Interlocked.Add(ref dataLength, len);
                sigDataAdded.Set();
            }
        }

        public override int Read(byte[] buffer, int index, int count) {
            while (dataLength == 0) {
                if (isClosed) return 0;
                sigDataAdded.WaitOne();
            }

            int len = Math.Min(Math.Min(count, dataLength), bufSize - dataBegin);
            Array.Copy(buf, dataBegin, buffer, index, len);

            dataBegin = (dataBegin + len) % bufSize;
            Interlocked.Add(ref dataLength, -len);
            Interlocked.Add(ref bufLength, len);
            sigBufferAdded.Set();

            return len;
        }

        public override void Close() {
            base.Close();
            sigBufferAdded.Close();
            sigBufferAdded.Dispose();
            sigDataAdded.Close();
            sigDataAdded.Dispose();
        }

        public override void Write(byte[] buffer, int offset, int count) { }
        public override bool CanSeek { get { return false; } }
        public override bool CanRead { get { return true; } }
        public override bool CanWrite { get { return false; } }
        public override void Flush() { }
        public override long Length { get { return 0; } }
        public override void SetLength(long length) { }
        public override long Position { get { return 0; } set { } }
        public override long Seek(long offset, SeekOrigin origin) { return 0; }
    }

    /// <summary>
    /// Spanoff a thread to do the write part asychroneously.
    /// </summary>
    public class AsynWriteStream : Stream {
        Stream outStream;
        byte[] buf;
        int bufSize;
        AutoResetEvent sigDataAdded;
        AutoResetEvent sigBufferAdded;
        int dataBegin;
        int dataLength;
        int bufBegin;
        int bufLength;
        bool closing;
        Thread writeThread;
        static bool waitOnClose = false;

        public AsynWriteStream(Stream inStream, int bufSize = (1<<22)) {
            this.outStream = inStream;
            this.bufSize = bufSize;
            buf = new byte[this.bufSize];   // implements a buffer queue and data queue.
            dataBegin = 0;
            dataLength = 0;
            bufBegin = 0;
            bufLength = this.bufSize;
            closing = false;
            sigDataAdded = new AutoResetEvent(false);
            sigBufferAdded = new AutoResetEvent(false);
            writeThread = new Thread(Proc);
            writeThread.Start();
        }

        void Proc(object arg) {
            while (true) {
                while (dataLength == 0) {
                    sigDataAdded.WaitOne();
                    if (closing && (dataLength == 0)) {
                        outStream.Flush();
                        outStream.Close();
                        base.Close();
                        sigBufferAdded.Close();
                        sigBufferAdded.Dispose();
                        sigDataAdded.Close();
                        sigDataAdded.Dispose();
                        return;
                    }
                }

                int len = Math.Min(dataLength, bufSize - dataBegin);
                outStream.Write(buf, dataBegin, len);

                dataBegin = (dataBegin + len) % bufSize;
                Interlocked.Add(ref bufLength, len); // without Interlock, the methods will fail for large dataset.
                Interlocked.Add(ref dataLength,-len);
                sigBufferAdded.Set();
            }
        }

        public override void Write(byte[] buffer, int index, int count) {
            while (bufLength < count ) {
                sigBufferAdded.WaitOne();
            }

            int len1 = Math.Min(count, bufSize - bufBegin);
            int len2 = count - len1;            

            Array.Copy(buffer, index, buf, bufBegin, len1);
            if (len2 > 0) { 
                // the buffer[] has to be splitted into 2 pices and copied to the end and beginning
                // of the array buf[]. Here we copy the second pice, if it is not empty.
                Array.Copy(buffer, index + len1, buf, 0, len2);
            }

            bufBegin = (bufBegin + count) % bufSize;
            Interlocked.Add(ref dataLength, count);
            Interlocked.Add(ref bufLength, -count);
            sigDataAdded.Set();
        }

        public override void Flush() { // will be automatically done in Close() and in Proc().
        }

        /// <summary>
        /// Global variable to tentative make the AsynWriteStream object to wait when Close() is called.
        /// </summary>
        public static bool WaitOnClose {
            get { return waitOnClose; }
            set { waitOnClose = value; }
        }

        public override void Close() {
            closing = true;
            sigDataAdded.Set();
            if (waitOnClose) {
                // Wait till the writing thread finished all its job. We need to wait
                // if we want't read the file immediately, as by Reload document operation.
                // In most case we don't have to wait here. The caller should
                // set the waitOnClose() flag to activate this waiting step.
                if (writeThread != null) {
                    writeThread.Join();
                    writeThread = null;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count) { return 0; }
        public override bool CanSeek { get { return false; } }
        public override bool CanRead { get { return false; } }
        public override bool CanWrite { get { return true; } }
        public override long Length { get { return 0; } }
        public override void SetLength(long length) { }
        public override long Position { get { return 0; } set { } }
        public override long Seek(long offset, SeekOrigin origin) { return 0; }
    }

    public struct LoopInterval {
        public int Begin;
        public int End;
        public int Size {
            get { return End - Begin; }
        }

        public LoopInterval(int begin, int end) {
            Begin = begin;
            End = end;
        }
    };

    public static class Multithreading {

        public static int CfgWorkThreads = 6;

        //
        // Notice: we should avoid creating multiple thread (like animation threads) which call
        // this class to span-off sub-threads again. This will cause dead-lock at AutoResetEvent.WaitOne()
        // call when some threads get interrupted.
        // For instance, the PCA view used to Matrix.Mult() in animation thread. In that case, when
        // multiple PCA views are running animation and some are closed, other running PCA views may
        // get locked. For to fix this problem, the method Matrix.MultNoParallel() has been created.
        //
        //
        public delegate void ThreadProc(ThreadInfo threadInfo);
        public delegate void ThreadProc2(int loopBegin, int loopEnd);
        public delegate void LoopProc(int n);
        public delegate void LoopProc2(int n, int threadIndex);
        public delegate void LoopProc3(int loopBegin, int loopEnd);
        public delegate void LoopProc4(int n, ThreadInfo info);

        //  Parallelize a for-loop by equally slicing the loop range.
        //  Usage: Replaces "for(int n=s; n<t; n++) ..." by 
        //  MultiThreading.StartLoops(s, t, delegate(int n) { 
        //     ...
        //  });
        //
        public static void StartLoops(int loopBegin, int loopEnd, LoopProc proc) {
            StartThreads(
                delegate(object arg) {
                    ThreadInfo threadInfo = arg as ThreadInfo;
                    LoopInterval loop = threadInfo.GetLoopInterval(loopEnd-loopBegin);
                    for (int n = loop.Begin; n < loop.End; n++) {
                        proc(loopBegin+n);
                    }
                    threadInfo.Finished();
                }
            );
        }

        // Threads loop intervals are interwined with each other
        public static void StartLoopsSync(int loopBegin, int loopEnd, LoopProc proc) {
            StartThreads(
                delegate(object arg) {
                    ThreadInfo threadInfo = arg as ThreadInfo;
                    for (int n = loopBegin + threadInfo.ThreadIndex; n < loopEnd; n+=threadInfo.Siblings) {
                        proc(n);
                    }
                    threadInfo.Finished();
                }
            );
        }

        // Loop with thread initialization section.
        // Usage: Replaces for(int n=s; n<t; n++) ..." by
        //   MultiThreading.StartLoops(s, t, delegate(int loopBegin, int loopEnd) { 
        //     <Thrad Initialization Code>
        //     for(int n=loopBegin; n<loopEnd; n++) {
        //       ...
        //     }
        //     <Thread Dispose Code> 
        //   }
        //
        public static void StartLoops(int loopBegin, int loopEnd, LoopProc3 proc) {
            StartThreads(
                delegate(object arg) {
                    ThreadInfo threadInfo = arg as ThreadInfo;
                    LoopInterval loop = threadInfo.GetLoopInterval(loopEnd - loopBegin);
                    proc(loopBegin + loop.Begin, loopBegin + loop.End); 
                    threadInfo.Finished();
                }
            );
        }

        public static ThreadInfo[] StartLoops(int loopBegin, int loopEnd, LoopProc4 proc) {
            return StartThreads(
                delegate(object arg) {
                    ThreadInfo threadInfo = arg as ThreadInfo;
                    LoopInterval loop = threadInfo.GetLoopInterval(loopEnd - loopBegin);
                    for (int n = loop.Begin; n < loop.End; n++) {
                        proc(loopBegin + n, threadInfo);
                    }
                    threadInfo.Finished();
                }
            );
        }

        /// <summary>
        /// For loop with exception-catch-and-propagate.
        /// </summary>
        /// <param name="loopBegin"></param>
        /// <param name="loopEnd"></param>
        /// <param name="proc"></param> 
        public static void StartLoopsException(int loopBegin, int loopEnd, LoopProc2 proc) {
            Exception threadException = null;

            StartThreads(
                delegate(object arg) {
                    ThreadInfo threadInfo = arg as ThreadInfo;
                    LoopInterval loop = threadInfo.GetLoopInterval(loopEnd - loopBegin);
                    try {
                        for (int n = loop.Begin; n < loop.End; n++) {
                        proc(loopBegin + n, threadInfo.ThreadIndex);
                    }
                    } catch (Exception ex) {
                        threadException = ex;
                    }
                    threadInfo.Finished();
                }
            );

            if (threadException != null) {
                throw threadException;
            }
        }

        //
        // This method is intended for parallelize loops with different work-load for each steps, 
        // e.g, when calculating an triangle matrix.
        // This method slices the loop range into smaller intervals, so that more physical threads 
        // will be working at the same time.
        //
        public static void StartLoopsEx(int loopBegin, int loopEnd, LoopProc proc) {
            if (CfgWorkThreads != 1) {
                int oldThreads = CfgWorkThreads;
                CfgWorkThreads = 3 * CfgWorkThreads + 3;
                StartLoops(loopBegin, loopEnd, proc);
                CfgWorkThreads = oldThreads;
           } else {
                StartLoops(loopBegin, loopEnd, proc);
            }
        }

        public static ThreadInfo[] StartThreadsEx(ThreadProc proc) {
            if (CfgWorkThreads != 1) {
                int oldThreads = CfgWorkThreads;
                CfgWorkThreads = 5 * CfgWorkThreads;
                ThreadInfo[] ret = StartThreads(delegate(object arg) { proc(arg as ThreadInfo); });
                CfgWorkThreads = oldThreads;
                return ret;
            } else {
                return StartThreads(delegate(object arg) { proc(arg as ThreadInfo); });
            }
        }

        public static ThreadInfo[] StartThreads(ThreadProc proc) {
            return StartThreads(delegate(object arg) { proc(arg as ThreadInfo); });
        }

        public static ThreadInfo[] StartThreads(int N, ThreadProc2 proc) {
            return StartThreads(delegate(object arg) {
                ThreadInfo tInfo = arg as ThreadInfo;
                LoopInterval loopInfo = tInfo.GetLoopInterval(N);
                proc(loopInfo.Begin, loopInfo.End);
                tInfo.Finished();
            });
        }

        static ThreadInfo[] StartThreads(ParameterizedThreadStart proc) {
            int threads = CfgWorkThreads;

            if (threads == 1) {
                ThreadInfo info = new ThreadInfo(0, 1);
                proc(info);
                return new ThreadInfo[] { info };
            } else {
                AutoResetEvent[] waitHandles = new AutoResetEvent[threads];
                ThreadInfo[] infoList = new ThreadInfo[threads];

                CfgWorkThreads = 1;  // disabled child threads to spawn more threads.

                for (int n = 0; n < threads; n++) {
                    infoList[n] = new ThreadInfo(n, threads);
                    waitHandles[n] = infoList[n].ResetEvent = new AutoResetEvent(false);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(proc), infoList[n]);
                }

                //We cannot call WaitAll() here, as it doesnot work in STA threads.
                foreach (AutoResetEvent e in waitHandles) {
                    e.WaitOne();
                    e.Close();
                }

                CfgWorkThreads = threads;
                return infoList;
            }
        }
    }

    /// <summary>
    /// Pipeline with synchronization (i.e. serialization).
    /// </summary>
    /// Notice: this type of multithreading is similar as those realized in
    /// HLSL: Threads run concurrently in general, but must pass seq-section(s) sequentially.
    /// 
    public class SyncPipe {

        public class ThreadInfo {
            public int threadIndex;
            public AutoResetEvent completeEvent;
            public AutoResetEvent[] seqEvents;
            public AutoResetEvent[] seqEvents2;

            public class SeqEvent : IDisposable {
                AutoResetEvent evt;
                public SeqEvent(AutoResetEvent evt) { this.evt = evt; }
                public void Dispose() { evt.Set(); }
            }

            public SeqEvent Synchronize1 {
                get {
                    if (seqEvents==null) return null; // single thread case.              
                    seqEvents[threadIndex].WaitOne();
                    return new SeqEvent(seqEvents[(threadIndex + 1) % seqEvents.Length]);
                }
            }

            public SeqEvent Synchronize2 {
                get {
                    if (seqEvents2 ==null) return null; // single thread case.              
                    seqEvents2[threadIndex].WaitOne();
                    return new SeqEvent(seqEvents2[(threadIndex + 1) % seqEvents2.Length]);
                }
            }

        }
        public delegate bool Proc(ThreadInfo info);

        int threads;  // maximal number of threads.

        /// <summary>
        /// Creates a pipeline with default number of threads.
        /// </summary>
        public SyncPipe() : this(Multithreading.CfgWorkThreads) {
        }

        /// <summary>
        /// Creates a pipeline with given maximal number of threads.
        /// </summary>
        /// <param name="threads"></param>
        public SyncPipe(int threads) {
            this.threads = threads;
            if (threads == 1) return;  // No concurrency.
        }

        public static void Do(Proc proc) {
            SyncPipe pp = new SyncPipe();
            pp.DoAll(proc);
        }

        public void DoAll(Proc proc) {
            if (threads == 1) {
                ThreadInfo info = new ThreadInfo();
                info.threadIndex = 0;
                while (proc(info)) {
                    ;
                }
                return;
            }

            AutoResetEvent[] completeEventList = new AutoResetEvent[threads];
            ThreadInfo[] infoList = new ThreadInfo[threads];
            AutoResetEvent[] seqEvents = new AutoResetEvent[threads];
            AutoResetEvent[] seqEvents2 = new AutoResetEvent[threads];

            for (int n = 0; n < threads; n++) {
                infoList[n] = new ThreadInfo();
                infoList[n].threadIndex = n;
                completeEventList[n] = infoList[n].completeEvent = new AutoResetEvent(false);
                seqEvents[n] = new AutoResetEvent(false);
                infoList[n].seqEvents = seqEvents;  // each thread has a reference to the whole list.
                seqEvents2[n] = new AutoResetEvent(false);
                infoList[n].seqEvents2 = seqEvents2;  // each thread has a reference to the whole list.
            }
            seqEvents[0].Set();
            seqEvents2[0].Set();

            for (int n = 0; n < threads; n++) {
                ThreadPool.QueueUserWorkItem(new WaitCallback(
                    delegate(object state) {
                        ThreadInfo info = state as ThreadInfo;
                        while (proc(info)) {
                            ;
                        }
                        info.completeEvent.Set();
                    }
                ), infoList[n]);
            }

            // Wait till all threads terminated.
            foreach (AutoResetEvent e in completeEventList) {
                e.WaitOne();
                e.Close();
            }
            foreach (AutoResetEvent e in seqEvents) {
                e.Close();
            }
            foreach (AutoResetEvent e in seqEvents2) {
                e.Close();
            }
        }
    }

}
