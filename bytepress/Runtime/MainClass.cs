﻿//using System;
//using System.IO;
//using System.IO.Compression;
//using System.Reflection;
//using System.Threading;
//using System.Runtime.InteropServices;

//[assembly: AssemblyTitle("")]
//[assembly: AssemblyDescription("")]
//[assembly: AssemblyCompany("")]
//[assembly: AssemblyProduct("")]
//[assembly: AssemblyCopyright("")]
//[assembly: ComVisible(false)]
//[assembly: Guid("")]
//[assembly: AssemblyVersion("")]
//[assembly: AssemblyFileVersion("")]

//namespace bytepress
//{
//    class Program
//    {
//        private static Assembly lib;
//        private static void Main(string[] args)
//        {
//            try
//            {
//                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("bytepress.lib.dll"))
//                {
//                    using (MemoryStream ms = new MemoryStream())
//                    {
//                        stream.CopyTo(ms);
//                        lib = Assembly.Load(ms.ToArray());
//                    }
//                }
//                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("data"))
//                using (MemoryStream ms = new MemoryStream())
//                {
//                    stream.CopyTo(ms);
//                    byte[] data = Decompress(ms.ToArray(), *type *);
//                    if (data == null || data.Length == 0)
//                        throw new Exception("Failed to decompress file");
//                    RunHelper.Run(data, args);
//                    Array.Clear(data, 0, data.Length);
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.ToString());
//            }

//        }

//        /// <summary>
//        /// Invokes the library that contains the decompression methods with the correct algorithm type.
//        /// </summary>
//        private static byte[] Decompress(byte[] data, int compressionType)
//        {
//            MethodInfo d = lib.GetType("bytepress.lib.Main").GetMethod("Decompress");
//            return (byte[])d.Invoke(null, new object[] { data, compressionType });
//        }
//    }

//    class RunHelper
//    {
//        /// <summary>
//        /// Invokes the assembly in memory with the correct parameters (if applicable).
//        /// </summary>
//        public static bool Run(byte[] assemblyBytes, string[] arguments)
//        {
//            Assembly assembly = AppDomain.CurrentDomain.Load(assemblyBytes);
//            MethodInfo entryPoint = assembly.EntryPoint;
//            object instance = assembly.CreateInstance(entryPoint.Name);

//            ManualResetEvent ensureLoaded = new ManualResetEvent(false);
//            bool loaded = false;

//            object[] parameters = GetMethodParameters(entryPoint, arguments);

//            try
//            {
//                entryPoint.Invoke(instance, parameters);
//                loaded = true;
//            }
//            catch (TargetInvocationException invocationEx)
//            {
//                loaded = false;
//            }
//            finally
//            {
//                ensureLoaded.Set();
//            }

//            ensureLoaded.WaitOne();
//            ensureLoaded.Dispose();

//            return loaded;
//        }

//        /// <summary>
//        /// Builds the necessary parameters to invoke the main method (if applicable).
//        /// </summary>
//        private static object[] GetMethodParameters(MethodInfo method, string[] optionalParameters)
//        {
//            if (method.GetParameters().Length == 0)
//                return null;

//            if (optionalParameters != null)
//                return new object[] { optionalParameters };

//            return new object[] { null };
//        }
//    }
//}