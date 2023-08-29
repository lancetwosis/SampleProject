using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.TreeMap;

namespace RedmineTimePuncher.ViewModels.Visualize.TreeMapItems
{
    public class MyCutomMapping : CustomMapping
    {
        private static Dictionary<int, Brush> GROUP_BRUSH_DIC = new Dictionary<int, Brush>()
        {
            { 1,  new SolidColorBrush(Colors.WhiteSmoke)},
            { 2,  new SolidColorBrush(Colors.Gainsboro)},
            { 3,  new SolidColorBrush(Colors.LightGray)},
        };

        protected override void Apply(RadTreeMapItem treemapItem, object dataItem)
        {
            if (dataItem is GroupingItemViewModel group)
            {
                treemapItem.Background = GROUP_BRUSH_DIC[group.Depth];
                treemapItem.ToolTip = new TextBlock()
                {
                    Text = $"{group.ToolTip.Label} : Total {group.ToolTip.TotalHours.Value} h",
                };
            }
            else if (dataItem is TicketItemViewModel ticket)
            {
                treemapItem.Background = FactorType.Issue.GetColor(ticket.XLabel, ticket.Issue.Id);

                var text = new FrameworkElementFactory(typeof(TextBlock));
                text.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                text.SetValue(TextBlock.TextWrappingProperty, TextWrapping.WrapWithOverflow);
                text.SetValue(TextBlock.TextProperty, ticket.DisplayValue);

                var tmp = new DataTemplate();
                tmp.VisualTree = text;
                treemapItem.HeaderTemplate = tmp;

                var tooltip = new TextBlock();
                tooltip.Text = ticket.ToolTip.ShowTotal ?
                    $"{ticket.ToolTip.Label} : Total {ticket.ToolTip.TotalHours.Value} h" :
                    $"{ticket.ToolTip.Label} : {ticket.ToolTip.Hours} h";
                treemapItem.ToolTip = tooltip;

                var button = new RadButton();
                button.Command = ticket.GoToTicketCommand;
                button.HorizontalAlignment = HorizontalAlignment.Right;
                button.VerticalAlignment = VerticalAlignment.Bottom;
                button.Style = Application.Current.FindResource("GotoTicketButtonStyle") as Style;
                if (ticket.Children.Any())
                {
                    button.Margin = new Thickness(0, 0, 1, 0);
                }
                else
                {
                    Grid.SetRow(button, 1);
                }

                var grid = treemapItem.ChildrenOfType<Grid>().ElementAt(1);
                grid.Children.Add(button);
            }
        }

        protected override void Clear(RadTreeMapItem treemapItem, object dataItem)
        {
            treemapItem.ClearValue(RadTreeMapItem.BackgroundProperty);
        }
    }
}
