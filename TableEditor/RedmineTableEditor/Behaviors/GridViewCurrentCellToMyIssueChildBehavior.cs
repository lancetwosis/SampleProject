using RedmineTableEditor.Models;
using RedmineTableEditor.Models.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace RedmineTableEditor.Behaviors
{
    public class GridViewCurrentCellToMyIssueChildBehavior : LibRedminePower.Behaviors.Bases.BehaviorBase<RadGridView>
    {
        public MyIssueBase Item
        {
            get { return (MyIssueBase)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(MyIssueBase), typeof(GridViewCurrentCellToMyIssueChildBehavior), new PropertyMetadata(null));

        public ObservableCollection<MyIssueBase> Items
        {
            get { return (ObservableCollection<MyIssueBase>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<MyIssueBase>), typeof(GridViewCurrentCellToMyIssueChildBehavior), new PropertyMetadata(null));

        protected override void OnSetup()
        {
            base.OnSetup();
            this.AssociatedObject.CurrentCellChanged += AssociatedObject_CurrentCellChanged;
            this.AssociatedObject.SelectedCellsChanged += AssociatedObject_SelectedCellsChanged;
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
            this.AssociatedObject.CurrentCellChanged -= AssociatedObject_CurrentCellChanged;
            this.AssociatedObject.SelectedCellsChanged -= AssociatedObject_SelectedCellsChanged;
        }

        private void AssociatedObject_SelectedCellsChanged(object sender, Telerik.Windows.Controls.GridView.GridViewSelectedCellsChangedEventArgs e)
        {
            var grid = sender as RadGridView;
            foreach(var item in e.AddedCells.Select(a => getMyIssueBase(getGridViewCell(grid, a))).Where(a => a != null))
            {
                if(!Items.Contains(item))
                    Items.Add(item);
            }
            foreach(var item in e.RemovedCells.Select(a => getMyIssueBase(getGridViewCell(grid, a))).Where(a => a != null))
            {
                if (Items.Contains(item))
                    Items.Remove(item);
            }
        }

        private void AssociatedObject_CurrentCellChanged(object sender, GridViewCurrentCellChangedEventArgs e)
        {
            if (e.NewCell != null)
                Item = getMyIssueBase(e.NewCell);
        }

        private GridViewCellBase getGridViewCell(RadGridView grid, GridViewCellInfo cellInfo)
        {
            var row = grid.ItemContainerGenerator.ContainerFromItem(cellInfo.Item) as GridViewRow;
            if (row != null)
            {
                foreach (var cell in row.Cells)
                {
                    if (cell.Column == cellInfo.Column)
                    {
                        return cell;
                    }
                }
            }
            return null;
        }

        private MyIssueBase getMyIssueBase(GridViewCellBase cell)
        {
            var model = cell?.DataContext as MyIssue;
            if (model != null)
            {
                if (cell.Column.Tag != null)
                {
                    if (int.TryParse(cell.Column.Tag.ToString(), out var number))
                        if (model.ChildrenDic.TryGetValue(number, out var result))
                            return result;
                }
                else
                    return model;
            }
            return null;
        }
    }
}
