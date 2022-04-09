using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoR2EditorKit.Utilities
{
    /// <summary>
    /// General System.IO related utilities.
    /// </summary>
    public static class IOUtils
    {
        /// <summary>
        /// If the directory specified in <paramref name="directoryPath"/> does not exist, it creates it.
        /// </summary>
        /// <param name="directoryPath">The directory path to ensure its existence</param>
        public static void EnsureDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }
    }
}
