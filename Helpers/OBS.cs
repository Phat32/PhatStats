using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Phat_Stats.Helpers
{
    public static class OBS
    {
        private static readonly string logFile = $"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\\log.txt";
        private static bool obsFound = true;

        // Activate an application window.
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private static bool SetObs()
        {
            IntPtr obs = IntPtr.Zero;

            Process[] pname = Process.GetProcessesByName(AppSettings.Get<string>("OBS"));

            var obsId = 0;
            if (pname.Length != 0)
            {
                obsId = pname[0].Id;
                obs = pname[0].MainWindowHandle;
            }

            // Verify that Calculator is a running process.
            if (obsId == 0 || obs == IntPtr.Zero)
            {
                if (obsFound)
                {
                    if (AppSettings.Get<bool>("EnableLog"))
                    {
                        File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - OBS does not appear to be running" });
                    }
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - OBS does not appear to be running");

                    obsFound = false;
                }

                return false;
            }

            if (!obsFound)
            {
                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - OBS has been detected" });
                }
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - OBS has been detected");
            }

            SetForegroundWindow(obs);
            return true;
        }

        // Send a series of key presses to the Calculator application.
        public static bool StartStream()
        {
            // Make Calculator the foreground application and send it 
            // a set of calculations.

            if (SetObs())
            {
                SendKeys.SendWait(AppSettings.Get<string>("StartKey"));

                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Stream Started" });
                }
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Stream Started");

                return true;
            }

            return false;
        }

        public static bool StopStream()
        {
            if (SetObs())
            {
                SendKeys.SendWait(AppSettings.Get<string>("StopKey"));

                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Stream Stopped" });
                }
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Stream Stopped");

                return true;
            }

            return false;
        }
    }
}
