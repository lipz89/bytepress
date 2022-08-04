using System;
using System.Collections.Generic;
using System.IO;
using lzpress.Compresison;
using lzpress.Extensions;

namespace lzpress.Engine
{
    public class Presser
    {
        private string _source;
        private string _file;
        private byte[] _fileBytes;
        private byte[] _fileBytesCompressed;
        private bool _isWpf;
        private AssemblyCloner _cloner;
        private List<string> _libraries;
        public UpdateHandler UpdateStatus = delegate { };
        public delegate void UpdateHandler(string status, StatusType type);

        public enum StatusType
        {
            Normal,
            Warning,
            Error
        };

        public Presser(string file, bool isWpf)
        {
            _file = file;
            _isWpf = isWpf;
            _source = Properties.Resources.source;
            _cloner = new AssemblyCloner(_file);
            CleanWorkspace();
        }

        public Presser(string file, byte[] data, bool isWpf)
        {
            _file = file;
            _fileBytes = data;
            _isWpf = isWpf;
            _source = Properties.Resources.source;
            _cloner = new AssemblyCloner(_file);
            CleanWorkspace();
        }

        /// <summary>
        /// Verifies and sets the libraries to be merged with the main assembly.
        /// </summary>
        /// <param name="libraries"></param>
        public void MergeLibraries(List<string> libraries)
        {
            foreach(string lib in libraries)
            {
                if(!lib.ToLower().EndsWith(".dll"))
                    throw new Exception("Additional files must be .NET libraries (.dll).");

                byte[] temp = File.ReadAllBytes(lib);
                if(!IsManagedAssembly(temp))
                    throw new Exception("Libraries to merge must be valid .NET assemblies.");
                Array.Clear(temp, 0, temp.Length);
            }
            _libraries = libraries;
        }

        /// <summary>
        /// Checks the .NET header conventions to determine if assembly is managed (.NET).
        /// </summary>
        private bool IsManagedAssembly(byte[] payloadBuffer)
        {
            int e_lfanew = BitConverter.ToInt32(payloadBuffer, 0x3c);//0x3c:80
            int magicNumber = BitConverter.ToInt16(payloadBuffer, e_lfanew + 0x18);  //0x54:72
            int isManagedOffset = magicNumber == 0x10B ? 0xE8 : 0xF8;
            int isManaged = BitConverter.ToInt32(payloadBuffer, e_lfanew + isManagedOffset);// 0x10b ? 0x168 :0x178
            return isManaged != 0;
        }

        /// <summary>
        /// Compresses the supplied executable and generates a wrapper program to load it in memory.
        /// </summary>
        public void Process()
        {
            string outLocation;
            FileInfo f = new FileInfo(_file);
            if(_fileBytes == null)
                _fileBytes = File.ReadAllBytes(_file);

            UpdateStatus("Verifying file is .NET assembly...", StatusType.Normal);
            if(!IsManagedAssembly(_fileBytes))
                throw new Exception("Only .NET executable files are supported");

            if(_libraries != null && _libraries.Count > 0)
            {
                outLocation = Path.Combine(f.DirectoryName, f.Name.Replace(".exe", "_merge.exe"));

                UpdateStatus("Merging additional libraries...", StatusType.Normal);
                Merger m = new Merger(_libraries);
                if(!m.Merge(_file, outLocation))
                    throw new Exception("Failed to merge libraries");

                _fileBytes = File.ReadAllBytes(outLocation);

                if(File.Exists(outLocation))
                    File.Delete(outLocation);
            }

            Compress();

            UpdateStatus("Writing compressed payload to %temp%...", StatusType.Normal);
            File.WriteAllBytes(Path.GetTempPath() + "data", _fileBytesCompressed);

            UpdateStatus("Copying assembly information and icon...", StatusType.Normal);
            CopyAssembly();

            UpdateStatus("Compiling...", StatusType.Normal);

            var outPath = Path.Combine(f.DirectoryName, "output");
            if(!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }

            outLocation = Path.Combine(outPath, f.Name);
            Compiler comp = new Compiler
            {
                CompileLocation = outLocation,
                ResourceFiles = new[] { Path.GetTempPath() + "data" },
                SourceCodes = new[] { _source },
                References = new[] {
                    "System.dll",
                    "System.Reflection.dll",
                },

                Icon = Path.GetTempPath() + "icon.ico"
            };
            if(_isWpf)
            {
                // Credit: https://stackoverflow.com/questions/12429917/load-wpf-application-from-the-memory
                comp.WPFReferences = new[]
                {
                    "System.Xaml.dll",
                    "PresentationCore.dll",
                    "PresentationFramework.dll",
                    "WindowsBase.dll"
                };
                _source = _source.Replace("//wpfhack", Properties.Resources.wpfhack);
                comp.SourceCodes = new[] { _source };
            }

            if(!comp.Compile())
            {
                CleanWorkspace();
                throw new Exception(comp.CompileError);
            }

            Console.WriteLine();

            byte[] compiled = File.ReadAllBytes(outLocation);
            double compressionRatio = 100 - (double)compiled.Length / _fileBytes.Length * 100.0;
            UpdateStatus("Compression Results", StatusType.Normal);
            UpdateStatus("--------------------------------------------", StatusType.Normal);
            UpdateStatus($"Initial File Size:    {_fileBytes.Length.ToPrettySize(2)}", StatusType.Normal);
            UpdateStatus($"Compressed File Size: {compiled.Length.ToPrettySize(2)}", StatusType.Normal);
            UpdateStatus($"Compression Ratio:    {compressionRatio:F}%", StatusType.Normal);

            if(compiled.Length > _fileBytes.Length)
                UpdateStatus("Warning: Compressed size is larger than original.", StatusType.Warning);
            UpdateStatus("--------------------------------------------", StatusType.Normal);
            Console.WriteLine();
            UpdateStatus("Cleaning up...", StatusType.Normal);
            CleanWorkspace();

            UpdateStatus($"Done! File successfully lzpressed to: {comp.CompileLocation}", StatusType.Normal);
        }

        /// <summary>
        /// Tests and chooses the best compression algorithm.
        /// </summary>
        private void Compress()
        {
            UpdateStatus($"Staring Compression...", StatusType.Normal);
            _fileBytesCompressed = new QuickLZ().Compress(_fileBytes);
            double cR = 100 - (double)_fileBytesCompressed.Length / _fileBytes.Length * 100.0;
            UpdateStatus($"Compression Ratio: {cR:F}%", StatusType.Normal);
        }

        /// <summary>
        /// Replaces the codedom assembly information with the information from the supplied executable.
        /// </summary>
        private void CopyAssembly()
        {
            _source = _source.Replace("[assembly: AssemblyTitle(\"\")]", $"[assembly: AssemblyTitle(\"{_cloner.Title}\")]");
            _source = _source.Replace("[assembly: AssemblyDescription(\"\")]", $"[assembly: AssemblyDescription(\"{_cloner.Description}\")]");
            _source = _source.Replace("[assembly: AssemblyCompany(\"\")]", $"[assembly: AssemblyCompany(\"{_cloner.Company}\")]");
            _source = _source.Replace("[assembly: AssemblyProduct(\"\")]", $"[assembly: AssemblyProduct(\"{_cloner.Product}\")]");
            _source = _source.Replace("[assembly: AssemblyCopyright(\"\")]", $"[assembly: AssemblyCopyright(\"{_cloner.Copyright}\")]");
            _source = _source.Replace("[assembly: Guid(\"\")]", "[assembly: Guid(\"" + Guid.NewGuid() + "\")]");
            _source = _source.Replace("[assembly: AssemblyVersion(\"\")]", $"[assembly: AssemblyVersion(\"{_cloner.Version}\")]");
            _source = _source.Replace("[assembly: AssemblyFileVersion(\"\")]", $"[assembly: AssemblyFileVersion(\"{_cloner.Version}\")]");

            if(_cloner.Icon != null)
                using(FileStream s = new FileStream(Path.GetTempPath() + "icon.ico", FileMode.CreateNew))
                    _cloner.Icon.Save(s);
        }

        /// <summary>
        /// Cleans the temporary files needed for compilation.
        /// </summary>
        private void CleanWorkspace()
        {
            string[] tempFiles =
            {
                "data",
                "icon.ico",
            };
            foreach(string file in tempFiles)
            {
                if(File.Exists(Path.GetTempPath() + file))
                    File.Delete(Path.GetTempPath() + file);
            }
        }
    }
}
