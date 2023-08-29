using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Visualize;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Visualize
{
    public class TicketViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public TicketModel Model { get; set; }

        public string Label => Model.RawIssue.GetLabel();
        public string Url => MyIssue.GetUrl(Model.RawIssue.Id);
        public string Tooltip => Model.RawIssue.GetFullLabel();

        public ObservableCollection<PersonHourModel> TimeEntries { get; set; }
        public ObservableCollection<TicketViewModel> Children { get; set; }

        public ReadOnlyReactivePropertySlim<bool> HasTimeEntry { get; set; }

        public ReactivePropertySlim<bool> IsEnabled { get; set; }
        public ReactivePropertySlim<bool> IsExpanded { get; set; }
        public ReadOnlyReactivePropertySlim<bool> IsReaf { get; set; }

        public TicketViewModel Parent { get; set; }

        public TicketViewModel(TicketModel model)
        {
            Model = model;
            TimeEntries = new ObservableCollection<PersonHourModel>();
            Children = new ObservableCollection<TicketViewModel>();

            var timeEntriesChanged = TimeEntries.CollectionChangedAsObservable().StartWithDefault().CombineLatest(
                Children.CollectionChangedAsObservable().StartWithDefault(), (_1, _2) => true);

            HasTimeEntry = timeEntriesChanged.Select(_ =>
            {
                return TimeEntries.Any() || Children.Select(a => a.HasTimeEntry.Value).Any(a => a);
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsEnabled = Model.ToReactivePropertySlimAsSynchronized(a => a.IsEnabled).AddTo(disposables);
            IsEnabled.Subscribe(i =>
            {
                foreach (var c in Children)
                    c.IsEnabled.Value = i;
            });

            IsExpanded = Model.ToReactivePropertySlimAsSynchronized(a => a.IsExpanded).AddTo(disposables);

            IsReaf = IsEnabled.CombineLatest(Children.CollectionChangedAsObservable().StartWithDefault(), (_, __) => true).Select(_ =>
            {
                if (!IsEnabled.Value)
                    return false;

                if (Children.Count == 0 || !Children.Any(c => c.IsEnabled.Value))
                    return true;

                return false;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public List<PersonHourModel> GetAllTimeEntries()
        {
            if (!IsEnabled.Value)
                return new List<PersonHourModel>();

            if (IsExpanded.Value)
            {
                var entries = TimeEntries.ToList();
                foreach (var child in Children.Where(c => c.IsEnabled.Value))
                {
                    entries.AddRange(child.GetAllTimeEntries());
                }
                return entries;
            }
            else
            {
                var allEntries = getChildrenTimeEntries();
                return allEntries.GroupBy(e => (e.SpentOn, e.User, e.Category, e.OnTime))
                    .Select(p => new PersonHourModel(Model.RawIssue, p.ToArray())).ToList();
            }
        }

        private List<PersonHourModel> getChildrenTimeEntries()
        {
            var entries = TimeEntries.ToList();
            foreach (var child in Children.Where(c => c.IsEnabled.Value))
            {
                entries.AddRange(child.getChildrenTimeEntries());
            }
            return entries;
        }

        public List<TicketViewModel> GetVisibleTickets()
        {
            var result = new List<TicketViewModel>();
            if (!IsEnabled.Value)
                return result;

            result.Add(this);
            if (IsExpanded.Value)
            {
                foreach (var c in Children)
                {
                    result.AddRange(c.GetVisibleTickets());
                }
            }

            return result;
        }

        public void Collapse()
        {
            foreach (var c in Children)
            {
                c.Collapse();
            }

            IsExpanded.Value = false;
        }

        public override string ToString()
        {
            return $"{Model.RawIssue.GetFullLabel()} HasTimeEntry={HasTimeEntry.Value}";
        }
    }
}
