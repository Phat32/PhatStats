using Newtonsoft.Json;
using RestSharp;
using Phat_Stats.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Phat_Stats
{
    class Program
    {
        private static readonly RestClient client = new RestClient(AppSettings.Get<string>("ApiUri"));
        private static readonly string printFile = $"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\\print.txt";
        private static readonly string logFile = $"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\\log.txt";
        
        private static bool isStreaming = false;
        private static DateTime standbyTime;

        static void Main(string[] args)
        {
            try
            {
                var message = new StringBuilder();

                message.AppendLine();
                message.AppendLine("==================================");
                message.AppendLine("      Welcome to Phat Stats!      ");
                message.AppendLine("==================================");
                message.AppendLine();
                message.AppendLine("Print details are being written to");
                message.AppendLine(printFile);
                message.AppendLine();

                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllText(logFile, message.ToString());
                }
                Console.WriteLine(message.ToString());

                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Starting up..." });
                }
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Starting up...");

                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromSeconds(1);

                var timer = new System.Threading.Timer((e) =>
                {
                    GetPrint();
                }, null, startTimeSpan, periodTimeSpan);

                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Starting up..." });
                }
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Up and running");

                while (true)
                {
                    System.Console.ReadKey();
                }
            }
            catch (AppSettingNotFoundException ex)
            {
                File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - ERROR: AppSetting with the key \"{ex.Message}\" is missing"});
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - ERROR: {ex.Message}");
            }
        }

        public static void GetPrint()
        {
            try
            {
                var request = new RestRequest("api/printer", Method.GET);
                request.AddParameter("apikey", AppSettings.Get<string>("ApiKey"));
                var printerResponse = client.Execute(request);

                request = new RestRequest("api/job", Method.GET);
                request.AddParameter("apikey", AppSettings.Get<string>("ApiKey"));
                var jobResponse = client.Execute(request);

                request = new RestRequest("plugin/filamentmanager/selections", Method.GET);
                request.AddParameter("apikey", AppSettings.Get<string>("ApiKey"));
                var filamentResponse = client.Execute(request);

                var message = new StringBuilder();

                var job = JsonConvert.DeserializeObject<dynamic>(jobResponse.Content);
                if (job["state"] == "Offline")
                {
                    OBS.StopStream();
                    isStreaming = false;

                    message.AppendLine("Offline");
                }
                else
                { 
                var printer = JsonConvert.DeserializeObject<dynamic>(printerResponse.Content);                
                var filament = JsonConvert.DeserializeObject<dynamic>(filamentResponse.Content);

                    if (printer["state"] == null)
                    {
                        if (standbyTime == DateTime.MinValue)
                        {
                            standbyTime = DateTime.Now;

                            if (AppSettings.Get<bool>("EnableLog"))
                            {
                                File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Starting Standby Timer" });
                            }
                            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Starting Standby Timer");
                        }

                        if (DateTime.Now.Subtract(standbyTime).TotalMinutes > 20 && isStreaming)
                        {
                            OBS.StopStream();
                            isStreaming = false;
                        }

                        message.AppendLine("Offline");
                    }
                    else if (printer["state"]["text"] != "Printing")
                    {
                        if (standbyTime == DateTime.MinValue)
                        {
                            standbyTime = DateTime.Now;
                            if (AppSettings.Get<bool>("EnableLog"))
                            {
                                File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Starting Standby Timer" });
                            }
                            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Starting Standby Timer");
                        }

                        if (DateTime.Now.Subtract(standbyTime).TotalMinutes > 30 && isStreaming)
                        {
                            OBS.StopStream();
                            isStreaming = false;
                        }

                        message.AppendLine(GetTemps(printer, filament));
                        message.AppendLine(string.Empty);
                        message.AppendLine("Standing By");
                    }
                    else
                    {
                        if (!isStreaming)
                        {
                            OBS.StartStream();
                            isStreaming = true;
                            standbyTime = DateTime.MinValue;
                        }

                        if (standbyTime != DateTime.MinValue)
                        {
                            standbyTime = DateTime.MinValue;

                            if (AppSettings.Get<bool>("EnableLog"))
                            {
                                File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Clearing Standby Timer" });
                            }
                            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - Clearing Standby Timer");
                        }

                        message.AppendLine(GetTemps(printer, filament));

                        var printTimeSpan = TimeSpan.FromSeconds(Convert.ToDouble(job["progress"]["printTime"]));
                        var printTimeLeftSpan = TimeSpan.FromSeconds(0);
                        if (!string.IsNullOrWhiteSpace(job["progress"]["printTimeLeft"].ToString()))
                        {
                            printTimeLeftSpan = TimeSpan.FromSeconds(Convert.ToDouble(job["progress"]["printTimeLeft"]));
                        }
                        var printTimeLeftOrigin = job["progress"]["printTimeLeftOrigin"];

                        var printTime = string.Empty;
                        var printTimeLeft = string.Empty;

                        if (printTimeSpan.Days > 0)
                        {
                            printTime += $"{printTimeSpan.Days}d";
                        }
                        if (printTimeSpan.Hours > 0)
                        {
                            printTime += $"{printTimeSpan.Hours}h";
                        }
                        if (printTimeSpan.Minutes > 0)
                        {
                            printTime += $"{printTimeSpan.Minutes}m";
                        }
                        if (string.IsNullOrWhiteSpace(printTime))
                        {
                            printTime = "< 1 Min";
                        }

                        if (printTimeLeftSpan.Days > 0)
                        {
                            printTimeLeft += $"{printTimeLeftSpan.Days}d";
                        }
                        if (printTimeLeftSpan.Hours > 0)
                        {
                            printTimeLeft += $"{printTimeLeftSpan.Hours}h";
                        }
                        if (printTimeLeftSpan.Minutes > 0)
                        {
                            printTimeLeft += $"{printTimeLeftSpan.Minutes}m";
                        }

                        if (string.IsNullOrWhiteSpace(printTimeLeft))
                        {
                            printTimeLeft = "< 1 Min";
                        }

                        message.AppendLine($"Printing for {printTime} with {printTimeLeft} left ({printTimeLeftOrigin})  {Math.Round(Convert.ToDecimal(job["progress"]["completion"]), 2, MidpointRounding.AwayFromZero)}%");
                        message.AppendLine($"File: {job["job"]["file"]["name"]}");
                    }
                }

                File.WriteAllText($"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\\print.txt", message.ToString());
            }
            catch (AppSettingNotFoundException ex)
            {
                File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - ERROR: AppSetting with the key \"{ex.Message}\" is missing" });
            }
            catch (Exception ex)
            {
                if (AppSettings.Get<bool>("EnableLog"))
                {
                    File.AppendAllLines(logFile, new List<string>() { $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - {ex.Message}" });
                }
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")} - {ex.Message}");
            }
        }

        private static string GetTemps(dynamic printer, dynamic filament)
        {
            var message = new StringBuilder();

            if (printer["temperature"]["tool0"]["target"] > 0)
            {
                message.AppendLine($"Tool 1: {filament["selections"][0]["spool"]["name"]} ({filament["selections"][0]["spool"]["profile"]["material"]}) {printer["temperature"]["tool0"]["actual"]}/{printer["temperature"]["tool0"]["target"]}");
            }
            else
            {
                if (printer["temperature"]["tool0"]["actual"] > 40)
                {
                    message.AppendLine($"Tool 1: {filament["selections"][0]["spool"]["name"]} ({filament["selections"][0]["spool"]["profile"]["material"]}) {printer["temperature"]["tool0"]["actual"]} (Cooling)");
                }
                else
                {
                    message.AppendLine($"Tool 1: Off");
                }
            }

            if (printer["temperature"]["tool1"]["target"] > 0)
            {
                message.AppendLine($"Tool 2: {filament["selections"][1]["spool"]["name"]} ({filament["selections"][1]["spool"]["profile"]["material"]}) {printer["temperature"]["tool1"]["actual"]}/{printer["temperature"]["tool1"]["target"]}");
            }
            else
            {
                if (printer["temperature"]["tool1"]["actual"] > 40)
                {
                    message.AppendLine($"Tool 2: {filament["selections"][1]["spool"]["name"]} ({filament["selections"][1]["spool"]["profile"]["material"]}) {printer["temperature"]["tool1"]["actual"]} (Cooling)");
                }
                else
                {
                    message.AppendLine($"Tool 2: Off");
                }
            }

            if (printer["temperature"]["bed"]["target"] > 0)
            {
                message.AppendLine($"Bed: {printer["temperature"]["bed"]["actual"]}/{printer["temperature"]["bed"]["target"]}");
            }
            else
            {
                if (printer["temperature"]["bed"]["actual"] > 35)
                {
                    message.AppendLine($"Bed: {printer["temperature"]["bed"]["actual"]} (Cooling)");
                }
                else
                {
                    message.AppendLine($"Bed: Off");
                }
            }

            return message.ToString().TrimEnd('\r', '\n');
        }        
    }
}
