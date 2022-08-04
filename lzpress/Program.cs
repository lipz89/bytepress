using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using lzpress.Engine;
using NDesk.Options;

namespace lzpress
{
    class Program
    {
        /// <summary>
        /// Main method to handle logic.
        /// </summary>
        static void Main(string[] args)
        {
            bool isWpf = false;
            bool showHelp = false;
            List<string> libs = new List<string>();
            OptionSet argz = new OptionSet {
                {
                    "wpf", "must be specified for wpf applications to work",
                    v => isWpf = true
                },
                {
                    "h|help",  "show this message",
                    v => showHelp = v != null
                },
            };

            try
            {
                Watermark();

                List<string> argsList = argz.Parse(args);

                if(showHelp || argsList.Count == 0)
                {
                    ShowHelp(argz);
                    return;
                }

                if(argsList.Count > 0)
                {
                    foreach(string arg in argsList)
                    {
                        if(arg.ToLower().EndsWith(".dll"))
                        {
                            var farg = GetFile(arg);
                            if(farg == null)
                            {
                                throw new Exception($"File {arg} does not exist");
                            }
                            libs.Add(farg);
                        }
                    }
                }

                string inFile = GetFile(args[0]);

                if(!File.Exists(inFile))
                    throw new Exception("Specified file does not exist");

                FileInfo f = new FileInfo(inFile);
                byte[] original = File.ReadAllBytes(inFile);

                if(!f.Name.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("Only executable files are supported");

                Presser p = new Presser(inFile, original, isWpf);
                p.UpdateStatus += UpdateStatus;

                if(libs.Count > 0)
                    p.MergeLibraries(libs);

                p.Process();
            }
            catch(OptionException e)
            {
                UpdateStatus(e.ToString(), Presser.StatusType.Error);
                UpdateStatus("Try lzpress --help for more information.", Presser.StatusType.Error);
                Console.Read();
            }

            catch(Exception e)
            {
                UpdateStatus(e.ToString(), Presser.StatusType.Error);
                throw;
                Environment.Exit(0); // Failsafe
            }
        }


        /// <summary>
        /// Displays help information to the user
        /// </summary>
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: lzpress [file to compress] [option] [value]");
            p.WriteOptionDescriptions(Console.Out);
        }

        /// <summary>
        /// Handles the console status updating.
        /// </summary>
        static void UpdateStatus(string status, Presser.StatusType type)
        {
            switch(type)
            {
                case Presser.StatusType.Normal:
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}: {status}");
                    break;
                case Presser.StatusType.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}: {status}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case Presser.StatusType.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}: {status}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
        }

        private static string GetFile(string file)
        {
            if(File.Exists(file))
            {
                return file;
            }
            var cdir = Directory.GetCurrentDirectory();
            var path = Path.Combine(cdir, file);
            if(File.Exists(path))
            {
                return path;
            }
            return null;
        }

        /// <summary>
        /// Watermarks the console with information.
        /// </summary>
        static void Watermark()
        {
            Console.WriteLine(@"    - * -  ＬＺＰＲＥＳＳ  - * -
Version: " + $"{Application.ProductVersion}" + "\r\nAuthor: lpz\r\n");
        }
    }
}
