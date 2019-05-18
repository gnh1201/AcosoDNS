using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace AcosoDNS
{
    class Program
    {
        static string lastOutput;
        static string lastError;
        static int lastExitCode;
        static List<WannaCry> lastItems;

        static void Main(string[] args)
        {
            Console.WriteLine("=== AcosoDNS ===");
            Console.WriteLine("Monitoring your network configuration...");

            while (true)
            {
                ExecuteCommand("ipconfig /all");
                List<WannaCry> items = getItems();
                
                if(lastItems != null)
                {
                    for (var i = 0; i < lastItems.Count; i++)
                    {
                        if (lastItems[i].value != items[i].value)
                        {
                            Console.WriteLine("경고! 설정이 변경되었습니다.");
                            Console.WriteLine("=== 변경 전 ===");
                            Console.WriteLine(string.Format("{0}: {1}", lastItems[i].key, lastItems[i].value));
                            Console.WriteLine("=== 변경 후 ===");
                            Console.WriteLine(string.Format("{0}: {1}", items[i].key, items[i].value));
                        }
                    }
                }

                // save to last items
                lastItems = items;

                // wait for next process
                Thread.Sleep(30);
            }
            
        }

        static List<WannaCry> getItems()
        {
            List<WannaCry> items = new List<WannaCry>();
            string[] lines = Regex.Split(lastOutput, "\r\n|\r|\n");

            foreach (var line in lines)
            {
                int index = line.IndexOf(':');
                if (index <= 0) continue; // skip empty lines

                string key = line.Substring(0, index).TrimEnd(' ', '.');
                string value = line.Substring(index + 1).Replace("(Preferred)", "").Trim();

                WannaCry item = new WannaCry();
                item.key = key;
                item.value = value;

                items.Add(item);
            }

            return items;
        }

        static void ExecuteCommand(string command)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;

            //Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            //Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            //Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");

            lastOutput = (String.IsNullOrEmpty(output) ? "(none)" : output);
            lastError = (String.IsNullOrEmpty(error) ? "(none)" : error);
            lastExitCode = exitCode;

            process.Close();
        }
    }
}
