using lzpress.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace lzpress.Engine
{
    class Merger
    {
        private List<string> _libraries;
        public Merger(List<string> libraries)
        {
            _libraries = libraries;
        }

        public bool Merge(string tempAssembly, string outputLocation)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Application.StartupPath + "\\ILMerge.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = tempAssembly.Quote() + " ";
            foreach(string lib in _libraries)
                startInfo.Arguments += lib.Quote() + " ";

            //TODO: See if we can get 4.5 to work, and check if the assembly location exists before using.
            string refAsms = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.5");
            startInfo.Arguments += $"/out:{outputLocation.Quote()} /targetplatform:\"v4,{refAsms}\"";

            using(Process proc = Process.Start(startInfo))
                proc?.WaitForExit();


            //File.Delete(tempAssembly);
            File.Delete(outputLocation.Replace(".exe", ".pdb"));

            return File.Exists(outputLocation);
        }
    }
}
