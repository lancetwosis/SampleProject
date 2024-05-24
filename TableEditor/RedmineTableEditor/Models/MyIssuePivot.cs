using LibRedminePower.Attributes;
using Reactive.Bindings.Extensions;
using LibRedminePower.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models
{
    public class MyIssuePivot : LibRedminePower.Models.Bases.ModelBase
    {
        [Display(Name = "子チケット列名")]
        public string Title { get; }
        [LocalizedDisplayName(nameof(Resources.enumIssuePropertyTypeStatus), typeof(Resources))]
        public string Status => target.Status.DisplayValue;
        [LocalizedDisplayName(nameof(Resources.enumIssuePropertyTypeAssignedTo), typeof(Resources))]
        public string AssignedTo => target.AssignedTo.DisplayValue;
        [LocalizedDisplayName(nameof(Resources.enumIssuePropertyTypeFixedVersion), typeof(Resources))]
        public string FixedVersion => target.FixedVersion.DisplayValue;
        [LocalizedDisplayName(nameof(Resources.enumIssuePropertyTypePriority), typeof(Resources))]
        public string Priority => target.Priority.DisplayValue;
        [LocalizedDisplayName(nameof(Resources.enumIssuePropertyTypeCategory), typeof(Resources))]
        public string Category => target.Category.DisplayValue;
        [LocalizedDisplayName(nameof(Resources.enumIssuePropertyTypeStartDate), typeof(Resources))]
        public DateTime? StartDate => target.StartDate.Value;
        [LocalizedDisplayName(nameof(Resources.enumIssuePropertyTypeDueDate), typeof(Resources))]
        public DateTime? DueDate => target.StartDate.Value;
        [LocalizedDisplayName(nameof(Resources.enumIssuePropertyTypeDoneRatio), typeof(Resources))]
        public double? DoneRatio => target.DoneRatio.Value;
        [LocalizedDisplayName(nameof(Resources.enumIssuePropertyTypeEstimatedHours), typeof(Resources))]
        public double? EstimatedHours => target.EstimatedHours.Value;
        [LocalizedDisplayName(nameof(Resources.enumIssuePropertyTypeSpentHours), typeof(Resources))]
        public double? MySpentHours => target.MySpentHours;
        [LocalizedDisplayName(nameof(Resources.enumIssuePropertyTypeDiffEstimatedSpent), typeof(Resources))]
        public double? DiffEstimatedSpent => target.DiffEstimatedSpent;

        private MySubIssue target;
        public MyIssuePivot(MySubIssue target)
        {
            this.target = target;
            Title = target.TargetSubTicket.Title;
            target.ObserveProperty(a => a.Status.Value).Subscribe(_ => RaisePropertyChanged(nameof(Status))).AddTo(disposables);
            target.ObserveProperty(a => a.AssignedTo.Value).Subscribe(_ => RaisePropertyChanged(nameof(AssignedTo))).AddTo(disposables);
            target.ObserveProperty(a => a.FixedVersion.Value).Subscribe(_ => RaisePropertyChanged(nameof(FixedVersion))).AddTo(disposables);
            target.ObserveProperty(a => a.Priority.Value).Subscribe(_ => RaisePropertyChanged(nameof(Priority))).AddTo(disposables);
            target.ObserveProperty(a => a.Category.Value).Subscribe(_ => RaisePropertyChanged(nameof(Category))).AddTo(disposables);
            target.ObserveProperty(a => a.StartDate.Value).Subscribe(_ => RaisePropertyChanged(nameof(StartDate))).AddTo(disposables);
            target.ObserveProperty(a => a.DueDate.Value).Subscribe(_ => RaisePropertyChanged(nameof(DueDate))).AddTo(disposables);
            target.ObserveProperty(a => a.DoneRatio.Value).Subscribe(_ => RaisePropertyChanged(nameof(DoneRatio))).AddTo(disposables);
            target.ObserveProperty(a => a.EstimatedHours.Value).Subscribe(_ => RaisePropertyChanged(nameof(EstimatedHours))).AddTo(disposables);
            target.ObserveProperty(a => a.MySpentHours).Subscribe(_ => RaisePropertyChanged(nameof(MySpentHours))).AddTo(disposables);
            target.ObserveProperty(a => a.DiffEstimatedSpent).Subscribe(_ => RaisePropertyChanged(nameof(DiffEstimatedSpent))).AddTo(disposables);
        }
    }
}
