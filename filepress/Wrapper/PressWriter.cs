using bytepress.Compresison;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace filepress
{

    class PressReader : IDisposable
    {
        private readonly Stream stream;
        private readonly BinaryReader reader;
        private bool isDisposed = false;
        private ICompressor compressor;
        public PressReader(byte[] input)
        {
            Console.WriteLine($"解压开始...");
            stream = new MemoryStream(input);
            reader = new BinaryReader(stream);
            this.ReadHeader();
            this.ReadCompressor();
            this.ReadRemark();
        }

        private void ReadHeader()
        {
            var length = this.reader.ReadInt32();
            var bytes = this.reader.ReadBytes(length);
            var header = Encoding.UTF8.GetString(bytes);
            if(header != Header.DATA)
            {
                throw new Exception("文件格式不正确");
            }
        }
        private void ReadCompressor()
        {
            var length = this.reader.ReadInt32();
            var bytes = this.reader.ReadBytes(length);
            var algo = Encoding.UTF8.GetString(bytes);
            this.compressor = Common.GetCompressor(algo, true);
            Console.WriteLine($"检查到算法 {compressor.Name} ...");
        }
        private void ReadRemark()
        {
            var length = this.reader.ReadInt32();
            var bytes = this.reader.ReadBytes(length);
        }
        private void ReadNodes(string output)
        {
            Console.WriteLine($"开始抽取...");
            var nodeCount = this.reader.ReadInt32();
            if(nodeCount == 0)
            {
                this.ReadFile(output);
            }
            else
            {
                this.ReadDirectory(output, nodeCount);
            }
        }
        private FileNode ReadFile(string path, int index = 0)
        {
            var nameLength = this.reader.ReadInt32();
            var nameBuffer = this.reader.ReadBytes(nameLength);
            var name = Encoding.UTF8.GetString(nameBuffer);

            if(index > 0)
            {
                Console.WriteLine($"  抽取第 {index} 个文件 {name} ...");
            }
            else
            {
                Console.WriteLine($"  抽取文件 {name} ...");
            }
            var dataLength = this.reader.ReadInt32();
            var dataBuffer = this.reader.ReadBytes(dataLength);
            Console.WriteLine($"    数据大小 {dataBuffer.Length.ToPrettySize(2)}");
            byte[] data = null;
            if(dataBuffer.Length > 0)
            {
                data = this.compressor.Decompress(dataBuffer);
                Console.WriteLine($"    文件大小 {data.Length.ToPrettySize(2)}");
            }
            else
            {
                data = dataBuffer;
                Console.WriteLine($"    文件大小 {data.Length.ToPrettySize(2)}");
            }

            var fn = new FileNode(name, data);
            SaveFile(path, fn);
            return fn;
        }
        private DirectoryNode ReadDirectory(string path, int count)
        {
            var nameLength = this.reader.ReadInt32();
            var nameBuffer = this.reader.ReadBytes(nameLength);
            var name = Encoding.UTF8.GetString(nameBuffer);
            Console.WriteLine($"  抽取文件夹 {name} ...");
            var dNode = new DirectoryNode(name);
            Console.WriteLine($"  有 {count} 个文件待抽取...");
            var dpath = Path.Combine(path, dNode.Name.Value);
            for(int i = 0; i < count; i++)
            {
                dNode.Items.Add(ReadFile(dpath, i));
            }
            return dNode;
        }

        public void Save(string output)
        {
            if(this.isDisposed)
            {
                throw new Exception("对象已被释放，无法使用。");
            }

            this.ReadNodes(output);
        }

        private void SaveFile(string output, FileNode fn)
        {
            var path = Path.Combine(output, fn.Name.Value);
            var dirPath = Path.GetDirectoryName(path);
            if(!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(path, fn.Data.GetBuffer());
        }

        public void Dispose()
        {
            this.compressor = null;
            this.reader?.Dispose();
            this.stream?.Dispose();
            this.isDisposed = true;
        }
    }

    class PressWriter
    {
        private List<ISection> sections = new List<ISection>();
        private Node node;
        private PressWriter(ICompressor compressor)
        {
            Console.WriteLine("压缩开始...");
            this.sections.Add(new Header());//固定头信息
            Console.WriteLine($"使用算法 {compressor.Name} ...");
            this.sections.Add(new Algorithm(compressor));//算法名称
            this.sections.Add(new Remark(60));//预留64个字节长度
        }
        public PressWriter(ICompressor compressor, FileSystemInfo fileSystem) : this(compressor)
        {
            if(fileSystem is FileInfo file)
            {
                Console.WriteLine($"加载文件 {file.FullName} ...");
                byte[] original = File.ReadAllBytes(file.FullName);
                Console.WriteLine($"  文件原大小 {original.Length.ToPrettySize(2)}");
                var data = compressor.Compress(original);
                Console.WriteLine($"  压缩后大小 {data.Length.ToPrettySize(2)}");
                double compressionRatio = 100 - (double)data.Length / original.Length * 100.0;
                Console.WriteLine($"    压缩比率 {compressionRatio:F}%");
                this.node = new FileNode(file.Name, data);
            }
            else if(fileSystem is DirectoryInfo directory)
            {
                Console.WriteLine($"加载文件夹 {directory.FullName} ...");
                var dNode = new DirectoryNode(directory.Name);
                var files = directory.GetFiles("*", SearchOption.AllDirectories);
                var dirPath = directory.FullName.Length + 1;
                Console.WriteLine($"有 {files.Length} 个文件待加载 {directory.FullName} ...");
                var index = 1;
                foreach(var fi in files)
                {
                    var fileName = fi.FullName.Substring(dirPath);
                    Console.WriteLine($"  加载第 {index} 个文件 {fileName} ...");
                    byte[] original = File.ReadAllBytes(fi.FullName);
                    Console.WriteLine($"    文件原大小 {original.Length.ToPrettySize(2)}");
                    var data = compressor.Compress(original);
                    Console.WriteLine($"    压缩后大小 {data.Length.ToPrettySize(2)}");
                    double compressionRatio = 100 - (double)data.Length / original.Length * 100.0;
                    Console.WriteLine($"    压缩比率 {compressionRatio:F}%");
                    dNode.Items.Add(new FileNode(fileName, data));
                    index++;
                }
                dNode.Count = dNode.Items.Count;
                this.node = dNode;
            }
        }

        public byte[] Write()
        {
            Console.WriteLine($"开始汇总压缩数据...");
            using(var ms = new MemoryStream())
            {
                using(BinaryWriter br = new BinaryWriter(ms))
                {
                    foreach(var item in this.sections)
                    {
                        br.Write(item.Length);
                        br.Write(item.GetBuffer());
                    }
                    if(this.node is FileNode fn)
                    {
                        br.Write(0);
                        WriteNode(br, fn);
                    }
                    else if(this.node is DirectoryNode dn)
                    {
                        br.Write(dn.Count);
                        WriteNode(br, dn);
                    }
                }
                return ms.ToArray();
            }
        }
        private void WriteNode(BinaryWriter br, DirectoryNode node)
        {
            br.Write(node.Name.Length);
            br.Write(node.Name.GetBuffer());
            foreach(var n in node.Items)
            {
                WriteNode(br, n);
            }
        }

        private void WriteNode(BinaryWriter br, FileNode node)
        {
            br.Write(node.Name.Length);
            br.Write(node.Name.GetBuffer());
            br.Write(node.Data.Length);
            br.Write(node.Data.GetBuffer());
        }
    }

    interface ISection
    {
        int Length { get; }
        byte[] GetBuffer();
    }

    class Data : ISection
    {
        private readonly byte[] buffer;
        public Data(byte[] data)
        {
            this.Length = data.Length;
            this.buffer = new byte[this.Length];
            data.CopyTo(this.buffer, 0);
        }
        public int Length { get; }
        public byte[] GetBuffer() => this.buffer;
    }

    class Header : Varchar
    {
        internal const string DATA = "plz";
        public Header() : base(DATA)
        {
        }
    }

    class Algorithm : Varchar
    {
        public Algorithm(ICompressor compressor) : base(compressor.Name)
        {
        }
    }

    class Remark : ISection
    {
        private readonly byte[] buffer;
        public Remark(int size = 4)
        {
            this.Length = size;
            this.buffer = new byte[this.Length];
        }
        public int Length { get; }
        public byte[] GetBuffer() => this.buffer;
    }

    class Varchar : Data
    {
        internal string Value { get; }
        public Varchar(string data) : base(Encoding.UTF8.GetBytes(data))
        {
            this.Value = data;
        }
    }

    abstract class Node
    {
        public Node(string name)
        {
            this.Name = new Varchar(name);
        }
        public Varchar Name { get; }
    }

    class FileNode : Node
    {
        public FileNode(string name, byte[] data) : base(name)
        {
            this.Data = new Data(data);
        }
        public Data Data { get; }
    }

    class DirectoryNode : Node
    {
        public DirectoryNode(string name) : base(name)
        {
            this.Items = new List<FileNode>();
        }
        public List<FileNode> Items { get; }
        public int Count { get; internal set; }
    }
}
