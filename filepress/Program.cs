using bytepress.Compresison;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;

namespace filepress
{
    class Program
    {

        static void Main(string[] args)
        {
            Watermark();
            string algo = string.Empty;
            bool isCompress = true;
            bool showHelp = false;
            bool showConfirm = true;
            string output = string.Empty;
            OptionSet argz = new OptionSet {
                {
                    "a=", "选择压缩算法 (qlz(默认), gzip, lzma)",
                    v => algo = v
                },
                {
                    "d", "解压缩",
                    v => isCompress = false
                },
                {
                    "o=", "输出目录，默认当前工作目录",
                    v => output = v
                },
                {
                    "h",  "显示帮助信息",
                    v => showHelp = true
                },
                {
                    "y","不提示覆盖",
                    v => showConfirm = false
                }
            };

            try
            {
                List<string> argsList = argz.Parse(args);

                if(showHelp || argsList.Count == 0)
                {
                    var helpText = "用法：\tfilepress [file|directory] [options]";
                    Console.WriteLine(helpText);
                    argz.WriteOptionDescriptions(Console.Out);
                    return;
                }

                //var inFiles = argsList.Select(x => GetFileOrDirectory(x)).ToList();
                //var commpresser = GetCompressor(algo);

                //foreach(var item in inFiles)
                //{
                //    CommpressFile(commpresser, isCompress, item);
                //}
                if(string.IsNullOrWhiteSpace(output))
                {
                    output = Directory.GetCurrentDirectory();
                }
                if(!Directory.Exists(output))
                {
                    UpdateStatus("指定的输出路径不是有效的文件夹", StatusType.Warning);
                    return;
                }

                var input = GetFileOrDirectory(argsList[0]);
                if(input == null)
                {
                    UpdateStatus("指定的输入路径不是有效的文件或文件夹", StatusType.Warning);
                    return;
                }

                if(isCompress)
                {
                    var outputFile = Path.Combine(output, input.Name + ".plz");
                    if(IsOverride(outputFile, showConfirm))
                    {
                        var commpresser = Common.GetCompressor(algo);
                        var writer = new PressWriter(commpresser, input);
                        var buffer = writer.Write();
                        UpdateStatus($"开始写入包:{outputFile},大小:{buffer.Length.ToPrettySize(2)}");
                        File.WriteAllBytes(outputFile, buffer);
                        UpdateStatus($"压缩完成!");
                    }
                }
                else if(input is FileInfo fi)
                {
                    var buffer = File.ReadAllBytes(fi.FullName);
                    UpdateStatus($"包大小:{buffer.Length.ToPrettySize(2)}");
                    var reader = new PressReader(buffer);
                    UpdateStatus($"开始写入路径:{output}");
                    reader.Save(output);
                    UpdateStatus($"解压完成!");
                }
            }
            catch(Exception e)
            {
                UpdateStatus(e.ToString(), StatusType.Error);
                throw;
                Environment.Exit(0); // Failsafe
            }
        }

        private static bool IsOverride(string path, bool showConfirm)
        {
            if(!showConfirm)
            {
                return true;
            }
            if(File.Exists(path))
            {
                if(DialogResult.Yes != MessageBox.Show("文件已经存在，是否覆盖？", "提示", MessageBoxButtons.YesNo))
                {
                    return false;
                }
            }
            return true;
        }

        private static void CommpressFile(ICompressor commpresser, bool isCompress, string input)
        {
            if(!File.Exists(input))
                throw new Exception("文件不存在");

            FileInfo f = new FileInfo(input);
            byte[] original = File.ReadAllBytes(input);

            UpdateStatus($"使用算法：{commpresser.Name}");
            var outPath = Path.Combine(Directory.GetCurrentDirectory(), $"output_press_{commpresser.Name}");
            if(!isCompress)
            {
                outPath = Path.Combine(Directory.GetCurrentDirectory(), $"output_unpress");
            }
            if(!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }

            byte[] result;
            if(isCompress)
            {
                UpdateStatus("开始压缩...");
                result = commpresser.Compress(original);

                double compressionRatio = 100 - (double)result.Length / original.Length * 100.0;

                UpdateStatus("压缩结果", StatusType.Normal);
                UpdateStatus($"源文件大小:    {original.Length.ToPrettySize(2)}", StatusType.Normal);
                UpdateStatus($"压缩后大小: {result.Length.ToPrettySize(2)}", StatusType.Normal);
                UpdateStatus($"压缩比率:    {compressionRatio:F}%", StatusType.Normal);
                UpdateStatus("--------------------------------------------", StatusType.Normal);
            }
            else
            {
                UpdateStatus("开始解压...");
                result = commpresser.Decompress(original);

                UpdateStatus("压缩结果", StatusType.Normal);
                UpdateStatus($"源文件大小:    {original.Length.ToPrettySize(2)}", StatusType.Normal);
                UpdateStatus($"解压后大小: {result.Length.ToPrettySize(2)}", StatusType.Normal);
                UpdateStatus("--------------------------------------------", StatusType.Normal);
            }

            var output = Path.Combine(outPath, f.Name);
            File.WriteAllBytes(output, result);
        }

        /// <summary>
        /// Handles the console status updating.
        /// </summary>
        internal static void UpdateStatus(string status, StatusType type = StatusType.Normal)
        {
            switch(type)
            {
                case StatusType.Normal:
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}: {status}");
                    break;
                case StatusType.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}: {status}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case StatusType.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}: {status}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
        }


        private static FileSystemInfo GetFileOrDirectory(string path)
        {
            if(Path.IsPathRooted(path))
            {
                if(File.Exists(path))
                {
                    return new FileInfo(path);
                }
                if(Directory.Exists(path))
                {
                    return new DirectoryInfo(path);
                }
                return null;
            }
            else
            {
                var cdir = Directory.GetCurrentDirectory();
                var fpath = Path.Combine(cdir, path);
                return GetFileOrDirectory(fpath);
            }
        }
        public enum StatusType
        {
            Normal,
            Warning,
            Error
        }
        /// <summary>
        /// Watermarks the console with information.
        /// </summary>
        static void Watermark()
        {
            Console.WriteLine(@"    - * -  ＦＩＬＥＰＲＥＳＳ  - * -
Version: " + $"{Application.ProductVersion}" + "\r\nAuthor: lpz\r\n");
        }
    }
}
