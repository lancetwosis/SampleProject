using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
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
        public RequestWorkSettingsViewModel RequestWork { get; set; }
        public PersonHourReportSettingsViewModel PersonHourReport { get; set; }

        public ReactiveCommand<SettingsDialog> ImportAllCommand { get; set; }
        public ReactiveCommand<SettingsDialog> ExportAllCommand { get; set; }

        public ReactiveCommand<SettingsDialog> OkCommand { get; set; }
        public ReactiveCommand<SettingsDialog> CancelCommand { get; set; }

        private CompositeDisposable myDisposables;
        private const string defaultFileName = "TimePuncherSetting";

        public SettingsViewModel(MainWindowViewModel parent, SettingsModel model)
        {
            Mode = parent.Mode;

            setViewModel(model);

            ImportAllCommand = new ReactiveCommand<SettingsDialog>().WithSubscribe(win =>
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
                        setViewModel(model);
                    }
                    catch(Exception ex)
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

        private void setViewModel(SettingsModel model)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            Redmine = new RedmineSettingsViewModel(model.Redmine).AddTo(myDisposables);
            var redmineManager = new ReactivePropertySlim<RedmineManager>().AddTo(myDisposables);
            var message = new ReactivePropertySlim<string>().AddTo(myDisposables);
            model.Redmine.IsValid.SubscribeWithErr(async a => 
            {
                if (a)
                {
                    try
                    {
                        message.Value = Properties.Resources.SettingsMsgNowConnecting;
                        var r = new RedmineManager(model.Redmine);
                        await r.CheckConnectAsync();
                        redmineManager.Value = r;
                        message.Value = "";
                    }
                    catch (Exception ex)
                    {
                        redmineManager.Value = null;
                        message.Value = ex.Message;
                        Logger.Error(ex, "Failed to create RedmineManager on SettingsViewModel.");
                    }
                }
                else
                {
                    redmineManager.Value = null;
                    message.Value = Properties.Resources.SettingsMsgSetRedmineSettings;
                }
            }).AddTo(myDisposables);
            Schedule = new ScheduleSettingsViewModel(model.Schedule).AddTo(myDisposables);
            Calendar = new CalendarSettingsViewModel(model.Calendar).AddTo(myDisposables);
            Category = new CategorySettingsViewModel(model.Category, redmineManager, message) { }.AddTo(myDisposables);
            Appointment = new AppointmentSettingsViewModel(model.Appointment, redmineManager, message).AddTo(myDisposables);
            Query = new QuerySettingsViewModel(model.Query, redmineManager, message).AddTo(myDisposables);
            User = new UserSettingsViewModel(model.User, redmineManager, message).AddTo(myDisposables);
            OutputData = new OutputDataSettingsViewModel(model.OutputData).AddTo(myDisposables);
            CreateTicket = new CreateTicketSettingsViewModel(model.CreateTicket, model.TranscribeSettings, redmineManager, message).AddTo(myDisposables);
            ReviewIssueList = new ReviewIssueListSettingViewModel(model.ReviewIssueList, redmineManager, message).AddTo(myDisposables);
            RequestWork = new RequestWorkSettingsViewModel(model.RequestWork, redmineManager, message).AddTo(myDisposables);
            PersonHourReport = new PersonHourReportSettingsViewModel(model.PersonHourReport).AddTo(myDisposables);
        }
    }
}
