using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using static System.StringComparison;

#pragma warning disable SCS0001 // Command injection possible in {1} argument passed to '{0}'

namespace Bnaya.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            string argsLine = string.Join(" ", args);
            Console.WriteLine($"args: {argsLine}");
            // Environment.SetEnvironmentVariable("abc", "from env");
            // Environment.SetEnvironmentVariable("xyz", "# from env");

            #region Help

            if (args?.Length <= 1)
            {
                if (args?.Length == 0 || args[0].StartsWith("-h", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(File.ReadAllText("Sample.txt"));
                    Thread.Sleep(TimeSpan.FromMinutes(3)); // let people chance to read before the docker goes down 
                    return;
                }
            }

            #endregion // Help

            #region Dictionary<string, StringBuilder> map = args

            var query = from arg in args
                        where arg.StartsWith("-", InvariantCultureIgnoreCase)
                        let len = arg.IndexOf(":", InvariantCultureIgnoreCase)
                        let key = len == -1 ? arg.Substring(1) : arg.Substring(1, len - 1)
                        let value = len == -1 ? null : arg.Substring(len + 1)
                        select new KeyValuePair<string, string>(key, value);
            var map = new ConcurrentDictionary<string, string>(query);

            #endregion // Dictionary<string, StringBuilder> map = args

            #region string additionalArgs = ...

            string dynamicArgsToken = "cmd /S /C";
            int cmd = argsLine.IndexOf(dynamicArgsToken, InvariantCultureIgnoreCase);
            string additionalArgs = string.Empty;
            if (cmd != -1)
                additionalArgs = argsLine.Substring(cmd + dynamicArgsToken.Length);

            #endregion // string additionalArgs = ...

            Console.WriteLine("Prepare Parameters:");
            Console.WriteLine(string.Join("\r\n", map.Select(m => $"{m.Key} = {m.Value}")));

            try
            {
                #region Override Config

                string configDirectory = map.GetOrAdd("config_dir", Environment.CurrentDirectory)?.ToString() ?? ".";
                if (map.TryGetValue("env_to_config", out string withSubFolder))
                {
                    Console.WriteLine("------------------ Config override ------------------------");
                    bool subDir = string.Compare(withSubFolder, "sub-dir", InvariantCultureIgnoreCase) == 0;
                    SearchOption options = subDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                    Console.WriteLine($"Configuration path = {configDirectory}");
                    string[] files = Directory.GetFiles(configDirectory, "*.config", options);
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        if (fileName == "Prepare.exe")
                            continue;
                        try
                        {
                            Console.WriteLine($"Overriding {file}");
                            var cnf = XDocument.Load(file);

                            #region App-Settings

                            var adds = cnf?.Element("configuration")
                                          ?.Element("appSettings")
                                          ?.Elements()
                                          ?.Where(m => m.Name == "add") ?? Array.Empty<XElement>();
                            foreach (var add in adds)
                            {
                                string name = add.Attribute("key").Value;
                                try
                                {
                                    string value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
                                    if (value != null)
                                        add.Attribute("value").Value = value;
                                }
                                catch { }
                            }

                            #endregion // App-Settings

                            #region Connection-Strings

                            var conns = cnf?.Element("configuration")
                                         ?.Element("connectionStrings")
                                         ?.Elements()
                                         ?.Where(m => m.Name == "add") ?? Array.Empty<XElement>();
                            foreach (var conn in conns)
                            {
                                string name = conn.Attribute("name").Value;
                                try
                                {
                                    string value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
                                    if (value != null)
                                        conn.Attribute("connectionString").Value = value;
                                }
                                catch { }
                            }

                            #endregion // Connection-Strings

                            var fi = new FileInfo(file);
                            var att = fi.Attributes;
                            fi.IsReadOnly = false;
                            cnf.Save(file, SaveOptions.OmitDuplicateNamespaces);
                            fi.Attributes = att;
                        }
                        #region Exception Handling

                        catch (Exception exc)
                        {
                            Console.WriteLine("ERROR (Config parse):");
                            Console.WriteLine(exc);
                        }

                        #endregion // Exception Handling
                    }
                }

                #endregion // Override Config

                Console.WriteLine("----------------- Start Process -----------------------");

                #region Start Process

                string workingDirectory = map.GetOrAdd("work_dir", string.Empty)?.ToString();
                string process = map.GetOrAdd("proc", string.Empty)?.ToString();
                string parameters = map.GetOrAdd("args", string.Empty)?.ToString();
                parameters = (parameters + additionalArgs).Trim();
                bool createNoWindow = map.ContainsKey("create_no_window");
                bool useShellExecute = map.ContainsKey("use_shell_execute");

                Console.WriteLine($@"Process Starting:
proc: {process}
args: {parameters}
working-directory: {workingDirectory}
create-no-window: {createNoWindow}
use-shell-execute: {useShellExecute}");

                if (!string.IsNullOrWhiteSpace(process))
                {
                    var pinfo = new ProcessStartInfo(process, parameters);
                    if (!string.IsNullOrWhiteSpace(workingDirectory))
                    {
                        pinfo.WorkingDirectory = workingDirectory;
                    }
                    if (!useShellExecute)
                    {
                        pinfo.RedirectStandardOutput = true;
                        pinfo.RedirectStandardError = true;
                    }
                    pinfo.CreateNoWindow = createNoWindow;
                    pinfo.UseShellExecute = useShellExecute;
                    var p = new Process();
                    p.StartInfo = pinfo;
                    if (!useShellExecute)
                    {
                        p.OutputDataReceived += (sender, a) => Console.WriteLine(a.Data);
                        p.ErrorDataReceived += (sender, a) => Console.WriteLine(a.Data);
                    }
                    Console.WriteLine("---------------------------------------------------");
                    p.Start();
                    if (!useShellExecute)
                    {
                        // start our event pumps
                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();
                    }

                    p.WaitForExit();
                }

                #endregion // Start Process
            }
            #region Exception Handling

            catch (Exception ex)
            {
                Console.WriteLine("ERROR:");
                Console.WriteLine(ex);
            }

            #endregion // Exception Handling
        }
    }
}
