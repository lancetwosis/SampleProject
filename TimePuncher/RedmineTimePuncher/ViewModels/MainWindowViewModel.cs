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

        public SettingsModel Settings { get; set; }
        public ReactivePropertySlim<RedmineManager> Redmine { get; set; }
        public OutlookManager Outlook { get; set; }
        public TeamsManager Teams { get; set; }

        public ReactivePropertySlim<int> SelectedIndex { get; set; }
        public ReadOnlyReactivePropertySlim<ApplicationMode> Mode { get; set; }

        private string url;

        public InputViewModel Input { get; set; }
        public TableEditorViewModel TableEditor { get; set; }
        public CreateTicketViewModel CreateTicket { get; set; }
        public VisualizeViewModel Visualize { get; set; }
        public WikiPageViewModel WikiPage { get; set; }
        public ObservableCollection<FunctionViewModelBase> Functions { get; set; }

        public MainWindowViewModel(string[] args)
        {
            SelectedIndex = new ReactivePropertySlim<int>(0).AddTo(disposables);
            Mode = SelectedIndex.Select(i => (ApplicationMode)i).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsBusy = new BusyTextNotifier();

            RibbonMinimizeCommand = new ReactiveCommand().WithSubscribe(() =>
            Properties.Settings.Default.RadRibbonViewIsMinimized = !Properties.Settings.Default.RadRibbonViewIsMinimized).AddTo(disposables);

            // 設定を作成する。
            Settings = SettingsModel.Read();

            // 各種マネージャーを作成する。
            Outlook = new OutlookManager(Settings.ObserveProperty(a => a.Appointment.Outlook).ToReadOnlyReactivePropertySlim().AddTo(disposables)).AddTo(disposables);
            Teams = new TeamsManager(Settings).AddTo(disposables);
            Redmine = new ReactivePropertySlim<RedmineManager>().AddTo(disposables);
            ErrorMessage = new TextNotifier().AddTo(disposables);
            Settings.ObserveProperty(a => a.Redmine).SubscribeWithErr(async s =>
            {
                if (s.IsValid.Value)
                {
                    using (IsBusy.ProcessStart(Properties.Resources.ProgressMsgConnectingRedmine))
                    {
                        try
                        {
                            var manager = new RedmineManager(s);
                            ErrorMessage.Value = null;
                            await manager.CheckConnectAsync();

                            // すぐに必要にならない情報なので、裏で取得しておく。
                            MyIssue.PrioritiesTask = Task.Run(() => manager.Priorities.Value);
                            MyIssue.StatussTask = Task.Run(() => manager.Statuss.Value);
                            MyIssue.TrakersTask = Task.Run(() => manager.Trackers.Value);

                            // ユーザ設定に自分自身が設定されていたら削除する
                            if (Settings.User.Items.Any(u => u.Id == manager.MyUser.Id))
                            {
                                var userSettings = Settings.User.Clone();
                                var self = userSettings.Items.First(u => u.Id == manager.MyUser.Id);
                                userSettings.Items.Remove(self);
                                Settings.User = userSettings;
                                Settings.Save();
                            }

                            Redmine.Value = manager;

                            await Input.ReloadIfNeededAsync();
                        }
                        catch (Exception ex)
                        {
                            Redmine.Value = null;
                            ErrorMessage.Value = ex.Message;
                            Logger.Error(ex, "Failed to create RedmineManager on MainWindowViewModel.");
                        }
                    }
                }
                else
                {
                    Redmine.Value = null;
                    ErrorMessage.Value = Properties.Resources.msgErrUnsetRedminSettings;
                }
            }).AddTo(disposables);
            Redmine.Where(a => a != null).SubscribeWithErr(r =>
            {
                MyAppointment.Redmine = r;
                OutlookManager.Redmine = r;
                MyScheduleViewDragDropBehavior.Redmine = r;
                MyGridViewDragDropBehavior.Redmine = r;
            });

            Input = new InputViewModel(this).AddTo(disposables);
            TableEditor = new TableEditorViewModel(this).AddTo(disposables);
            CreateTicket = new CreateTicketViewModel(this).AddTo(disposables);
            Visualize = new VisualizeViewModel(this).AddTo(disposables);
            WikiPage = new WikiPageViewModel(this).AddTo(disposables);

            // 定義の順番が NavigationView での表示順と処理に影響するので注意すること
            Functions = new ObservableCollection<FunctionViewModelBase>() { Input, Visualize, TableEditor, CreateTicket, WikiPage };

            Redmine.CombineLatest(Functions.Select(f => f.ErrorMessage).Where(e => e != null).CombineLatest(), (r, errs) => (r, errs)).SubscribeWithErr(p =>
            {
                if (p.r != null)
                {
                    ErrorMessage.Value = p.errs.FirstOrDefault(e => e != null);
                }
            });

            Title = Functions.Select(f => f.Title).CombineLatest().CombineLatest(Mode, (ts, m) => true)
                .Select(_ => Functions.First(f => f.IsSelected.Value).Title.Value).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // バージョンダイアログを開く
            ShowVersionDialogCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                TraceMonitor.AnalyticsMonitor.TrackAtomicFeature(nameof(ShowVersionDialogCommand) + ".Executed");

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
                TraceMonitor.AnalyticsMonitor.TrackAtomicFeature(nameof(ShowSettingDialogCommand) + ".Executed");

                // 自動更新を止めておく
                Input.Timers.Where(a => a != null).Select(a => a.Value).Where(a => a != null).ToList().ForEach(a => a.Stop());
                if (Input.Redmine.QueryTimer != null && Input.Redmine.QueryTimer.Value != null)
                    Input.Redmine.QueryTimer.Value.Stop();

                var clone = Settings.Clone();
                clone.Redmine.UserName = Settings.Redmine.UserName;
                clone.Redmine.Password = Settings.Redmine.Password;
                clone.Redmine.AdminApiKey = Settings.Redmine.AdminApiKey;
                clone.Redmine.ApiKey = Settings.Redmine.ApiKey;
                clone.Redmine.UserNameOfBasicAuth = Settings.Redmine.UserNameOfBasicAuth;
                clone.Redmine.PasswordOfBasicAuth = Settings.Redmine.PasswordOfBasicAuth;
                using (var vm = new SettingsViewModel(this, clone))
                {
                    var dialog = new Views.Settings.SettingsDialog();
                    dialog.DataContext = vm;
                    dialog.Owner = App.Current.MainWindow;
                    dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    dialog.ShowDialog();
                    if (dialog.DialogResult == true)
                    {
                        // [IgnoreDataMember] のプロパティがあるため Equals で同値判定を行う
                        if (!Settings.Redmine.Equals(clone.Redmine))
                            Settings.Redmine = clone.Redmine;
                        if (Settings.Schedule.ToJson() != clone.Schedule.ToJson())
                        {
                            Settings.Schedule = clone.Schedule;
                            Input.MinTimeRulerExtent.Value = Settings.Schedule.TickLength.GetDefaultMinTimeRulerExtent();
                            Input.ScalingSliderValue.Value = 100;
                        }
                        if (Settings.Calendar.ToJson() != clone.Calendar.ToJson())
                            Settings.Calendar = clone.Calendar;
                        if (Settings.Category.ToJson() != clone.Category.ToJson())
                            Settings.Category = clone.Category;
                        if (Settings.Appointment.ToJson() != clone.Appointment.ToJson())
                        {
                            Settings.Appointment = clone.Appointment;
                        }
                        else
                        {
                            // 設定が更新されていなかったら、自動更新を再開させる。
                            Input.Timers.Select(a => a.Value).Where(a => a != null).ToList().ForEach(a => a.Reset());
                            Input.Timers.Select(a => a.Value).Where(a => a != null).ToList().ForEach(a => a.Start());
                        }
                        if (Settings.Query.ToJson() != clone.Query.ToJson())
                        {
                            Settings.Query = clone.Query;
                        }
                        else
                        {
                            // 設定が更新されていなかったら、自動更新を再開させる。
                            if (Input.Redmine.QueryTimer != null && Input.Redmine.QueryTimer.Value != null)
                            {
                                Input.Redmine.QueryTimer.Value.Reset();
                                Input.Redmine.QueryTimer.Value.Start();
                            }
                        }
                        if (Settings.User.ToJson() != clone.User.ToJson())
                            Settings.User = clone.User;
                        if (Settings.OutputData.ToJson() != clone.OutputData.ToJson())
                            Settings.OutputData = clone.OutputData;
                        if (Settings.CreateTicket.ToJson() != clone.CreateTicket.ToJson())
                            Settings.CreateTicket = clone.CreateTicket;
                        if (Settings.ReviewIssueList.ToJson() != clone.ReviewIssueList.ToJson())
                            Settings.ReviewIssueList = clone.ReviewIssueList;
                        if (Settings.TranscribeSettings.ToJson() != clone.TranscribeSettings.ToJson())
                            Settings.TranscribeSettings = clone.TranscribeSettings;
                        if (Settings.RequestWork.ToJson() != clone.RequestWork.ToJson())
                            Settings.RequestWork = clone.RequestWork;
                        if (Settings.PersonHourReport.ToJson() != clone.PersonHourReport.ToJson())
                            Settings.PersonHourReport = clone.PersonHourReport;
                        Settings.Save();
                    }
                }
            }).AddTo(disposables);

            WindowLoadedEventCommand = new ReactiveCommand<RoutedEventArgs>().WithSubscribe(e =>
            {
                // スプラッシュウィンドウを非表示にする
                RadSplashScreenManager.Close();

                autoUpdateCheck();

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

            }).AddTo(disposables);

            WindowClosingEventCommand = new ReactiveCommand<CancelEventArgs>().WithSubscribe(e =>
            {
                Functions.ToList().ForEach(f => f.OnWindowClosing(e));
            }).AddTo(disposables);

            WindowClosedEventCommand = new ReactiveCommand<EventArgs>().WithSubscribe(_ =>
            {
                Logger.Info("WindowClosedEventCommand was started.");

                Functions.ToList().ForEach(f => f.OnWindowClosed());

                Properties.Settings.Default.LastSelectedIndex = SelectedIndex.Value;
                Properties.Settings.Default.Save();

                Logger.Info("WindowClosedEventCommand was finished.");
            }).AddTo(disposables);
        }

        private void autoUpdateCheck()
        {
            string url = "https://www.redmine-power.com/";
            if (DateTime.Today >= new DateTime(2024, 8, 1))
            {
                System.Diagnostics.Process.Start(this.url);
            }
        }
    }
}