using LibRedminePower.Extentions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Visualize;
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
        public double TotalHours { get; set; }

        public Issue Issue { get; set; }

        protected TreeMapItemViewModelBase()
        {
            Children = new ObservableCollection<TreeMapItemViewModelBase>();
        }

        public TreeMapItemViewModelBase(PersonHourModel model) : this()
        {
            Issue = model.RawIssue;
            Hours = model.TotalHours;
            XLabel = Issue.GetLabel();
        }

        public TreeMapItemViewModelBase(Issue issue) : this()
        {
            Issue = issue;
            Hours = 0;
            XLabel = Issue.GetLabel();
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
