using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Models.Visualize.Factors;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Visualize.Charts;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
            if (dataItem is GroupingItemViewModel g)
            {
                treemapItem.Background = GROUP_BRUSH_DIC[g.Depth];
                treemapItem.ToolTip = new TextBlock()
                {
                    Text = $"{g.ToolTip.Label} : Total {g.ToolTip.TotalHours.Value} h",
                };

                var contextMenu = new ContextMenu();
                contextMenu.Items.Add(createMenuItem(Resources.VisualizeTicketTreeCmdExpand, g.ExpandTicketCommand));
                contextMenu.Items.Add(createMenuItem(Resources.VisualizeTicketTreeCmdCollapse, g.CollapseTicketCommand));
                contextMenu.Items.Add(new System.Windows.Controls.Separator());
                contextMenu.Items.Add(createMenuItem(Resources.VisualizeTicketTreeCmdExcludeAggregate, g.RemoveTicketCommand));

                treemapItem.ContextMenu = contextMenu;

                treemapItem.SetBinding(RadTreeMapItem.IsSelectedProperty, new Binding("DataItem.IsSelected") { Mode = BindingMode.TwoWay });
                treemapItem.MouseRightButtonUp += treemapItem_MouseRightButtonUp;
            }
            else if (dataItem is TicketItemViewModel t)
            {
                treemapItem.Background = FactorTypes.Issue.GetColor(t.XLabel, t.Issue.Id);

                var text = new FrameworkElementFactory(typeof(TextBlock));
                text.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                text.SetValue(TextBlock.TextWrappingProperty, TextWrapping.WrapWithOverflow);
                text.SetValue(TextBlock.TextProperty, t.DisplayValue);

                var tmp = new DataTemplate();
                tmp.VisualTree = text;
                treemapItem.HeaderTemplate = tmp;

                var tooltip = new TextBlock();
                tooltip.Text = t.ToolTip.ShowTotal ?
                    $"{t.ToolTip.Label} : Total {t.ToolTip.TotalHours.Value} h" :
                    $"{t.ToolTip.Label} : {t.ToolTip.Hours} h";
                treemapItem.ToolTip = tooltip;

                var button = new RadButton();
                button.Command = t.GoToTicketCommand;
                button.CommandParameter = t.Issue.Id;
                button.HorizontalAlignment = HorizontalAlignment.Right;
                button.VerticalAlignment = VerticalAlignment.Bottom;
                button.Style = Application.Current.FindResource("GotoTicketButtonStyle") as Style;
                if (t.Children.Any())
                    button.Margin = new Thickness(0, 0, 1, 0);
                else
                    Grid.SetRow(button, 1);

                var grid = treemapItem.ChildrenOfType<Grid>().ElementAt(1);
                grid.Children.Add(button);

                var contextMenu = new ContextMenu();
                contextMenu.Items.Add(createMenuItem(Resources.VisualizeTicketTreeCmdGoToTicket, t.GoToTicketCommand, t.Issue.Id));
                contextMenu.Items.Add(new System.Windows.Controls.Separator());
                contextMenu.Items.Add(createMenuItem(Resources.VisualizeTicketTreeCmdExpand, t.ExpandTicketCommand));
                contextMenu.Items.Add(createMenuItem(Resources.VisualizeTicketTreeCmdCollapse, t.CollapseTicketCommand));
                contextMenu.Items.Add(new System.Windows.Controls.Separator());
                contextMenu.Items.Add(createMenuItem(Resources.VisualizeTicketTreeCmdExcludeAggregate, t.RemoveTicketCommand));

                treemapItem.ContextMenu = contextMenu;

                treemapItem.SetBinding(RadTreeMapItem.IsSelectedProperty, new Binding("DataItem.IsSelected") { Mode = BindingMode.TwoWay});
                treemapItem.MouseRightButtonUp += treemapItem_MouseRightButtonUp;
            }
        }

        private MenuItem createMenuItem(string header, ICommand command, object parameter = null)
        {
            return new MenuItem() { Header = header, Command = command, CommandParameter = parameter };
        }

        // 右クリックでも選択状態になるようにする
        private void treemapItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as RadTreeMapItem;

            if (!Keyboard.IsKeyDown(Key.LeftShift) &&
                !Keyboard.IsKeyDown(Key.RightShift) &&
                !Keyboard.IsKeyDown(Key.LeftCtrl) &&
                !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                var treeVM = item.ParentOfType<RadTreeMap>().DataContext as TreeMapViewModel;
                treeVM.SelectTickets();
            }

            var data = item.DataContext as TreeMapData;
            var vm = data.DataItem as TreeMapItemViewModelBase;
            vm.IsSelected = true;

            // e.Handled = true によってイベントが伝播しないため手動で開く
            item.ContextMenu.IsOpen = true;

            // イベントが親チケットに伝播し、連続して選択状態になるのを防ぐため Handled にする
            e.Handled = true;
        }

        protected override void Clear(RadTreeMapItem treemapItem, object dataItem)
        {
            treemapItem.ClearValue(RadTreeMapItem.BackgroundProperty);
            treemapItem.MouseRightButtonUp -= treemapItem_MouseRightButtonUp;
        }
    }
}
