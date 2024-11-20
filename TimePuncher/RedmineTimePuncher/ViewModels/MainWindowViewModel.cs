using LibRedminePower;
using LibRedminePower.Exceptions;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using LibRedminePower.ViewModels;
using Microsoft.WindowsAPICodePack.Taskbar;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Behaviors;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels.Input.Controls;
using RedmineTimePuncher.ViewModels.Input;
using RedmineTimePuncher.ViewModels.Input.Resources;
using RedmineTimePuncher.ViewModels.Input.Resources.Bases;
using RedmineTimePuncher.ViewModels.Settings;
using RedmineTimePuncher.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;
using Telerik.Windows.Controls.TreeMap;
using RedmineTimePuncher.ViewModels.WikiPage;
using RedmineTimePuncher.ViewModels.Bases;
using System.Text.RegularExpressions;
using RedmineTimePuncher.ViewModels.CreateTicket;
using Newtonsoft.Json;
using TelerikEx.PersistenceProvider;
using LibRedminePower.Applications;
using RedmineTimePuncher.ViewModels.TableEditor;
using RedmineTimePuncher.ViewModels.Visualize;
using System.Diagnostics;
using RedmineTimePuncher.Properties;

namespace RedmineTimePuncher.ViewModels
{
    public class MainWindowViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public BusyTextNotifier IsBusy { get; set; }
        public TextNotifier ErrorMessage { get; set; }
        public ReadOnlyReactivePropertySlim<string> Title { get; private set; }

        public ReactiveCommand RibbonMinimizeCommand { get; set; }
        public ReactiveCommand ShowVersionDialogCommand { get; set; }
        public ReactiveCommand ShowOnlienHelpCommand { get; set; }
        public ReactiveCommand ShowSettingDialogCommand { get; set; }

        public ReactiveCommand<RoutedEventArgs> WindowLoadedEventCommand { get; }
        public ReactiveCommand<CancelEventArgs> WindowClosingEventCommand { get; set; }
        public ReactiveCommand<EventArgs> WindowClosedEventCommand { get; set; }

        public ReactivePropertySlim<int> SelectedIndex { get; set; }
        public ReadOnlyReactivePropertySlim<ApplicationMode> Mode { get; set; }

        private string url;

        public InputViewModel Input { get; set; }
        public TableEditorViewModel TableEditor { get; set; }
        public CreateTicketViewModel CreateTicket { get; set; }
        public VisualizeViewModel Visualize { get; set; }
        public WikiPageViewModel WikiPage { get; set; }
        public ObservableCollection<FunctionViewModelBase> Functions { get; set; }

        private ReactivePropertySlim<RedmineManager> redmine;

        public MainWindowViewModel(string[] args)
        {
            SelectedIndex = new ReactivePropertySlim<int>(0).AddTo(disposables);
            Mode = SelectedIndex.Select(i => (ApplicationMode)i).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsBusy = new BusyTextNotifier();

            RibbonMinimizeCommand = new ReactiveCommand().WithSubscribe(() =>
            Properties.Settings.Default.RadRibbonViewIsMinimized = !Properties.Settings.Default.RadRibbonViewIsMinimized).AddTo(disposables);

            // 各種マネージャーを作成する。
            redmine = new ReactivePropertySlim<RedmineManager>().AddTo(disposables);
            RedmineManager.Default = redmine.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            ErrorMessage = new TextNotifier().AddTo(disposables);
            SettingsModel.Default.ObserveProperty(a => a.Redmine).SubscribeWithErr(async s =>
            {
                await tryConnectAsync(s);
            }).AddTo(disposables);

            Input = new InputViewModel().AddTo(disposables);
            TableEditor = new TableEditorViewModel().AddTo(disposables);
            CreateTicket = new CreateTicketViewModel().AddTo(disposables);
            Visualize = new VisualizeViewModel().AddTo(disposables);
            WikiPage = new WikiPageViewModel().AddTo(disposables);

            // 定義の順番が NavigationView での表示順と処理に影響するので注意すること
            Functions = new ObservableCollection<FunctionViewModelBase>() { Input, Visualize, TableEditor, CreateTicket, WikiPage };

            // TODO 例外が発生しないように仮の処理を行う。恒久対処が行われたら置き換えること
            Title = Functions.Select(f => f.Title).CombineLatest().CombineLatest(Mode, (ts, m) => m)
                .Select(m => Functions.First(f => f.Mode == m).Title.Value).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            //Title = Functions.Select(f => f.Title).CombineLatest().CombineLatest(Mode, (ts, m) => true)
            //    .Select(_ => Functions.First(f => f.IsSelected.Value).Title.Value).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // バージョンダイアログを開く
            ShowVersionDialogCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                TraceHelper.TrackCommand(nameof(ShowVersionDialogCommand));

                using (var vm = new VersionDialogViewModel())
                {
                    var dialog = new VersionDialog();
                    dialog.DataContext = vm;
                    dialog.Owner = App.Current.MainWindow;
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    dialog.ShowDialog();
                }
            }).AddTo(disposables);

            // オンラインヘルプを開く
            ShowOnlienHelpCommand = new ReactiveCommand().WithSubscribe(() => Process.Start(ApplicationInfo.AppBaseUrl)).AddTo(disposables);

            // 設定ダイアログを開く
            ShowSettingDialogCommand = IsBusy.Inverse().ToReactiveCommand().WithSubscribe(() =>
            {
                TraceHelper.TrackAtomicFeature($"{nameof(ShowSettingDialogCommand)}.Executed@{Mode.Value}");

                // 自動更新を止めておく
                Input.Timers.Where(a => a != null).Select(a => a.Value).Where(a => a != null).ToList().ForEach(a => a.Stop());
                if (Input.Redmine.QueryTimer != null && Input.Redmine.QueryTimer.Value != null)
                    Input.Redmine.QueryTimer.Value.Stop();

                var clone = SettingsModel.Default.Clone();
                using (var vm = new SettingsViewModel(this, clone))
                {
                    var dialog = new Views.Settings.SettingsDialog();
                    dialog.DataContext = vm;
                    dialog.Owner = App.Current.MainWindow;
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    dialog.ShowDialog();
                    if (dialog.DialogResult == true)
                    {
                        SettingsModel.Default = clone;
                        SettingsModel.Default.Save();

                        // 設定ダイアログを開く前に、タイマーを止めていたので、再開させる。
                        Input.Timers.Select(a => a.Value).Where(a => a != null && !a.IsEnabled).ToList().ForEach(a => { a.Reset(); a.Start(); });
                        if (Input.Redmine.QueryTimer != null && Input.Redmine.QueryTimer.Value != null && !Input.Redmine.QueryTimer.Value.IsEnabled)
                        {
                            Input.Redmine.QueryTimer.Value.Reset();
                            Input.Redmine.QueryTimer.Value.Start();
                        }
                    }
                }
            }).AddTo(disposables);

            WindowLoadedEventCommand = new ReactiveCommand<RoutedEventArgs>().WithSubscribe(async e =>
            {
                // スプラッシュウィンドウを非表示にする
                RadSplashScreenManager.Close();

                // 画面をアクティブにする
                (e.Source as Window).Activate();

                Functions.ToList().ForEach(f => f.OnWindowLoaded(e));

                if (args.Length == 2 && args[0] == TableEditorViewModel.OPTION_KEY)
                {
                    TableEditor.SetFirstSettings(args[1]);
                    SelectedIndex.Value = (int)ApplicationMode.TableEditor;
                }
                else
                {
                    SelectedIndex.Value = Properties.Settings.Default.LastSelectedIndex;
                }
                autoUpdateCheck();
            }).AddTo(disposables);

            WindowClosingEventCommand = new ReactiveCommand<CancelEventArgs>().WithSubscribe(e =>
            {
                Functions.ToList().ForEach(f => f.OnWindowClosing(e));
            }).AddTo(disposables);

            WindowClosedEventCommand = new ReactiveCommand<EventArgs>().WithSubscribe(_ =>
            {
                Logger.Info("WindowClosedEventCommand was started.");

                CacheManager.Default.SaveCache(redmine.Value);

                Functions.ToList().ForEach(f => f.OnWindowClosed());

                Properties.Settings.Default.LastSelectedIndex = SelectedIndex.Value;
                Properties.Settings.Default.Save();

                Logger.Info("WindowClosedEventCommand was finished.");
            }).AddTo(disposables);
        }

        private async Task tryConnectAsync(RedmineSettingsModel settings)
        {
            if (settings.IsValid.Value)
            {
                using (IsBusy.ProcessStart(Resources.ProgressMsgConnectingRedmine))
                {
                    try
                    {
                        var manager = new RedmineManager(settings);
                        await manager.CheckConnectAsync();
                        ErrorMessage.Value = null;

                        await Task.Run(() => CacheManager.Default.UpdateCacheIfNeeded(manager));

                        redmine.Value = manager;
                    }
                    catch (Exception ex)
                    {
                        redmine.Value = null;

                        if (ex is AggregateException ae && ae.InnerException is RedmineApiException rae)
                        {
                            ErrorMessage.Value = Resources.msgErrUnauthorizedRedmineSettings;
                            Logger.Error(rae, "Failed to create RedmineManager on MainWindowViewModel.");
                        }
                        else
                        {
                            ErrorMessage.Value = ex.Message;
                            Logger.Error(ex, "Failed to create RedmineManager on MainWindowViewModel.");
                        }
                    }
                }
            }
            else
            {
                redmine.Value = null;
                ErrorMessage.Value = Resources.msgErrUnsetRedmineSettings;
            }
        }

        private void autoUpdateCheck()
        {
            var url = "https://www.redmine-power.com/";
            var limit = new DateTime(2025, 7, 24);

#if DEBUG
            // #1537 参照
            var debug = limit.AddMonths(-2);
            if (DateTime.Today >= debug)
            {
                Process.Start(this.url);
            }
#else
#endif

            var final = limit.AddDays(10);
            if (DateTime.Today >= final)
            {
                Properties.Settings.Default.NeedsAutoUpdate = false;
                Properties.Settings.Default.Save();
                Process.Start(this.url);
            }
            else if (DateTime.Today >= limit)
            {
                var needsUpdate = Properties.Settings.Default.NeedsAutoUpdate;
                Properties.Settings.Default.NeedsAutoUpdate = !needsUpdate;
                Properties.Settings.Default.Save();

                if (!needsUpdate)
                {
                    Process.Start(this.url);
                }
            }
        }
    }
}