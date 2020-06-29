// Copyright (C) 2020 VisuMap Technologies Inc.
//
using System;

namespace TsneDx {
    public class TimeCheck : IDisposable {
        long initTime;
        public TimeCheck() {
            initTime = DateTime.Now.Ticks / 10000;
        }

        public static TimeCheck Check => new TimeCheck();

        public void Dispose() {
            string rpt = "Time: " + (((int)(DateTime.Now.Ticks / 10000 - initTime)) / 1000.0).ToString("f3") + " sec";
            Console.WriteLine(rpt);
        }
    }
}
