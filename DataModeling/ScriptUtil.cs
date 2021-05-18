using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace VisuMap.DataModeling {
    public class ScriptUtil {
        public static Process StartCmd(string progName, string argList,  bool showWindow) {
            // new ProcessStartInfo("cmd.exe", "/K " + progName + " " + argList + " && exit")
            ProcessStartInfo info = new ProcessStartInfo(progName, argList) {
                WorkingDirectory = DataModeling.workDir,
                WindowStyle = showWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
            };
            return Process.Start(info);
        }

        public static void CallCmd(string progName, string argList, bool showWindow) {
            var proc = StartCmd(progName, argList, showWindow);
            while (!proc.WaitForExit(100))
                Application.DoEvents();
        }
    }
}
