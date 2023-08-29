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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls.ChartView;

namespace RedmineTimePuncher.ViewModels.Visualize.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FactorType
    {
        None,
        Date,
        Issue,
        Project,
        User,
        Category,
        ASC,
        DESC,
        Center,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        OnTime,

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

    public static class FactorTypeEx
    {
        private static Dictionary<(FactorType, string), Brush> colorDic = new Dictionary<(FactorType, string), Brush>();

        private static List<Brush> issueColors = getColors(ChartPalettes.Flower);
        private static List<Brush> userColors = getColors(ChartPalettes.Spring);
        private static List<Brush> projectColors = getColors(ChartPalettes.Windows8);
        private static List<Brush> defaultColors = getColors(ChartPalettes.Office2019);

        private static List<Brush> getColors(params ChartPalette[] palettes)
        {
            return palettes.SelectMany(p => p.GlobalEntries).Select(a => a.Fill).ToList();
        }

        private static int index;
        public static Brush GetColor(this FactorType type, string title, int? id = null)
        {
            if (colorDic.TryGetValue((type, title), out var color))
            {
                return color;
            }

            switch (type)
            {
                case FactorType.Issue:
                    colorDic[(type, title)] = getModifiedBrush(id.Value, issueColors);
                    return colorDic[(type, title)];
                case FactorType.User:
                    colorDic[(type, title)] = getModifiedBrush(id.Value, userColors);
                    return colorDic[(type, title)];
                case FactorType.Project:
                    colorDic[(type, title)] = getModifiedBrush(id.Value, projectColors);
                    return colorDic[(type, title)];
                default:
                    colorDic[(type, title)] = defaultColors[index % defaultColors.Count];
                    index++;
                    return colorDic[(type, title)];
            }
        }

        private static SolidColorBrush getModifiedBrush(int id, List<Brush> colors)
        {
            // id から元となる色を決定
            var brush = colors[id % colors.Count] as SolidColorBrush;

            // id の各桁の合計で明度を調整
            var digiSum = id.ToString().ToCharArray().Select(a => int.Parse(a.ToString())).Sum();
            var brightness = 0.08 * (digiSum % 5);

            var hsb = brush.Color.ToHsb();
            if (hsb.Brightness + brightness < 0.97)
                hsb.Brightness += brightness;
            else
                hsb.Brightness -= brightness;

            return new SolidColorBrush(hsb.ToRgb());
        }
    }
}
