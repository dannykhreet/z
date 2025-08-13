using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EZGO.Maui.Core.Models
{
    /// <summary>
    /// Represents a file item
    /// </summary>
    public class FileItem
    {
        public bool IsLocal { get; set; }
        public string Name { get; set; }
        public string AbsolutePath { get; set; }
        public string Url { get; set; }

        public bool IsEmpty => string.IsNullOrEmpty(Url) && string.IsNullOrEmpty(AbsolutePath);

        private FileItem()
        {

        }

        public static FileItem FromOnlineFile(string url)
        {
            return new FileItem()
            {
                IsLocal = false,
                Url = url,
                Name = Path.GetFileName(url),
            };
        }

        public static FileItem FromLocalFile(string absolutePath)
        {
            return new FileItem()
            {
                IsLocal = true,
                AbsolutePath = absolutePath,
                Name = Path.GetFileName(absolutePath),
            };
        }

        public static FileItem Empty => new FileItem();

    }
}
