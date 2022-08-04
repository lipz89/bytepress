﻿using System;
using System.IO;
using System.Text;

namespace press
{
    static class pr
    {
        static int hl(byte[] source)
        {
            return ((source[0] & 2) == 2) ? 9 : 3;
        }

        static int sd(byte[] source)
        {
            if(hl(source) == 9)
                return source[5] | (source[6] << 8) | (source[7] << 16) | (source[8] << 24);
            else
                return source[2];
        }

        static byte[] dc(byte[] source)
        {
            int level;
            int size = sd(source);
            int src = hl(source);
            int dst = 0;
            uint cword_val = 1;
            byte[] destination = new byte[size];
            int[] hashtable = new int[4096];
            byte[] hash_counter = new byte[4096];
            int last_matchstart = size - 6 - 4 - 1;
            int last_hashed = -1;
            int hash;
            uint fetch = 0;

            level = (source[0] >> 2) & 0x3;

            if(level != 1 && level != 3)
                throw new ArgumentException("C# version only supports level 1 and 3");

            if((source[0] & 1) != 1)
            {
                byte[] d2 = new byte[size];
                Array.Copy(source, hl(source), d2, 0, size);
                return d2;
            }

            while(true)
            {
                if(cword_val == 1)
                {
                    cword_val = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16) | (source[src + 3] << 24));
                    src += 4;
                    if(dst <= last_matchstart)
                    {
                        if(level == 1)
                            fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16));
                        else
                            fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16) | (source[src + 3] << 24));
                    }
                }

                if((cword_val & 1) == 1)
                {
                    uint matchlen;
                    uint offset2;

                    cword_val = cword_val >> 1;

                    if(level == 1)
                    {
                        hash = ((int)fetch >> 4) & 0xfff;
                        offset2 = (uint)hashtable[hash];

                        if((fetch & 0xf) != 0)
                        {
                            matchlen = (fetch & 0xf) + 2;
                            src += 2;
                        }
                        else
                        {
                            matchlen = source[src + 2];
                            src += 3;
                        }
                    }
                    else
                    {
                        uint offset;
                        if((fetch & 3) == 0)
                        {
                            offset = (fetch & 0xff) >> 2;
                            matchlen = 3;
                            src++;
                        }
                        else if((fetch & 2) == 0)
                        {
                            offset = (fetch & 0xffff) >> 2;
                            matchlen = 3;
                            src += 2;
                        }
                        else if((fetch & 1) == 0)
                        {
                            offset = (fetch & 0xffff) >> 6;
                            matchlen = ((fetch >> 2) & 15) + 3;
                            src += 2;
                        }
                        else if((fetch & 127) != 3)
                        {
                            offset = (fetch >> 7) & 0x1ffff;
                            matchlen = ((fetch >> 2) & 0x1f) + 2;
                            src += 3;
                        }
                        else
                        {
                            offset = (fetch >> 15);
                            matchlen = ((fetch >> 7) & 255) + 3;
                            src += 4;
                        }
                        offset2 = (uint)(dst - offset);
                    }

                    destination[dst + 0] = destination[offset2 + 0];
                    destination[dst + 1] = destination[offset2 + 1];
                    destination[dst + 2] = destination[offset2 + 2];

                    for(int i = 3; i < matchlen; i += 1)
                    {
                        destination[dst + i] = destination[offset2 + i];
                    }

                    dst += (int)matchlen;

                    if(level == 1)
                    {
                        fetch = (uint)(destination[last_hashed + 1] | (destination[last_hashed + 2] << 8) | (destination[last_hashed + 3] << 16));
                        while(last_hashed < dst - matchlen)
                        {
                            last_hashed++;
                            hash = (int)(((fetch >> 12) ^ fetch) & (4096 - 1));
                            hashtable[hash] = last_hashed;
                            hash_counter[hash] = 1;
                            fetch = (uint)((fetch >> 8 & 0xffff) | destination[last_hashed + 3] << 16);
                        }
                        fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16));
                    }
                    else
                    {
                        fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16) | (source[src + 3] << 24));
                    }
                    last_hashed = dst - 1;
                }
                else
                {
                    if(dst <= last_matchstart)
                    {
                        destination[dst] = source[src];
                        dst += 1;
                        src += 1;
                        cword_val = cword_val >> 1;

                        if(level == 1)
                        {
                            while(last_hashed < dst - 3)
                            {
                                last_hashed++;
                                int fetch2 = destination[last_hashed] | (destination[last_hashed + 1] << 8) | (destination[last_hashed + 2] << 16);
                                hash = ((fetch2 >> 12) ^ fetch2) & (4096 - 1);
                                hashtable[hash] = last_hashed;
                                hash_counter[hash] = 1;
                            }
                            fetch = (uint)((fetch >> 8 & 0xffff) | source[src + 2] << 16);
                        }
                        else
                        {
                            fetch = (uint)((fetch >> 8 & 0xffff) | source[src + 2] << 16 | source[src + 3] << 24);
                        }
                    }
                    else
                    {
                        while(dst <= size - 1)
                        {
                            if(cword_val == 1)
                            {
                                src += 4;
                                cword_val = 0x80000000;
                            }

                            destination[dst] = source[src];
                            dst++;
                            src++;
                            cword_val = cword_val >> 1;
                        }
                        return destination;
                    }
                }
            }
        }

        public static void rs(byte[] input, string output)
        {
            var stream = new MemoryStream(input);
            var reader = new BinaryReader(stream);
            var hl = reader.ReadInt32();
            var h = Encoding.UTF8.GetString(reader.ReadBytes(hl));
            if(h != "plz")
            {
                throw new Exception("文件格式不正确");
            }

            var al = reader.ReadInt32();
            var a = Encoding.UTF8.GetString(reader.ReadBytes(al));
            if(a != "qlz")
            {
                throw new Exception("文件格式不正确");
            }

            reader.ReadBytes(64);
            var c = reader.ReadInt32();
            if(c == 0)
            {
                var nl = reader.ReadInt32();
                var n = Encoding.UTF8.GetString(reader.ReadBytes(nl));
                var dl = reader.ReadInt32();
                var d = reader.ReadBytes(dl);
                var fn = Path.Combine(output, n);
                var dcd = d.Length == 0 ? d : dc(d);
                var dirPath = Path.GetDirectoryName(fn);
                if(!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                File.WriteAllBytes(fn, dcd);
            }
            else
            {
                var fl = reader.ReadInt32();
                var f = Encoding.UTF8.GetString(reader.ReadBytes(fl));
                for(int i = 0; i < c; i++)
                {
                    var nl = reader.ReadInt32();
                    var n = Encoding.UTF8.GetString(reader.ReadBytes(nl));
                    var dl = reader.ReadInt32();
                    var d = reader.ReadBytes(dl);
                    var fn = Path.Combine(output, f, n);
                    var dcd = d.Length == 0 ? d : dc(d);
                    var dirPath = Path.GetDirectoryName(fn);
                    if(!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }
                    File.WriteAllBytes(fn, dcd);
                }
            }
        }
    }
}
