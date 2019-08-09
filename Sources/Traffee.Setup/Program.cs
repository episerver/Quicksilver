using Microsoft.Web.Administration;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.ServiceProcess;
using System.Linq;
using System.Collections.Specialized;

namespace Traffee.Setup
{
    class Program
    {
        static NameValueCollection AppSettings
        {
            get
            {
                return ConfigurationManager.AppSettings;
            }
        }
        static void Main(string[] args)
        {
            /*  
             *  setup arguments:
             *      - (any of the following) setup iis/sql/node
             *      - setup iis sql node
             *      - or any combination
             *      - otherwise, all setup will be run.
             */
            bool runAll = args is null || args.Length.Equals(0);

            if (runAll)
            {
                if (RunIISSetup()) if (RunDatabaseSetup()) RunNodeSetup();
            }
            else
            {
                if (args.Any(arg => arg.ToLower().Trim().Contains("iis")))
                {
                    RunIISSetup();
                }

                if (args.Any(arg => arg.ToLower().Trim().Contains("sql")))
                {
                    RunDatabaseSetup();
                }

                if (args.Any(arg => arg.ToLower().Trim().Contains("node")))
                {
                    RunNodeSetup();
                }
            }
        }
        static bool RunIISSetup()
        {
            ServiceController serviceController = new ServiceController("World Wide Web Publishing Service");

            if (serviceController.Status != ServiceControllerStatus.Running)
            {
                WriteLine("Please make sure that IIS is enabled and running.");
                return false;
            }

            string name = GetSiteName(AppSettings["site-name"]);
            string path = GetPhysicalPath(AppSettings["physical-path"]);

            ServerManager manager = new ServerManager();

            if (manager.Sites.FirstOrDefault(site => site.Name.Equals(name)) != null)
            {
                WriteLine($"Site '{name}' already present in IIS.");
                return true;
            }

            Site defaultSite = manager.Sites.Add(
                name: name,
                bindingProtocol: AppSettings["protocol"],
                bindingInformation: $"{ AppSettings["binding-information"] }:{ name }",
                physicalPath: path
            );

            ApplicationPool pool = manager.ApplicationPools.Add($"{name} pool");
            defaultSite.Applications[0].ApplicationPoolName = $"{name} pool";

            manager.CommitChanges();

            return true;
        }
        static string GetSiteName(string name)
        {
            WriteLine($"Default Name: [ {name} ]");
            WriteLine();
            Write("Site Name (Press Enter key to use default): ");

            string newName = Console.ReadLine();

            return string.IsNullOrWhiteSpace(newName) ? name : newName;
        }

        static string GetPhysicalPath(string path)
        {
            WriteLines(2);
            WriteLine($"Default Path: [ {path} ]");
            WriteLine();
            Write("Physical Path (Press Enter key to use default): ");

            string newPath = Console.ReadLine();

            return string.IsNullOrWhiteSpace(newPath) ? path : newPath;
        }

        static void WriteLines(int lines = 1, string text = "")
        {
            for(int i = 0; i < lines; i++)
                WriteLine(text);
        }
        static void WriteLine(string text = "")
        {
            Console.WriteLine(text);
        }
        static void Write(string text = "")
        {
            Console.Write(text);
        }
        static bool RunDatabaseSetup()
        {
            WriteLines(1);
            WriteLine("Started setting up databases..");

            if (RunProcess(@"Setup\SetupDatabases.cmd", true, AppSettings["root"])) return RunProcess(@"SetupDatabaseMixedAuthenticationMode.cmd");

            WriteLine("Error: Failed to setup database!");
            return false;
        }
        static bool RunNodeSetup()
        {
            if (!RunProcess("CheckNode.cmd", true)) return DownloadNode();

            return true;
        }
        static bool DownloadNode()
        {
            try
            {
                if (AppSettings["node-update"] != "1") return SetupNodeJS();

                WebClient webClient = new WebClient();
                webClient.DownloadFileCompleted += DownloadFileCompleted;
                webClient.DownloadFileTaskAsync(new Uri($"http://nodejs.org/dist/v{AppSettings["node-version"]}/node-v{AppSettings["node-version"]}-x64.msi"), "node-v10.16.2-x64.msi").Wait();
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message.ToString());
            }
            return true;
        }
        static bool SetupNodeJS()
        {
            RunProcess("SetupNode.cmd");
            WriteLine("");
            WriteLine("Node.js installed!");
            return true;
        }
        static void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            WriteLine("");
            WriteLine("Node.js installer downloaded!");
            WriteLine("");
            SetupNodeJS();
        }

        static bool RunProcess(string command, bool readOutput = false, string directory = "")
        {
            string output = string.Empty;
            string appdirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (!string.IsNullOrWhiteSpace(directory))
            {
                int index = appdirectory.IndexOf(directory);
                if (index >= 0) appdirectory = $"{appdirectory.Substring(0, index) + directory}\\";
            }

            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.WorkingDirectory = appdirectory;
                    process.StartInfo.FileName = $"{appdirectory}{command}";

                    if (readOutput) process.StartInfo.RedirectStandardOutput = true;

                    process.Start();
                    process.WaitForExit();

                    if (readOutput) output = process.StandardOutput.ReadToEnd();
                }

                if (readOutput && !string.IsNullOrWhiteSpace(output))
                {
                    WriteLine(output);

                    if (!output.ToLower().Contains("done installing databases.")) return false;
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message.ToString());
            }

            return true;
        }
    }
}
