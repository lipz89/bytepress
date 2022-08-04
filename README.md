# bytepress
Compressor for .NET executables. Utilizes Codedom to build a wrapper executable using reflection to decompress and run the original executable in memory.

# Supported Algorithms
[.NET's DeflateStream (Gzip)](http://msdn.microsoft.com/en-us/library/system.io.compression.deflatestream%28v=vs.110%29.aspx)<br>
[QuickLZ](http://www.quicklz.com)<br>
[LZMA](http://www.7-zip.org/sdk.html)

# Demo
![Alt text](https://i.imgur.com/RInMrDd.png "Demo")

# Usage
```
bytepress mainfile.exe
bytepress mainfile.exe -a lzma
bytepress mainfile.exe -a lzma -l Newtonsoft.Json.dll
bytepress mainfile.exe -a lzma -l Newtonsoft.Json.dll -wpf
```
# Credits
[jerkimball](https://stackoverflow.com/users/48692/jerkimball) - Size extension class<br>
[TsudaKageyu](https://github.com/TsudaKageyu/IconExtractor) - Icon extractor
