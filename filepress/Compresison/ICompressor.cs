﻿namespace bytepress.Compresison
{
    public interface ICompressor
    {
        string Name { get;  }
        byte[] Compress(byte[] data);
        byte[] Decompress(byte[] data);
    }
}
