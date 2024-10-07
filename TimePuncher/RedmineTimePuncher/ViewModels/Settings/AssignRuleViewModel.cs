using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class AssignRuleViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ObservableCollection<Project> SelectedProjects { get; set; }
        public ObservableCollection<Tracker> SelectedTrackers { get; set; }
        public ReactivePropertySlim<string> Subject { get; set; }
        public ReactivePropertySlim<StringCompareType> StringCompare { get; set; }
        public ObservableCollection<IssueStatus> SelectedStatuss { get; set; }
        public ReactivePropertySlim<Enums.AssignToType> AssignTo { get; set; }

        public ReadOnlyReactivePropertySlim<string> Detail { get; set; }

        public AssignRuleModel Model { get; set; }

        /// <summary>
        /// RadGridView の「Click here to add new item」機能のために必要
        /// </summary>
        public AssignRuleViewModel()
        {
            Model = new AssignRuleModel();
            setBind();
        }

        public AssignRuleViewModel(AssignRuleModel model)
        {
            Model = model;
            setBind();
        }

        private void setBind()
        {
            var projects = CacheTempManager.Default.Projects.Value;
            var trackers = CacheTempManager.Default.Trackers.Value;
            var statuss = CacheTempManager.Default.Statuss.Value;


            Model.ProjectIds = Model.ProjectIds.Where(a => projects.Any(b => b.Id == a)).ToList();
            SelectedProjects = new ObservableCollection<Project>(Model.ProjectIds.Select(a => projects.SingleOrDefault(b => b.Id == a)).Where(a => a != null));
            SelectedProjects.ObserveAddChanged().SubscribeWithErr(a => Model.ProjectIds.Add(a.Id)).AddTo(disposables);
            SelectedProjects.ObserveRemoveChanged().SubscribeWithErr(a => Model.ProjectIds.Remove(a.Id)).AddTo(disposables);

            Model.TrackerIds = Model.TrackerIds.Where(a => trackers.Any(b => b.Id == a)).ToList();
            SelectedTrackers = new ObservableCollection<Tracker>(Model.TrackerIds.Select(a => trackers.SingleOrDefault(b => b.Id == a)).Where(a => a != null));
            SelectedTrackers.ObserveAddChanged().SubscribeWithErr(a => Model.TrackerIds.Add(a.Id)).AddTo(disposables);
            SelectedTrackers.ObserveRemoveChanged().SubscribeWithErr(a => Model.TrackerIds.Remove(a.Id)).AddTo(disposables);

            Subject = Model.ToReactivePropertySlimAsSynchronized(a => a.Subject).AddTo(disposables);
            StringCompare = Model.ToReactivePropertySlimAsSynchronized(a => a.StringCompare).AddTo(disposables);

            Model.StatusIds = Model.StatusIds.Where(a => statuss.Any(b => b.Id == a)).ToList();
            SelectedStatuss = new ObservableCollection<IssueStatus>(Model.StatusIds.Select(a => statuss.SingleOrDefault(b => b.Id == a)).Where(a => a!=null));
            SelectedStatuss.ObserveAddChanged().SubscribeWithErr(a => Model.StatusIds.Add(a.Id)).AddTo(disposables);
            SelectedStatuss.ObserveRemoveChanged().SubscribeWithErr(a => Model.StatusIds.Remove(a.Id)).AddTo(disposables);

            AssignTo = Model.ToReactivePropertySlimAsSynchronized(a => a.AssignTo).AddTo(disposables);
            Detail = new[] {
                SelectedProjects.CollectionChangedAsObservable().StartWithDefault().Select(_ =>
                    SelectedProjects.Any() ? $"{string.Join(" | ", SelectedProjects.Select(a => a.Name))}" : null),
                SelectedTrackers.CollectionChangedAsObservable().StartWithDefault().Select(_ =>
                    SelectedTrackers.Any() ? $"{string.Join(" | ", SelectedTrackers.Select(a => a.Name))}" : null),
                Subject.CombineLatest(StringCompare, (s,c) => !string.IsNullOrEmpty(s) ? $"{s} {c.GetDescription()}" : null),
                SelectedStatuss.CollectionChangedAsObservable().StartWithDefault().Select(_ =>
                    SelectedStatuss.Any() ? $"{string.Join(" | ", SelectedStatuss.Select(a => a.Name))}" : null),
                AssignTo.Select(a => a != Enums.AssignToType.Any ? a.GetDescription() : null),
            }.CombineLatest().Select(a => string.Join(", ",  a.Where(b => !string.IsNullOrEmpty(b)))).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
