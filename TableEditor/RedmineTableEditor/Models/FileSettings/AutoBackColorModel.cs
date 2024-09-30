using LibRedminePower.Extentions;
using RedmineTableEditor.Models.TicketFields.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTableEditor.Models.FileSettings
{
    public class AutoBackColorModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public StatusColorsModel StatusColors { get; set; } = new StatusColorsModel();
        public AssignedToColorsModel AssignedToColors { get; set; } = new AssignedToColorsModel();

        public static SolidColorBrush TRANSPARENT = new SolidColorBrush(Colors.Transparent);
        public static SolidColorBrush DISABLE_BACKGROUND = new SolidColorBrush(Colors.DimGray);

        public SolidColorBrush GetColor(RedmineManager redmine, int? statusId, string assignedTo)
        {
            if (StatusColors.IsEnabled)
                return getStatusColor(statusId);
            else if (AssignedToColors.IsEnabled)
                return getAssignedToColor(redmine, statusId, assignedTo);
            else
                return TRANSPARENT;
        }

        private SolidColorBrush getStatusColor(int? id)
        {
            var color = StatusColors.Items.FirstOrDefault(a => a.Id == id);
            return color != null ? getBrush(color.Color) : TRANSPARENT;
        }

        private SolidColorBrush getAssignedToColor(RedmineManager r, int? statusId, string assignedTo)
        {
            if (!string.IsNullOrEmpty(r.IsValid()))
                return TRANSPARENT;

            var status = r.Cache.Statuss.FirstOrDefault(a => a.Id == statusId);
            if (status != null && status.IsClosed && AssignedToColors.IsEnabledClosed)
            {
                return getStatusColor(statusId);
            }

            if (string.IsNullOrEmpty(assignedTo))
                return TRANSPARENT;

            var color = AssignedToColors.Items.Where(a => a.Name != null).FirstOrDefault(a => assignedTo.Contains(a.Name));
            return color != null ? getBrush(color.Color) : TRANSPARENT;
        }

        private static Dictionary<System.Drawing.Color, SolidColorBrush> brushDic = new Dictionary<System.Drawing.Color, SolidColorBrush>();
        private SolidColorBrush getBrush(System.Drawing.Color color)
        {
            SolidColorBrush brush;
            if (brushDic.TryGetValue(color, out brush))
            {
                return brush;
            }
            else
            {
                brush = color.ToBrush();
                brushDic.Add(color, brush);
                return brush;
            }
        }
    }
}
