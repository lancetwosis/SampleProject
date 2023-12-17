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

namespace RedmineTimePuncher.ViewModels
{
    public class MainWindowViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public BusyTextNotifier IsBusy { get; set; }
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        public ReadOnlyReactivePropertySlim<string> Title { get; private set; }

        public ReactiveCommand RibbonMinimizeCommand { get; set; }
        public ReactiveCommand ShowVersionDialogCommand { get; set; }
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

        public TextNotifier MainErrorMessage { get; set; }

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
            MainErrorMessage = new TextNotifier().AddTo(disposables);
            Settings.ObserveProperty(a => a.Redmine).SubscribeWithErr(s =>
            {
                if (s.IsValid.Value)
                {
                    using (IsBusy.ProcessStart(Properties.Resources.ProgressMsgConnectingRedmine))
                    {
                        MainErrorMessage.Value = null;
                        try
                        {
                            var redmine = new RedmineManager(s);
                            CacheManager.Default.Redmine = redmine;

                            // キャッシュが未設定であった場合や、Redmine設定が更新された場合は、
                            if(!CacheManager.Default.IsExist() ||
                                CacheManager.Default.RedmineSetting == null ||
                               !CacheManager.Default.RedmineSetting.Equals(s))
                            {
                                // キャッシュを更新する。
                                AsyncHelper.RunSync(() => CacheManager.Default.UpdateAsync());
                            }
                            Redmine.Value = redmine;
                        }
                        catch (Exception ex)
                        {
                            Redmine.Value = null;
                            MainErrorMessage.Value = ex.Message;
                            Logger.Error(ex, "Failed to create RedmineManager on MainWindowViewModel.");
                        }
                    }
                }
                else
                {
                    Redmine.Value = null;
                    MainErrorMessage.Value = Properties.Resources.msgErrUnsetRedminSettings;
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

            var errors =
                new[] { (IObservable<string>)MainErrorMessage }.Concat(
                    Functions.Select(f => (IObservable<string>)(f.ErrorMessage)).Where(a => a != null));
            ErrorMessage = errors.CombineLatest(errs => errs.FirstOrDefault(e => e != null)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Title = Functions.Select(f => f.Title).CombineLatest().CombineLatest(Mode, (ts, m) => true)
                .Select(_ => Functions.First(f => f.IsSelected.Value).Title.Value).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // バージョンダイアログを開く
            ShowVersionDialogCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                TraceMonitor.AnalyticsMonitor.TrackAtomicFeature(nameof(ShowVersionDialogCommand) + ".Executed");

                using (var vm = new VersionDialogViewModel())
                {
                    var dialog = new Views.VersionDialog();
                    dialog.DataContext = vm;
                    dialog.Owner = App.Current.MainWindow;
                    dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    dialog.ShowDialog();
                }
            }).AddTo(disposables);

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

                if (args.Length == 2)
                {
                    if (args[0] == TableEditorViewModel.OPTION_KEY)
                    {
                        TableEditor.SetFirstSettings(args[1]);
                        SelectedIndex.Value = (int)ApplicationMode.TableEditor;
                    }
                }

            }).AddTo(disposables);

            WindowClosingEventCommand = new ReactiveCommand<CancelEventArgs>().WithSubscribe(e =>
            {
                Functions.ToList().ForEach(f => f.OnWindowClosing(e));
            }).AddTo(disposables);

            WindowClosedEventCommand = new ReactiveCommand<EventArgs>().WithSubscribe(async _ =>
            {
                Logger.Info("WindowClosedEventCommand was started.");

                // キャッシュデータの更新、保持
                if(Redmine.Value != null) 
                {
                    // キャッシュを更新する。
                    AsyncHelper.RunSync(() => CacheManager.Default.UpdateAsync());
                }
                CacheManager.Save();

                Functions.ToList().ForEach(f => f.OnWindowClosed());

                Logger.Info("WindowClosedEventCommand was finished.");
            }).AddTo(disposables);
        }

        private void autoUpdateCheck()
        {
            string url = "https://www.redmine-power.com/";
            if (DateTime.Today >= new DateTime(2024, 2, 1))
            {
                System.Diagnostics.Process.Start(this.url);
            }
        }
    }
}