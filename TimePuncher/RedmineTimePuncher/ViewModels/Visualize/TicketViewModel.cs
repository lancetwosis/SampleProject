using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
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

        public string Id => Model.RawIssue.Id.ToString();
        public string Label => Model.RawIssue.GetLabel();
        public string Url => MyIssue.GetUrl(Model.RawIssue.Id);
        public string Tooltip => Model.RawIssue.GetFullLabel();

        public ObservableCollection<PersonHourModel> TimeEntries { get; set; }
        public ObservableCollection<TicketViewModel> Children { get; set; }

        public ReadOnlyReactivePropertySlim<bool> HasTimeEntry { get; set; }

        public ReactivePropertySlim<bool> IsEnabled { get; set; }

        /// <summary>
        /// RadTreeListView の IsExpandedBinding とバインドするため RP ではなくそのまま保持する
        /// </summary>
        public bool IsExpanded { get; set; } = true;

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
                updateRelatedIsEnabled(i);
            });

            IsExpanded = Model.IsExpanded;
            this.ObserveProperty(a => a.IsExpanded).Subscribe(i => Model.IsExpanded = i).AddTo(disposables);

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

            if (IsExpanded)
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
            if (IsExpanded)
            {
                foreach (var c in Children)
                {
                    result.AddRange(c.GetVisibleTickets());
                }
            }

            return result;
        }

        private BusyNotifier nowUpdateRelatedIsEnabled = new BusyNotifier();
        private void updateRelatedIsEnabled(bool isEnabled)
        {
            if (nowUpdateRelatedIsEnabled.IsBusy)
                return;

            if (isEnabled)
            {
                // 親を再帰的に true にする
                var parent = Parent;
                while (parent != null)
                {
                    if (parent.IsEnabled.Value)
                        break;

                    parent.setIsEnable(true);
                    parent = parent.Parent;
                }
            }
            else
            {
                // すべての子供を再帰的に false にする
                foreach (var c in Children)
                {
                    setDisableRecursive(c);
                }
            }
        }

        private void setIsEnable(bool isEnabled)
        {
            using (nowUpdateRelatedIsEnabled.ProcessStart())
            {
                SetIsEnabled(isEnabled);
            }
        }

        private void setDisableRecursive(TicketViewModel parent)
        {
            foreach (var c in parent.Children)
            {
                if (!c.IsEnabled.Value)
                    continue;

                setDisableRecursive(c);
                c.setIsEnable(false);
            }
            parent.setIsEnable(false);
        }

        public void SetIsEnabled(bool isEnabled, bool recursive = false)
        {
            if (isEnabled)
            {
                IsEnabled.Value = isEnabled;
                if (recursive)
                    Children.ToList().ForEach(c => c.SetIsEnabled(isEnabled, true));
            }
            else
            {
                if (recursive)
                    Children.ToList().ForEach(c => c.SetIsEnabled(isEnabled, true));

                IsEnabled.Value = isEnabled;
            }
        }

        public void SetIsExpanded(bool isExpanded, bool recursive = false)
        {
            if (isExpanded)
            {
                IsExpanded = isExpanded;
                if (recursive)
                    Children.ToList().ForEach(c => c.SetIsExpanded(isExpanded, true));
            }
            else
            {
                if (recursive)
                    Children.ToList().ForEach(c => c.SetIsExpanded(isExpanded, true));

                IsExpanded = isExpanded;
            }
        }

        public override string ToString()
        {
            return $"{Model.RawIssue.GetFullLabel()} HasTimeEntry={HasTimeEntry.Value}";
        }
    }
}
