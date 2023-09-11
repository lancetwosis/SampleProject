using LibRedminePower.Extentions;
using Reactive.Bindings;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.ViewModels.Visualize.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Visualize.TreeMapItems
{
    public abstract class TreeMapItemViewModelBase : PointViewModel
    {
        public ObservableCollection<TreeMapItemViewModelBase> Children { get; set; }

        public ReactiveCommand ExpandTicketCommand { get; set; }
        public ReactiveCommand CollapseTicketCommand { get; set; }
        public ReactiveCommand RemoveTicketCommand { get; set; }

        public double TotalHours { get; set; }

        public bool IsSelected { get; set; }

        public Issue Issue { get; set; }

        protected TreeMapItemViewModelBase(TreeMapViewModel tree)
        {
            Children = new ObservableCollection<TreeMapItemViewModelBase>();

            ExpandTicketCommand = tree.ExpandCommand;
            CollapseTicketCommand = tree.CollapseCommand;
            RemoveTicketCommand = tree.RemoveCommand;
        }

        public void SetTotalHours()
        {
            if (!Children.Any())
            {
                TotalHours = Hours;
                DisplayValue = string.Join(Environment.NewLine,
                   Issue.GetLongLabel(),
                    $"{TotalHours} h");
                ToolTip = new ToolTipViewModel(Issue.GetFullLabel(), Hours);
                return;
            }

            foreach (var c in Children)
            {
                c.SetTotalHours();
            }

            // 子チケットがなく工数が付いていないものは削除する
            foreach (var c in Children.Where(c => c.Hours == 0 && c.Children.Count == 0).ToList())
            {
                Children.Remove(c);
            }

            TotalHours = Children.Sum(c => c.TotalHours);
            if (Issue != null)
            {
                DisplayValue = Issue.GetLongLabel();
                ToolTip = new ToolTipViewModel(Issue.GetFullLabel(), Hours, TotalHours);
            }
            else
            {
                DisplayValue = XLabel;
                ToolTip = new ToolTipViewModel(XLabel, Hours, TotalHours);
            }
        }
    }
}
