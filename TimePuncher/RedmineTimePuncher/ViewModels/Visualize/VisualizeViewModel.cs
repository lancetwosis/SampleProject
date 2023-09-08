﻿using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Visualize;
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
                    return "(新規)";

                if (p.isEdited)
                    return $"{p.name} (更新)";
                else
                    return p.name;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsSelected.Where(a => a).Take(1).Subscribe(async _ =>
            {
                using (IsBusy.ProcessStart())
                using (parent.IsBusy.ProcessStart(""))
                {
                    await Task.Run(() =>
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Result.Initialize();
                        });
                    });
                }
            }).AddTo(disposables);

            GetTimeEntriesCommand = new AsyncCommandBase(
               "データ取得", Properties.Resources.icons8_database_down_48,
               new[] { IsBusy.Select(i => i ? "" : null), Filters.IsValid }.CombineLatest().Select(a => a.FirstOrDefault(m => m != null)),
               async () =>
               {
                   using (IsBusy.ProcessStart())
                   using (parent.IsBusy.ProcessStart(""))
                   {
                       await Task.Run(() =>
                       {
                           return App.Current.Dispatcher.Invoke(async () =>
                           {
                               await Result.GetTimeEntriesAsync(Filters);
                               Filters.IsExpanded.Value = false;
                           });
                       });
                   }
               }).AddTo(disposables);

            UpdateTimeEntriesCommand = new AsyncCommandBase(
               "再取得", Properties.Resources.reload,
               new[] { IsBusy.Select(i => i ? "" : null), Result.ObserveProperty(a => a.Model.HasValue).Select(h => h ? null : "") }.CombineLatest().Select(a => a.FirstOrDefault(m => m != null)),
               async () =>
               {
                   using (IsBusy.ProcessStart())
                   using (parent.IsBusy.ProcessStart(""))
                   {
                       await Task.Run(() =>
                       {
                           return App.Current.Dispatcher.Invoke(async () =>
                           {
                               await Result.UpdateTimeEntriesAsync();
                               Filters.IsExpanded.Value = false;
                           });
                       });
                   }
               }).AddTo(disposables);

            OpenResultCommand = new AsyncCommandBase(
                "開く", Properties.Resources.open_icon,
                new[] {
                    IsBusy.Select(a => !a),
                }.CombineLatestValuesAreAllTrue().Select(a => a ? null : ""),
                async () =>
                {
                    using (IsBusy.ProcessStart())
                    using (parent.IsBusy.ProcessStart(""))
                    {
                        await Task.Run(() =>
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Result.Open();
                            });
                        });
                    }
                }).AddTo(disposables);

            SaveResultCommand = new CommandBase(
                "保存", Properties.Resources.save,
                new[] {
                    parent.IsBusy.Select(a => !a),
                    this.ObserveProperty(a => a.Result.Model.HasValue),
                    Result.IsEdited,
                }.CombineLatestValuesAreAllTrue().Select(a => a ? null : ""),
                () => Result.SaveToFile()).AddTo(disposables);

            SaveAsResultCommand = new CommandBase(
                "名前を付けて保存", Properties.Resources.saveas_icon,
                new[] {
                    parent.IsBusy.Select(a => !a),
                    this.ObserveProperty(a => a.Result.Model.HasValue),
                }.CombineLatestValuesAreAllTrue().Select(a => a ? null : ""),
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
        }

        public override void OnWindowClosed()
        {
            Filters.Save();
            Result.Save();
        }
    }
}
