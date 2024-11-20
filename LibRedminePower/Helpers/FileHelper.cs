using LibRedminePower.Extentions;
using LibRedminePower.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Helpers
{
    public static class FileHelper
    {
        public static void WriteAllText(string fileName, string text)
        {
            System.IO.File.WriteAllText(fileName, text, System.Text.Encoding.UTF8);
            System.Diagnostics.Process.Start("EXPLORER.EXE", "/select, \"" + fileName + "\"");
        }

        public static T ReadAllText<T>(string fileName) where T : class, new()
        {
            var json = System.IO.File.ReadAllText(fileName);
            return CloneExtentions.ToObject<T>(json);
        }
    }
}
