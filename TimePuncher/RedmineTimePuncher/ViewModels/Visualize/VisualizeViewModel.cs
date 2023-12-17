using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Bases;
using RedmineTimePuncher.ViewModels.Visualize.Charts;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using RedmineTimePuncher.ViewModels.Visualize.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace RedmineTimePuncher.ViewModels.Visualize
{
    public class VisualizeViewModel : FunctionViewModelBase
    {
        public BusyNotifier IsBusy { get; set; }

        public TicketFiltersViewModel Filters { get; set; }
        public ResultViewModel Result { get; set; }
        public ReadOnlyReactivePropertySlim<string> TitlePrefix { get; set; }

        public AsyncCommandBase GetTimeEntriesCommand { get; set; }

        public AsyncCommandBase UpdateTimeEntriesCommand { get; set; }
        public AsyncCommandBase OpenResultCommand { get; set; }
        public CommandBase SaveResultCommand { get; set; }
        public CommandBase SaveAsResultCommand { get; set; }

        public ReactiveCommand ExpandCommand { get; set; }
        public ReactiveCommand ExpandRecursiveCommand { get; set; }
        public ReactiveCommand ExpandAllCommand { get; set; }
        public ReactiveCommand CollapseCommand { get; set; }
        public ReactiveCommand CollapseRecursiveCommand { get; set; }
        public ReactiveCommand CollapseAllCommand { get; set; }
        public ReactiveCommand EnableCommand { get; set; }
        public ReactiveCommand EnableRecursiveCommand { get; set; }
        public ReactiveCommand DisableCommand { get; set; }
        public ReactiveCommand DisableRecursiveCommand { get; set; }
        public ReactiveCommand<RadTreeListView> ScrollToSelectedRowCommand { get; set; }
        public ReactiveCommand AddFilterCommand { get; set; }
        public MainWindowViewModel Parent { get; set; }

        public VisualizeViewModel(MainWindowViewModel parent)
            : base(ApplicationMode.Visualizer, parent)
        {
            this.Parent = parent;

            IsBusy = new BusyNotifier();

            Filters = new TicketFiltersViewModel(this).AddTo(disposables);

            Result = new ResultViewModel(this).AddTo(disposables);
            TitlePrefix = Result.ObserveProperty(a => a.Model.FileName).CombineLatest(Result.IsEdited, (n, i) => (name:n, isEdited:i)).Select(p =>
            {
                if (!Result.Model.HasValue)
                    return null;

                if (string.IsNullOrEmpty(p.name))
                    return $"{Resources.VisualizeNew}";

                if (p.isEdited)
                    return $"{p.name} {Resources.VisualizeUpdated}";
                else
                    return p.name;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsSelected.Where(a => a).Take(1).Subscribe(_ =>
            {
                using (IsBusy.ProcessStart())
                using (parent.IsBusy.ProcessStart(""))
                {
                    Result.Initialize();
                }
            }).AddTo(disposables);

            GetTimeEntriesCommand = new AsyncCommandBase(
               Resources.VisualizeCmdGetData, Resources.icons8_database_down_48,
               new[] { IsBusy.Select(i => i ? "" : null), Filters.IsValid }.CombineLatest().Select(a => a.FirstOrDefault(m => m != null)),
               async () =>
               {
                   using (IsBusy.ProcessStart())
                   using (parent.IsBusy.ProcessStart(""))
                   {
                       await Result.GetTimeEntriesAsync(Filters);
                       Filters.IsExpanded.Value = false;
                   }
               }).AddTo(disposables);

            UpdateTimeEntriesCommand = new AsyncCommandBase(
               Resources.VisualizeCmdUpdateData, Resources.reload,
               new[] { IsBusy.Select(i => i ? "" : null), Result.ObserveProperty(a => a.Model.HasValue).Select(h => h ? null : "") }.CombineLatest().Select(a => a.FirstOrDefault(m => m != null)),
               async () =>
               {
                   using (IsBusy.ProcessStart())
                   using (parent.IsBusy.ProcessStart(""))
                   {
                       await Result.UpdateTimeEntriesAsync();
                       Filters.IsExpanded.Value = false;
                   }
               }).AddTo(disposables);

            OpenResultCommand = new AsyncCommandBase(
                Resources.VisualizeCmdOpen, Resources.open_icon,
                new[] {
                    IsBusy.Select(a => !a),
                }.CombineLatestValuesAreAllTrue().Select(a => a ? null : ""),
                async () =>
                {
                    using (IsBusy.ProcessStart())
                    using (parent.IsBusy.ProcessStart(""))
                    {
                        Result.Open();
                    }
                }).AddTo(disposables);

            var hasResult = new[] {
                    parent.IsBusy.Select(a => !a),
                    this.ObserveProperty(a => a.Result.Model.HasValue),
                }.CombineLatestValuesAreAllTrue();

            SaveResultCommand = new CommandBase(
                Resources.VisualizeCmdSave, Resources.save,
                new[] { hasResult, Result.IsEdited, }.CombineLatestValuesAreAllTrue().Select(a => a ? null : ""),
                () => Result.SaveToFile()).AddTo(disposables);

            SaveAsResultCommand = new CommandBase(
                Resources.VisualizeCmdSaveAs, Resources.saveas_icon,
                hasResult.Select(a => a ? null : ""),
                () => Result.SaveAsToFile()).AddTo(disposables);

            ExpandCommand = Result.SelectedTickets.AnyAsObservable(t => !t.IsExpanded && t.Children.Any()).ToReactiveCommand().WithSubscribe(() =>
            {
                Result.SelectedTickets.ToList().ForEach(t => t.SetIsExpanded(true));
            }).AddTo(disposables);
            ExpandRecursiveCommand = Result.SelectedTickets.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                Result.SelectedTickets.ToList().ForEach(t => t.SetIsExpanded(true, true));
            }).AddTo(disposables);
            ExpandAllCommand = new ReactiveCommand().WithSubscribe(() => Result.ExpandAll()).AddTo(disposables);

            CollapseCommand = Result.SelectedTickets.AnyAsObservable(t => t.IsExpanded && t.Children.Any()).ToReactiveCommand().WithSubscribe(() =>
            {
                Result.SelectedTickets.ToList().ForEach(t => t.SetIsExpanded(false));
            }).AddTo(disposables);
            CollapseRecursiveCommand = Result.SelectedTickets.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                Result.SelectedTickets.ToList().ForEach(t => t.SetIsExpanded(false, true));
            }).AddTo(disposables);
            CollapseAllCommand = new ReactiveCommand().WithSubscribe(() => Result.CollapseAll()).AddTo(disposables);

            EnableCommand = Result.SelectedTickets.AnyAsObservable(t => !t.IsEnabled.Value).ToReactiveCommand().WithSubscribe(() =>
            {
                Result.SelectedTickets.ToList().ForEach(t => t.SetIsEnabled(true));
            }).AddTo(disposables);
            EnableRecursiveCommand = Result.SelectedTickets.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                Result.SelectedTickets.ToList().ForEach(t => t.SetIsEnabled(true, true));
            }).AddTo(disposables);

            DisableCommand = Result.SelectedTickets.AnyAsObservable(t => t.IsEnabled.Value).ToReactiveCommand().WithSubscribe(() =>
            {
                Result.SelectedTickets.ToList().ForEach(t => t.SetIsEnabled(false));
            }).AddTo(disposables);
            DisableRecursiveCommand= Result.SelectedTickets.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                Result.SelectedTickets.ToList().ForEach(t => t.SetIsEnabled(false,true));
            }).AddTo(disposables);

            ScrollToSelectedRowCommand = Result.SelectedTickets.AnyAsObservable().ToReactiveCommand<RadTreeListView>().WithSubscribe(tree =>
            {
                var index = tree.Items.OfType<TicketViewModel>().ToList().IndexOf(Result.SelectedTickets[0]);
                tree.ScrollIntoViewAsync(tree.Items[index], tree.Columns[0], f => (f as GridViewRow).IsSelected = true);
            }).AddTo(disposables);

            AddFilterCommand = hasResult.ToReactiveCommand().WithSubscribe(() =>
            {
                Result.AddNewFilter();
            }).AddTo(disposables);

            Title = CacheManager.Default.MyUser.CombineLatest(TitlePrefix, (u, p) => 
                string.IsNullOrEmpty(p) ? getTitle(u) : $"{p}  {getTitle(u)}").ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public override void OnWindowClosed()
        {
            Filters.Save();
            Result.Save();
        }
    }
}
