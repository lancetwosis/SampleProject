using RedmineTimePuncher.Enums;
using RedmineTimePuncher.ViewModels.Input.Resources.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels.Input.Controls
{
    public class MyTimeIndicator : TimeIndicator
    {
        public DateTime DateTime { get; set; }
        public Brush Brush { get; }
        public DoubleCollection StrokeDashArray { get; set; } = new DoubleCollection(new[] { 1.0 });
        public double StrokeThickness { get; set; } = 3.0;
        public string ToolTip { get; set; }

        public MyTimeIndicator(Brush brush)
        {
            this.Brush = brush;
            this.Location = CurrentTimeIndicatorLocation.WholeArea;
        }

        public override DateTime GetDateTime()
        {
            return this.DateTime;
        }

        public override string ToString()
        {
            return $"{ToolTip} ({Brush}) - {DateTime}";
        }
    }
}