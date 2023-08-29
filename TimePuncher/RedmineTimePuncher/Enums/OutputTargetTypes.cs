using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using LibRedminePower.Extentions;
using RedmineTimePuncher.Converters;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum OutputTargetTypes
    {
        [LocalizedDescription(nameof(Resources.enumOutputTargetRedmine), typeof(Resources))]
        Redmine,
        [LocalizedDescription(nameof(Resources.enumOutputTargetCsvExport), typeof(Resources))]
        CsvExport,
        [LocalizedDescription(nameof(Resources.enumOutputTargetExtTool), typeof(Resources))]
        ExtTool,
    }

    public static class OutputTargetTypesEx
    {
        public static Bitmap GetIconResource(this OutputTargetTypes type, string warningMsg)
        {
            string resName = "save";
            switch (type)
            {
                case Enums.OutputTargetTypes.Redmine:
                    resName = "save";
                    break;
                case Enums.OutputTargetTypes.CsvExport:
                    resName = "export_excel";
                    break;
                case Enums.OutputTargetTypes.ExtTool:
                    resName = "export_tool";
                    break;
                default:
                    throw new InvalidOperationException();
            }
            resName += !string.IsNullOrEmpty(warningMsg) ? "_w" : "";

            object obj = Resources.ResourceManager.GetObject(resName);
            if (obj == null) throw new InvalidOperationException();

            return obj as System.Drawing.Bitmap;
        }
    }
}
