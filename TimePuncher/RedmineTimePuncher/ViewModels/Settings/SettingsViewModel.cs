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
using RedmineTimePuncher.ViewModels.Settings.CreateTicket;
using RedmineTimePuncher.Views;
using RedmineTimePuncher.Views.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
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
        public ReactivePropertySlim<int> SelectedIndex { get; set; }
        public AsyncReactiveCommand TryConnectCommand { get; set; }

        public RedmineSettingsViewModel Redmine { get; set; }
        public ReadOnlyReactivePropertySlim<ScheduleSettingsViewModel> Schedule { get; }
        public ReadOnlyReactivePropertySlim<CalendarSettingsViewModel> Calendar { get; }
        public ReadOnlyReactivePropertySlim<CategorySettingsViewModel> Category { get; }
        public ReadOnlyReactivePropertySlim<AppointmentSettingsViewModel> Appointment { get; }
        public ReadOnlyReactivePropertySlim<QuerySettingsViewModel> Query { get; }
        public ReadOnlyReactivePropertySlim<UserSettingsViewModel> User { get; }
        public ReadOnlyReactivePropertySlim<OutputDataSettingsViewModel> OutputData { get; }
        public ReadOnlyReactivePropertySlim<CreateTicketSettingsViewModel> CreateTicket { get; }
        public ReadOnlyReactivePropertySlim<ReviewIssueListSettingViewModel> ReviewIssueList { get; }
        public ReadOnlyReactivePropertySlim<ReviewCopyCustomFieldsSettingViewModel> ReviewCopyCustomFields { get; }
        public ReadOnlyReactivePropertySlim<TranscribeSettingsViewModel> TranscribeDescription { get; }
        public ReadOnlyReactivePropertySlim<RequestWorkSettingsViewModel> RequestWork { get; }
        public ReadOnlyReactivePropertySlim<PersonHourReportSettingsViewModel> PersonHourReport { get; }

        public ReactiveCommand<SettingsDialog> ImportAllCommand { get; set; }
        public ReactiveCommand<SettingsDialog> ExportAllCommand { get; set; }

        public ReactiveCommand<SettingsDialog> OkCommand { get; set; }
        public ReactiveCommand<SettingsDialog> CancelCommand { get; set; }

        private const string defaultFileName = "RedmineStudioSetting";

        public SettingsViewModel(ApplicationMode mode, SettingsModel model)
        {
            //------------------------
            // ModelからViewModelを生成する
            //------------------------
            // Redmine接続情報は、現在の設定は残しつつ上書きする仕様のため、Modelを丸ごと上書きしない運用のため、Reactive化はしない。
            Redmine = new RedmineSettingsViewModel(model.Redmine).AddTo(disposables);

            var isInput = mode == ApplicationMode.TimePuncher;
            Schedule = model.ToReadOnlyViewModel(a => a.Schedule, a => isInput ? new ScheduleSettingsViewModel(a) : null).AddTo(disposables);
            Calendar = model.ToReadOnlyViewModel(a => a.Calendar, a => isInput ? new CalendarSettingsViewModel(a) : null).AddTo(disposables);
            Category = model.ToReadOnlyViewModel(a => a.Category, a => isInput ? new CategorySettingsViewModel(a) : null).AddTo(disposables);
            Appointment = model.ToReadOnlyViewModel(a => a.Appointment, a => isInput ? new AppointmentSettingsViewModel(a) : null).AddTo(disposables);
            Query = model.ToReadOnlyViewModel(a => a.Query, a => isInput ? new QuerySettingsViewModel(a) : null).AddTo(disposables);
            User = model.ToReadOnlyViewModel(a => a.User, a => isInput ? new UserSettingsViewModel(a) : null).AddTo(disposables);
            OutputData = model.ToReadOnlyViewModel(a => a.OutputData, a => isInput ? new OutputDataSettingsViewModel(a) : null).AddTo(disposables);
            PersonHourReport = model.ToReadOnlyViewModel(a => a.PersonHourReport, a => isInput ? new PersonHourReportSettingsViewModel(a) : null).AddTo(disposables);

            var isReview = mode == ApplicationMode.TicketCreater;
            CreateTicket = model.ToReadOnlyViewModel(a => a.CreateTicket, a => isReview ? new CreateTicketSettingsViewModel(a) : null).AddTo(disposables);
            ReviewIssueList = model.ToReadOnlyViewModel(a => a.ReviewIssueList, a => isReview ? new ReviewIssueListSettingViewModel(a) : null).AddTo(disposables);
            ReviewCopyCustomFields = model.ToReadOnlyViewModel(a => a.ReviewCopyCustomFields, a => isReview ? new ReviewCopyCustomFieldsSettingViewModel(a) : null).AddTo(disposables);
            TranscribeDescription = model.ToReadOnlyViewModel(a => a.TranscribeSettings, a => isReview ? new TranscribeSettingsViewModel(a) : null).AddTo(disposables);
            RequestWork = model.ToReadOnlyViewModel(a => a.RequestWork, a => isReview ? new RequestWorkSettingsViewModel(a) : null).AddTo(disposables);

            model.ObserveProperty(a => a.CreateTicket).Subscribe(a => MessageBroker.Default.Publish(a)).AddTo(disposables);

            // ページ切り替えのタイミングでRedmineサーバーに接続を試みる。
            SelectedIndex = new ReactivePropertySlim<int>().AddTo(disposables);
            SelectedIndex.Pairwise().SubscribeWithErr(async p =>
            {
                // 「全般」から他のタブに移動したら必要に応じて接続確認をする
                if (p.OldItem == 0 && p.NewItem > 0)
                    await CacheTempManager.Default.TryConnectAsync(model.Redmine.Clone());
            }).AddTo(disposables);

            TryConnectCommand = model.Redmine.IsValid.ToAsyncReactiveCommand().WithSubscribe(async () =>
            {
                await CacheTempManager.Default.TryConnectAsync(model.Redmine.Clone());
                if (CacheTempManager.Default.Message.Value == null)
                    MessageBoxHelper.ConfirmInformation(Properties.Resources.SettingsGenMsgSuccessConnect);
                else
                    MessageBoxHelper.ConfirmWarning(CacheTempManager.Default.Message.Value);
            }).AddTo(disposables);

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
                    var clone = model.Clone();
                    clone.Redmine.SetNullValueForExport();
                    try
                    {
                        clone.Export(dialog.FileName);
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
                    (Appointment.Value != null && Appointment.Value.NeedsRestart()))
                {
                    var r = MessageBoxHelper.ConfirmQuestion(Properties.Resources.SettingsGenMsgRestartToChange, MessageBoxHelper.ButtonType.OkCancel);
                    if (r.Value)
                    {
                        Category.Value?.ApplyOrders();
                        model.Save();

                        Logger.Info($"Exit to change the settings required restart.");
                        Environment.Exit(0);
                    }
                    else
                    {
                        return;
                    }
                }

                Category.Value?.ApplyOrders();

                dialog.DialogResult = true;
                dialog.Close();
            }).AddTo(disposables);

            CancelCommand = new ReactiveCommand<SettingsDialog>().WithSubscribe(dialog =>
            {
                dialog.DialogResult = false;
                dialog.Close();
            }).AddTo(disposables);
        }
    }
}
