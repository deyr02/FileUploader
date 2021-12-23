using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUploader
{
    class FileHandler
    {

        private static string baseURL = @"C:\C:\Program Files\FileUploader";

        private static bool IsDirectoryExist()
        {
            if (!Directory.Exists(baseURL))
            {
                Directory.CreateDirectory(baseURL);
            }
            return true;
        }

        public async Task WriteFile<T>(List<T> lists)
        {

        }

    }
}
