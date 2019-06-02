using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RXscanWPF
{
    public class Utils
    {
        /// <summary>
        /// Generates unique filenames by appending the next highest number to a path.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>New unique filename</returns>
        public static string GetNextFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            int i = 0;
            while (File.Exists(fileName))
            {
                if (i == 0)
                    fileName = fileName.Replace(extension, "(" + ++i + ")" + extension);
                else
                    fileName = fileName.Replace("(" + i + ")" + extension, "(" + ++i + ")" + extension);
            }

            return fileName;
        }

        public enum ColorMode
        {
            Greyscale,
            Color,
        }
    }
}
