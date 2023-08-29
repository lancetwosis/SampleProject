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

namespace RedmineTimePuncher.ViewModels.Visualize.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ViewType
    {
        BarChart,
        PieChart,
        TreeMap

        //Unselected  = -1,
        //[LocalizedDescription(nameof(Resources.AppModeInputTime), typeof(Resources))]
        //TimePuncher,
        //[LocalizedDescription(nameof(Resources.AppModeShowTimeEntry), typeof(Resources))]
        //EntryViewer,
        //[LocalizedDescription(nameof(Resources.AppModeTableEditor), typeof(Resources))]
        //TableEditor,
        //[LocalizedDescription(nameof(Resources.AppModeTicketCreater), typeof(Resources))]
        //TicketCreater,
        //[LocalizedDescription(nameof(Resources.AppModeVisualize), typeof(Resources))]
        //Visualizer,
        //[LocalizedDescription(nameof(Resources.AppModeCountWiki), typeof(Resources))]
        //WikiPageCounter,
    }

}
