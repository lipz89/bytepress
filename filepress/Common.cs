using bytepress.Compresison;
using bytepress.Compresison.LZMA;
using System;
using System.Collections.Generic;

namespace filepress
{
    static class Common
    {
        private static List<ICompressor> _compressors = new List<ICompressor>();
        static Common()
        {
            _compressors.Add(new GZIP());
            _compressors.Add(new QuickLZ());
            _compressors.Add(new LZMAez());
        }
        public static ICompressor GetCompressor(string algorithm, bool isRequired = false)
        {
            algorithm = algorithm.ToLower().Trim();
            switch(algorithm)
            {
                case "gzip":
                    return _compressors[0];
                case "qlz":
                    return _compressors[1];
                case "lzma":
                    return _compressors[2];
                default:
                    if(isRequired)
                    {
                        throw new Exception("压缩算法不支持");
                    }
                    return _compressors[1];
            }
        }
        //public static int Size(int size)
        //{
        //    if(size <= 4)
        //    {
        //        return 4;
        //    }
        //    if(size % 4 == 0)
        //    {
        //        return size;
        //    }
        //    return ((size / 4) + 1) * 4;
        //}
    }
}
