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
    }
}
