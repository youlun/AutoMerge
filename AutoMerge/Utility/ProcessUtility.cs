using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMerge.Utility
{
    internal class ProcessUtility
    {
        internal static void StartProcess(
            string filename,
            string arguments,
            bool waitForExit = true,
            bool hidden = false,
            bool redirectStdout = false,
            Action processStarted = null,
            Action<string> outputDataReceived = null,
            Action processExited = null
        )
        {
            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = filename,
                Arguments = arguments,
                UseShellExecute = true
            };

            if (hidden) {
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
            }
            if (redirectStdout) {
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
            }

            var p = new Process() {
                StartInfo = psi,
                EnableRaisingEvents = true
            };
            if (redirectStdout) {
                p.OutputDataReceived += (s, e) => { outputDataReceived(e.Data); };
                p.ErrorDataReceived += (s, e) => { outputDataReceived(e.Data); };
            }
            if (processExited != null) {
                p.Exited += (s, e) => { processExited(); };
            }
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            processStarted();

            if (waitForExit) p.WaitForExit();
        }
    }
}
