using LibRedminePower.Logging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.ViewModels.Input.Bases;
using RedmineTimePuncher.ViewModels.Input.Slots;
using RedmineTimePuncher.ViewModels.Input.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using LibRedminePower.Extentions;
using System.Windows;
using LibRedminePower.Helpers;
using RedmineTimePuncher.Models.Managers;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Models.Settings;

namespace RedmineTimePuncher.ViewModels.Input
{
    public class OutlookTeamsViewModel : ResourceViewModelBase<OutlookTeamsResource>
    {
        public ReadOnlyReactivePropertySlim<ReactiveTimer> OutlookTimer { get; set; }
        public ReadOnlyReactivePropertySlim<ReactiveTimer> TeamsTimer { get; set; }

        public OutlookTeamsViewModel(InputViewModel parent) : base()
        {
            if (OutlookManager.Default.Value.IsInstalled || TeamsManager.Default.Value.IsInstalled)
                Resource = new OutlookTeamsResource(OutlookManager.Default.Value, TeamsManager.Default.Value).AddTo(disposables);

            // Outlookの読込コマンド
            if (OutlookManager.Default.Value.IsInstalled)
            {
                Resource.Updater.SetUpdateCommand(RedmineManager.Default.Select(a => a != null), async (ct) =>
                {
                    await execUpdateAsync(parent, async () =>
                    {
                        Logger.Info("outlookResource.Reload.SetReloadCommand Start");

                        // 予定を追加
                        var schedules = await Task.Run(() => OutlookManager.Default.Value.GetSchedules(Resource, parent.StartTime.Value, parent.EndTime.Value), ct);
                        parent.Appointments.RemoveAll(a => a.ApoType == AppointmentType.Schedule);
                        parent.Appointments.AddRange(schedules);

                        // メールを追加
                        var mails = await Task.Run(() => OutlookManager.Default.Value.GetSendMails(Resource, parent.StartTime.Value, parent.EndTime.Value), ct);
                        parent.Appointments.RemoveAll(a => a.ApoType == AppointmentType.Mail);
                        parent.Appointments.AddRange(SettingsModel.Default.Appointment.Outlook.Filter(mails));

                        Logger.Info("outlookResource.Reload.SetReloadCommand End");
                    });
                });
            }
            // Teamsの読込コマンド
            if (TeamsManager.Default.Value.IsInstalled)
            {
                // 表示範囲の変更、もしくは１分間隔でTeamsのステータスを読み取る
                parent.DisplayStartTime.CombineLatest(parent.DisplayEndTime, Observable.Interval(TimeSpan.FromMinutes(1)).StartWithDefault(), (s, e, _) => (Start: s, End: e))
                    .Throttle(TimeSpan.FromMilliseconds(100)).ObserveOnUIDispatcher().SubscribeWithErr(p =>
                    {
                        var status = TeamsManager.Default.Value.GetStatus(Resource, p.Start, p.End);
                        parent.SpecialSlots.RemoveAll(a => a is TeamsStatusSlot);
                        if (status != null)
                            parent.SpecialSlots.AddRange(status);
                    }).AddTo(disposables);

                Resource.Updater2.SetUpdateCommand(RedmineManager.Default.Select(a => a != null), async (ct) =>
                {
                    await execUpdateAsync(parent, async () =>
                    {
                        Logger.Info("outlookResource.Reload2.SetReloadCommand Start");

                        // Teams通話、会議を取得開始
                        try
                        {
                            var call = await TeamsManager.Default.Value.GetCallAsync(Resource, ct, parent.StartTime.Value, parent.EndTime.Value);
                            parent.Appointments.RemoveAll(a => a.ApoType == AppointmentType.TeamsCall || a.ApoType == AppointmentType.TeamsMeeting);
                            parent.Appointments.AddRange(SettingsModel.Default.Appointment.Outlook.Filter(call));
                        }
                        catch(Exception ex) 
                        {
                            Logger.Error(ex.ToString());

                            // 通話履歴の読み取りを無効にする。
                            var temp = SettingsModel.Default.Appointment.Teams.Clone();
                            temp.IsEnabledCallHistory = false;
                            SettingsModel.Default.Appointment.Teams = temp;
                            SettingsModel.Default.Save();

                            // ユーザーにその旨、連絡する。
                            MessageBoxHelper.ConfirmWarning(Properties.Resources.msgErrLoadingCallHistoryOff);
                        }

                        Logger.Info("outlookResource.Reload2.SetReloadCommand End");
                    });
                });
            }

            // 設定の内容に応じたUpdaterを作成
            OutlookTimer = SettingsModel.Default.ObserveProperty(a => a.Appointment.Outlook).Where(_ => OutlookManager.Default.Value.IsInstalled)
                .Select(a => Resource.Updater.CreateAutoReloadTimer(a))
                .DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            TeamsTimer = SettingsModel.Default.ObserveProperty(a => a.Appointment.Teams).Where(_ => TeamsManager.Default.Value.IsInstalled)
                .Select(a => Resource.Updater2.CreateAutoReloadTimer(a))
                .DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
