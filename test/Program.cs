using bytepress.Compresison;
using bytepress.Compresison.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                //var path = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                //var fs = path.GetFiles("*.jpg");
                //var qlz = new QuickLZ();
                //var gzip = new GZIP();
                //var lzma = new LZMAez();

                //foreach(var item in fs)
                //{
                //    var datas = File.ReadAllBytes(item.FullName);
                //    Console.WriteLine($"源文件：{item.Name}，大小：{datas.Length}");
                //    var qlzdata = qlz.Compress(datas);
                //    File.WriteAllBytes(item.FullName + ".qlz", qlzdata);
                //    Console.WriteLine($"使用压缩算法：qlz，压缩大小：{qlzdata.Length}");

                //    var gzipdata = gzip.Compress(datas);
                //    File.WriteAllBytes(item.FullName + ".gzip", gzipdata);
                //    Console.WriteLine($"使用压缩算法：gzip，压缩大小：{gzipdata.Length}");

                //    var lzmadata = lzma.Compress(datas);
                //    File.WriteAllBytes(item.FullName + ".lzma", lzmadata);
                //    Console.WriteLine($"使用压缩算法：lzma，压缩大小：{lzmadata.Length}");

                //    Console.WriteLine("-================================================-");
                //}

                var data = File.ReadAllBytes("pages.plz");

                press.pr.rs(data, AppDomain.CurrentDomain.BaseDirectory);

                Console.ReadKey();
                //Run(bytes, args.Skip(1).ToArray());
            }
            catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        /// <summary>
        /// Invokes the assembly in memory with the correct parameters (if applicable).
        /// </summary>
        static bool Run(byte[] assemblyBytes, string[] arguments)
        {
            Assembly assembly = AppDomain.CurrentDomain.Load(assemblyBytes);

            //wpfhack

            MethodInfo entryPoint = assembly.EntryPoint;
            object instance = assembly.CreateInstance(entryPoint.Name);

            ManualResetEvent ensureLoaded = new ManualResetEvent(false);
            bool loaded = false;

            object[] parameters = GetMethodParameters(entryPoint, arguments);

            try
            {
                entryPoint.Invoke(instance, parameters);
                loaded = true;
            }
            catch(TargetInvocationException invocationEx)
            {
                loaded = false;
            }
            finally
            {
                ensureLoaded.Set();
            }

            ensureLoaded.WaitOne();
            ensureLoaded.Dispose();

            return loaded;
        }

        /// <summary>
        /// Builds the necessary parameters to invoke the main method (if applicable).
        /// </summary>
        static object[] GetMethodParameters(MethodInfo method, string[] optionalParameters)
        {
            if(method.GetParameters().Length == 0)
                return null;

            if(optionalParameters != null)
                return new object[] { optionalParameters };

            return new object[] { null };
        }
    }
}
