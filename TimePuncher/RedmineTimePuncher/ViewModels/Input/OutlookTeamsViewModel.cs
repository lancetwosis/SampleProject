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

namespace RedmineTimePuncher.ViewModels.Input
{
    public class OutlookTeamsViewModel : ResourceViewModelBase<OutlookTeamsResource>
    {
        public ReadOnlyReactivePropertySlim<ReactiveTimer> OutlookTimer { get; set; }
        public ReadOnlyReactivePropertySlim<ReactiveTimer> TeamsTimer { get; set; }

        public OutlookTeamsViewModel(InputViewModel parent) : base()
        {
            if (parent.Parent.Outlook.IsInstalled || parent.Parent.Teams.IsInstalled)
                Resource = new OutlookTeamsResource(parent.Parent.Outlook, parent.Parent.Teams).AddTo(disposables);

            // 日付変更、もしくは１分間隔でTeamsのステータスを読み取る
            var teamsStatusSlots = Observable.Merge(
                parent.CurrentDate.Where(a => a != DateTime.MinValue).Select(_ => ""),
                Observable.Interval(TimeSpan.FromMinutes(1)).Select(_ => ""))
                .Select(_ => parent.Parent.Teams.GetStatus(Resource, parent.StartTime.Value, parent.EndTime.Value)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            teamsStatusSlots.ObserveOnUIDispatcher().SubscribeWithErr(t =>
            {
                parent.SpecialSlots.RemoveAll(a => a is TeamsStatusSlot);
                if (t != null)
                    parent.SpecialSlots.AddRange(t);
            }).AddTo(disposables);

            // Outlookの読込コマンド
            if (parent.Parent.Outlook.IsInstalled)
            {
                Resource.Updater.SetUpdateCommand(parent.Parent.Redmine.Select(a => a != null), async (ct) =>
                {
                    await execUpdateAsync(parent, async () =>
                    {
                        Logger.Info("outlookResource.Reload.SetReloadCommand Start");

                        // 予定を追加
                        var schedules = await Task.Run(() => parent.Parent.Outlook.GetSchedules(Resource, parent.StartTime.Value, parent.EndTime.Value), ct);
                        parent.Appointments.RemoveAll(a => a.ApoType == AppointmentType.Schedule);
                        parent.Appointments.AddRange(schedules);

                        // メールを追加
                        var mails = await Task.Run(() => parent.Parent.Outlook.GetSendMails(Resource, parent.StartTime.Value, parent.EndTime.Value), ct);
                        parent.Appointments.RemoveAll(a => a.ApoType == AppointmentType.Mail);
                        parent.Appointments.AddRange(parent.Parent.Settings.Appointment.Outlook.Filter(mails));

                        Logger.Info("outlookResource.Reload.SetReloadCommand End");
                    });
                });
            }
            // Teamsの読込コマンド
            if (parent.Parent.Teams.IsInstalled)
            {
                Resource.Updater2.SetUpdateCommand(parent.Parent.Redmine.Select(a => a != null), async (ct) =>
                {
                    await execUpdateAsync(parent, async () =>
                    {
                        Logger.Info("outlookResource.Reload2.SetReloadCommand Start");

                        // Teams通話、会議を取得開始
                        var call = await parent.Parent.Teams.GetCallAsync(Resource, ct, parent.StartTime.Value, parent.EndTime.Value);
                        parent.Appointments.RemoveAll(a => a.ApoType == AppointmentType.TeamsCall || a.ApoType == AppointmentType.TeamsMeeting);
                        parent.Appointments.AddRange(parent.Parent.Settings.Appointment.Outlook.Filter(call));

                        Logger.Info("outlookResource.Reload2.SetReloadCommand End");
                    });
                });
            }

            // 設定の内容に応じたUpdaterを作成
            OutlookTimer = parent.Parent.Settings.ObserveProperty(a => a.Appointment.Outlook).Where(_ => parent.Parent.Outlook.IsInstalled)
                .Select(a => Resource.Updater.CreateAutoReloadTimer(a))
                .DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            TeamsTimer = parent.Parent.Settings.ObserveProperty(a => a.Appointment.Teams).Where(_ => parent.Parent.Teams.IsInstalled)
                .Select(a => Resource.Updater2.CreateAutoReloadTimer(a))
                .DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
