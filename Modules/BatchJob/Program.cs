using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'

namespace BatchJob
{
    static class Program
    {
        static void Main(string[] args)
        {
            File.AppendAllText("app-trace.log", $"{DateTimeOffset.Now}: Start Job");
            int i = 0;
            while (true)
            {
                try
                {
                    string cfg = ConfigurationManager.AppSettings["xyz"] ?? "undefined";
                    string conn = ConfigurationManager.ConnectionStrings["abc"]?.ConnectionString ?? "undefined";
                    string envAbc = Environment.GetEnvironmentVariable("abc") ?? "undefined";
                    string envXyz = Environment.GetEnvironmentVariable("xyz") ?? "undefined";
                    var data = $"Processing {++i}: {envXyz} -> {cfg}, {envAbc} -> {conn}";
                    Console.WriteLine(data);
                    File.AppendAllText("trace.log", data);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($@"---------------- ERROR ---------------------
{ex}
--------------------------------------");
                }
                Thread.Sleep(2000);
            }
        }
    }
}
