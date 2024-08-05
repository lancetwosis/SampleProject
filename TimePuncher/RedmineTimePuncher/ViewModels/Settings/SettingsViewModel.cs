using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Views;
using RedmineTimePuncher.Views.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class SettingsViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReadOnlyReactivePropertySlim<ApplicationMode> Mode { get; }

        public ReactivePropertySlim<int> SelectedIndex { get; set; }
        public ReactiveCommand TryConnectCommand { get; set; }

        public RedmineSettingsViewModel Redmine { get; set; }
        public ScheduleSettingsViewModel Schedule { get; set; }
        public CalendarSettingsViewModel Calendar { get; set; }
        public CategorySettingsViewModel Category { get; set; }
        public AppointmentSettingsViewModel Appointment { get; set; }
        public QuerySettingsViewModel Query { get; set; }
        public UserSettingsViewModel User { get; set; }
        public OutputDataSettingsViewModel OutputData { get; set; }
        public CreateTicketSettingsViewModel CreateTicket { get; set; }
        public ReviewIssueListSettingViewModel ReviewIssueList { get; set; }
        public ReviewCopyCustomFieldsSettingViewModel ReviewCopyCustomFields { get; set; }
        public RequestWorkSettingsViewModel RequestWork { get; set; }
        public PersonHourReportSettingsViewModel PersonHourReport { get; set; }

        public ReactiveCommand<SettingsDialog> ImportAllCommand { get; set; }
        public ReactiveCommand<SettingsDialog> ExportAllCommand { get; set; }

        public ReactiveCommand<SettingsDialog> OkCommand { get; set; }
        public ReactiveCommand<SettingsDialog> CancelCommand { get; set; }

        private CompositeDisposable myDisposables;
        private const string defaultFileName = "TimePuncherSetting";

        private SettingsModel model { get; set; }

        public SettingsViewModel(MainWindowViewModel parent, SettingsModel model)
        {
            this.model = model;

            Mode = parent.Mode;

            // 接続確認が必要かどうかのフラグ。一度確認を行ったら false、設定が変更されたら true
            needsTryConnect = new ReactivePropertySlim<bool>(true).AddTo(disposables);
            model.Redmine.IsValid.SubscribeWithErr(isValid =>
            {
                // Redmineの設定が更新されたら接続確認を実施する
                needsTryConnect.Value = isValid;
            }).AddTo(disposables);
            SelectedIndex = new ReactivePropertySlim<int>().AddTo(disposables);
            SelectedIndex.Pairwise().SubscribeWithErr(async p =>
            {
                // 「全般」から他のタブに移動したら必要に応じて接続確認をする
                if (p.OldItem == 0 && p.NewItem > 0)
                    await tryConnectAsync(false, false);
            }).AddTo(disposables);
            TryConnectCommand = model.Redmine.IsValid.CombineLatest(needsTryConnect, (i, n) => i && n)
                .ToReactiveCommand().WithSubscribe(async () => await tryConnectAsync(true, true)).AddTo(disposables);

            ImportAllCommand = new ReactiveCommand<SettingsDialog>().WithSubscribe(async win =>
            {
                var dialog = new OpenFileDialog();
                dialog.FileName = defaultFileName;
                dialog.Filter =
                "Text Files|*.txt" +
                "|All Files|*.*";
                dialog.FilterIndex = 0;
                if (dialog.ShowDialog().Value == true)
                {
                    try
                    {
                        model.Import(dialog.FileName);
                        await SetupAsync(true);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(string.Format(Properties.Resources.errImport, ex.Message), ex);
                    }
                }
            }).AddTo(disposables);

            ExportAllCommand = new ReactiveCommand<SettingsDialog>().WithSubscribe(win =>
            {
                var dialog = new SaveFileDialog();
                dialog.FileName = defaultFileName;
                dialog.Filter =
                "Text Files|*.txt" +
                "|All Files|*.*";
                dialog.FilterIndex = 0;
                if (dialog.ShowDialog().Value == true)
                {
                    try
                    {
                        model.Export(dialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(string.Format(Properties.Resources.errExport, ex.Message), ex);
                    }
                }
            }).AddTo(disposables);

            OkCommand = new ReactiveCommand<SettingsDialog>().WithSubscribe(dialog => 
            {
                if (Redmine.Locale.NeedsRestart ||
                    Appointment.Outlook.Value.IsEnabled.NeedsRestart ||
                    Appointment.Teams.Value.IsEnabled.NeedsRestart)
                {
                    var r = MessageBoxHelper.ConfirmQuestion(Properties.Resources.SettingsGenMsgRestartToChange, MessageBoxHelper.ButtonType.OkCancel);
                    if (r.Value)
                    {
                        Category.ApplyOrders();
                        model.Save();

                        Logger.Info($"Exit to change the settings required restart.");
                        Environment.Exit(1);
                    }
                    else
                    {
                        return;
                    }
                }

                Category.ApplyOrders();

                dialog.DialogResult = true;
                dialog.Close();
            }).AddTo(disposables);

            CancelCommand = new ReactiveCommand<SettingsDialog>().WithSubscribe(dialog =>
            {
                dialog.DialogResult = false;
                dialog.Close();
            }).AddTo(disposables);
        }

        private ReactivePropertySlim<RedmineManager> redmineManager;
        private ReactivePropertySlim<string> message;
        public async Task SetupAsync(bool forceConnect = false)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            Redmine = new RedmineSettingsViewModel(model.Redmine).AddTo(myDisposables);
            this.redmineManager = new ReactivePropertySlim<RedmineManager>().AddTo(myDisposables);
            this.message = new ReactivePropertySlim<string>().AddTo(myDisposables);

            // インポートした場合、フラグが false になっている可能性があるので明示的に true にする
            needsTryConnect.Value = true;

            // 前回、接続に失敗していた場合、設定画面を開いた段階でも接続に失敗する可能性が高いため接続しない
            // ただし、インポートした場合は必ず接続する
            if (Properties.Settings.Default.LastAuthorized || forceConnect)
                await tryConnectAsync(false, true);

            Schedule = new ScheduleSettingsViewModel(model.Schedule).AddTo(myDisposables);
            Calendar = new CalendarSettingsViewModel(model.Calendar).AddTo(myDisposables);
            Category = new CategorySettingsViewModel(model.Category, redmineManager, message) { }.AddTo(myDisposables);
            Appointment = new AppointmentSettingsViewModel(model.Appointment, redmineManager, message).AddTo(myDisposables);
            Query = new QuerySettingsViewModel(model.Query, redmineManager, message).AddTo(myDisposables);
            User = new UserSettingsViewModel(model.User, redmineManager, message).AddTo(myDisposables);
            OutputData = new OutputDataSettingsViewModel(model.OutputData).AddTo(myDisposables);
            CreateTicket = new CreateTicketSettingsViewModel(model.CreateTicket, model.TranscribeSettings, redmineManager, message).AddTo(myDisposables);
            ReviewIssueList = new ReviewIssueListSettingViewModel(model.ReviewIssueList, redmineManager, message).AddTo(myDisposables);
            ReviewCopyCustomFields = new ReviewCopyCustomFieldsSettingViewModel(model.ReviewCopyCustomFields, redmineManager, message).AddTo(myDisposables);
            RequestWork = new RequestWorkSettingsViewModel(model.RequestWork, redmineManager, message).AddTo(myDisposables);
            PersonHourReport = new PersonHourReportSettingsViewModel(model.PersonHourReport).AddTo(myDisposables);
        }

        private ReactivePropertySlim<bool> needsTryConnect { get; set; }
        private BusyNotifier nowConnecting { get; set; } = new BusyNotifier();
        private async Task tryConnectAsync(bool showMsg, bool showErrorMsg)
        {
            if (nowConnecting.IsBusy)
                return;

            using (nowConnecting.ProcessStart())
            {
                if (model.Redmine.IsValid.Value)
                {
                    if (!needsTryConnect.Value)
                        return;

                    try
                    {
                        message.Value = Properties.Resources.SettingsMsgNowConnecting;
                        var r = new RedmineManager(model.Redmine);
                        await r.CheckConnectAsync();

                        // 設定画面では Redmine の現在の情報を使って選択肢などを表示する必要がある
                        // よって Projects などのデータを再取得し、それらを使って処理を行う
                        await CacheManager.Default.UpdateTemporaryCacheAsync(r);

                        redmineManager.Value = r;
                        message.Value = "";

                        // 接続に成功したら次回以降、確認を実施しない
                        needsTryConnect.Value = false;

                        if (showMsg)
                            MessageBoxHelper.ConfirmInformation(Properties.Resources.SettingsGenMsgSuccessConnect);
                    }
                    catch (Exception ex)
                    {
                        redmineManager.Value = null;
                        message.Value = ex.Message;
                        Logger.Error(ex, "Failed to create RedmineManager on SettingsViewModel.");
                        needsTryConnect.Value = true;

                        if (showErrorMsg)
                            MessageBoxHelper.ConfirmWarning(ex.Message);
                    }
                }
                else
                {
                    redmineManager.Value = null;
                    message.Value = Properties.Resources.SettingsMsgSetRedmineSettings;
                }
            }
        }
    }
}
