using DocAggregator.API.Core;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DocAggregator.API
{
    public class Program
    {
        static string Scripts
        {
            get => _scripts;
            set
            {
                _scripts = Path.GetFullPath(value);
            }
        }
        public string LibreOfficeFolder
        {
            get => _libreOfficeFolder;
            set
            {
                _libreOfficeFolder = Path.GetFullPath(value);
                _libreOfficeExecutable = Path.Combine(_libreOfficeFolder, "soffice");
            }
        }
        public string LibreOfficeExecutable
        {
            get => _libreOfficeExecutable;
        }
        private string _libreOfficeFolder;
        private string _libreOfficeExecutable;

        private static string _scripts;
        public static void Main(string[] args)
        {
            /*if (Process.GetProcessesByName("soffice").Length == 0)
            {
                // Запуск libreoffice(soffice) если не запущен 
                //_logger.Information("Path LibreOfficeExecutable:   " + LibreOfficeExecutable);
                ProcessStartInfo processServerInfo = new ProcessStartInfo()
                {
                    // cmd> cd "D:\Users\akkostin\source\repos\DocumentAggregator\unoserver\src\unoserver"
                    WorkingDirectory = Scripts,

                    FileName = "sh",
                    Arguments = $"py_console.bat",

                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                    //#endif
                };

                try
                {
                    Process serverInfoProcess = new Process()
                    {
                        StartInfo = processServerInfo
                    };

                    serverInfoProcess.Start();
                    serverInfoProcess.BeginErrorReadLine();
                    //serverInfoProcess.WaitForExit();
                }
                catch { }
            }*/
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
