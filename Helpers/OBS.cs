using System;
using System.Collections.Generic;
using System.IO;
using OBSWebsocketDotNet;


namespace Phat_Stats.Helpers
{
    public static class PhatOBS
    {
        private static readonly string logFile = $"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\\log.txt";

        // Send a series of key presses to the Calculator application.
        public static bool StartStream(OBSWebsocket obs)
        {
            try
            {
                obs.StartStreaming();

                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Stream Started");
                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Stream Started" });
                }                

                return true;
            }
            catch (AuthFailureException ex)
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - {ex.Message}");
                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - {ex.Message}" });
                }
                

                return false;
            }
            catch (ErrorResponseException ex)
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - {ex.Message}");
                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - {ex.Message}" });
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - {ex.Message}");
                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - {ex.Message}" });
                }

                return false;
            }
        }

        public static bool StopStream(OBSWebsocket obs)
        {
            try
            {
                obs.StopStreaming();

                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Stream Stopped");
                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Stream Stopped" });
                }                

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - {ex.Message}");
                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - {ex.Message}" });
                }                

                return false;
            }
        }
    }
}
