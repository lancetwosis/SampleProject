using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls.ChartView;

namespace TelerikEx.Helpers
{
    public static class ChartUtilities
    {
        // LabelDefinitions と SeriesProvider を一緒に使うための添付プロパティ
        // https://docs.telerik.com/devtools/wpf/controls/radchartview/features/labels/label-definition
        public static readonly DependencyProperty LabelDefinitionProperty =
            DependencyProperty.RegisterAttached(
                "LabelDefinition",
                typeof(ChartSeriesLabelDefinition),
                typeof(ChartUtilities),
                new PropertyMetadata(new ChartSeriesLabelDefinition(), OnLabelDefinitionChanged));

        public static ChartSeriesLabelDefinition GetLabelDefinition(DependencyObject obj)
        {
            return (ChartSeriesLabelDefinition)obj.GetValue(LabelDefinitionProperty);
        }

        public static void SetLabelDefinition(DependencyObject obj, ChartSeriesLabelDefinition value)
        {
            obj.SetValue(LabelDefinitionProperty, value);
        }

        private static void OnLabelDefinitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var series = (CartesianSeries)d;
            series.LabelDefinitions.Clear();
            if (e.NewValue != null)
            {
                var labelDefinition = (ChartSeriesLabelDefinition)e.NewValue;
                series.LabelDefinitions.Add(labelDefinition);
            }
        }
    }
}
