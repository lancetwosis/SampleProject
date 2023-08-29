using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using LibRedminePower.Extentions;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]

    // 定義の順番が NavigationView での表示順と処理に影響するので注意すること
    public enum ApplicationMode
    {
        Unselected  = -1,
        [LocalizedDescription(nameof(Resources.AppModeInputTime), typeof(Resources))]
        TimePuncher,
        [LocalizedDescription(nameof(Resources.AppModeShowTimeEntry), typeof(Resources))]
        EntryViewer,
        [LocalizedDescription(nameof(Resources.AppModeTableEditor), typeof(Resources))]
        TableEditor,
        [LocalizedDescription(nameof(Resources.AppModeTicketCreater), typeof(Resources))]
        TicketCreater,
        [LocalizedDescription(nameof(Resources.AppModeVisualize), typeof(Resources))]
        Visualizer,
        [LocalizedDescription(nameof(Resources.AppModeCountWiki), typeof(Resources))]
        WikiPageCounter,
    }

    public static class ApplicationModeEx
    {
        public static BitmapSource GetIcon(this ApplicationMode mode)
        {
            switch (mode)
            {
                case ApplicationMode.TimePuncher:
                    return Resources.rtp32.ToBitmapSource();
                case ApplicationMode.EntryViewer:
                    return Resources.icons8_select_all_files_48.ToBitmapSource();
                case ApplicationMode.WikiPageCounter:
                    return Resources.icons8_count.ToBitmapSource();
                case ApplicationMode.TicketCreater:
                    return Resources.icons8_review.ToBitmapSource();
                case ApplicationMode.TableEditor:
                    return Resources.rte32.ToBitmapSource();
                case ApplicationMode.Visualizer:
                    return Resources.icons8_analytics_48.ToBitmapSource();
                case ApplicationMode.Unselected:
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
