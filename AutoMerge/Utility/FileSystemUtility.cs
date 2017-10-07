using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMerge.Utility
{
    internal static class FileSystemUtility
    {
        internal static List<string> EnumerateFiles(string filePattern, string directory = null, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (!Directory.Exists(directory)) return new List<string>();
            return Directory.EnumerateFiles(
                (string.IsNullOrEmpty(directory) ? Directory.GetCurrentDirectory() : directory),
                filePattern,
                searchOption
            ).ToList();
        }

        internal static long GetFileSize(string fileName)
        {
            long length = 0;
            try {
                var fileInfo = new FileInfo(fileName);
                length = fileInfo.Length;
            } catch { }
            return length;
        }
    }
}
