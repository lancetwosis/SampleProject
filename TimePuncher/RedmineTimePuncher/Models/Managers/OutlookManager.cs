using LibRedminePower;
using LibRedminePower.Extentions;
using LibRedminePower.Logging;
using NetOffice.OutlookApi;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Calendar;

namespace RedmineTimePuncher.Models.Managers
{
    public class OutlookManager : LibRedminePower.Models.Bases.ModelBase
    {
        public static ReadOnlyReactivePropertySlim<OutlookManager> Default { get; set; } =
            SettingsModel.Default.ObserveProperty(a => a.Appointment.Outlook.IsEnabled).Select(a => new OutlookManager(a))
            .DisposePreviousValue().ToReadOnlyReactivePropertySlim();

        public bool IsInstalled { get; private set; }

        private Application outlook;

        public OutlookManager(bool isEnabled)
        {
            if (isEnabled)
            {
                setupOutlookInstance("New");
            }
        }

        private void setupOutlookInstance(string action)
        {
            try
            {
                outlook = new Application().AddTo(disposables);
                IsInstalled = true;
            }
            catch (System.Exception e)
            {
                Logger.Warn(e, $"Failed to start Outlook when {action}.");
            }
        }

        public List<MyAppointment> GetSchedules(Resource resource, DateTime start, DateTime end)
        {
            if (!IsInstalled) return new List<MyAppointment>();

            var schedules = new List<MyAppointment>();

            execWithErrHandle(Resources.OutlookActionGetAppointments, () =>
            {
                updateOutlookIfNeeded("GetSchedules");

                // 対象フォルダの取得
                var olNs = outlook.GetNamespace("MAPI");
                var folder = olNs.GetDefaultFolder(NetOffice.OutlookApi.Enums.OlDefaultFolders.olFolderCalendar);

                // スケジュールの検索
                var items = getScheduleItems(folder, start, end);

                // 本当は、以下のコードで取得したかったが、OutOfMemoryが発生して取得できないことがあるため、Whileで回す。
                // foreach (NetOffice.OutlookApi.AppointmentItem item in items)
                var i = 1;
                while (true)
                {
                    var item = items[i] as NetOffice.OutlookApi.AppointmentItem;
                    if (item == null) break;
                    else i++;

                    if (start <= item.Start && item.End <= end)
                    {
                        schedules.Add(createMyAppointment(resource, Enums.AppointmentType.Schedule,
                            get(() => item.Subject), get(() => item.Body),
                            item.Start, item.End, getEmailAddress(item.Recipients).ToArray()));
                    }
                }
            });

            return schedules.OrderBy(x => x.Start).ToList();
        }

        /// <summary>
        /// 定時内時間や実績時間の計算用。RP の初期化などは行っていないので、他の用途では使用しないこと。
        /// </summary>
        public List<MyAppointment> GetAppointments(DateTime start, DateTime end)
        {
            if (!IsInstalled) return new List<MyAppointment>();

            var appointments = new List<MyAppointment>();

            execWithErrHandle(Resources.OutlookActionGetAppointmentsForReport, () =>
            {
                updateOutlookIfNeeded("GetAppointments");

                // 対象フォルダの取得
                var olNs = outlook.GetNamespace("MAPI");
                var folder = olNs.GetDefaultFolder(NetOffice.OutlookApi.Enums.OlDefaultFolders.olFolderCalendar);

                // スケジュールの検索
                var items = getScheduleItems(folder, start, end);
                var i = 1;
                while (true)
                {
                    var item = items[i] as NetOffice.OutlookApi.AppointmentItem;
                    if (item == null) break;
                    else i++;

                    appointments.Add(new MyAppointment(item.Start, item.End, get(() => item.Subject), get(() => item.Categories)));
                }
            });

            return appointments.OrderBy(x => x.Start).ToList();
        }

        private void execWithErrHandle(string actionName, System.Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (System.Exception e)
            {
                ErrorHandler.Instance.HandleError(new ApplicationException(string.Format(Resources.OutlookErrMsg, actionName), e));
            }
        }

        /// <summary>
        /// AppointmentItem のプロパティへのアクセスで例外が発生する現象が報告された場合、本メソッドを使って個別に例外処理をすること
        /// </summary>
        private T get<T>(Func<T> getter)
        {
            try
            {
                return getter.Invoke();
            }
            catch (System.Exception e)
            {
                Logger.Warn(e, $"Failed to get Property.");
                return default(T);
            }
        }

        public List<MyAppointment> GetSendMails(Resource resource, DateTime start, DateTime end)
        {
            var result = new List<MyAppointment>();
            if (!IsInstalled) return result;

            execWithErrHandle(Resources.OutlookActionGetMails, () =>
            {
                updateOutlookIfNeeded("GetSendMails");

                // 対象フォルダの取得
                var olNs = outlook.GetNamespace("MAPI");
                var folder = olNs.GetDefaultFolder(NetOffice.OutlookApi.Enums.OlDefaultFolders.olFolderSentMail);

                // 送信済みメールを検索
                var items = getSentMails(folder, start, end);

                foreach (var item in items)
                {
                    string subject, body;
                    DateTime myEnd;
                    if (item is MailItem mail)
                    {
                        subject = mail.Subject;
                        body = mail.Body;
                        myEnd = mail.SentOn;
                    }
                    else if (item is MeetingItem meeting)
                    {
                        subject = meeting.Subject;
                        body = meeting.Body;
                        myEnd = meeting.SentOn;
                    }
                    else
                        continue;

                    subject = subject.Replace("RE: ", "").Replace("FW: ", "");
                    var tickLength = (int)SettingsModel.Default.Schedule.TickLength;
                    result.Add(createMyAppointment(resource, Enums.AppointmentType.Mail, subject, body, myEnd.AddMinutes((-1 * tickLength)), myEnd, null));
                }
            });

            return result;
        }

        private void updateOutlookIfNeeded(string action)
        {
            // Outlookへアクセスができなければ、再取得する。
            try
            {
                Logger.Info($"outlook version={outlook.Version}");
            }
            catch (System.Exception)
            {
                Logger.Warn($"Failed to access Outlook when {action}.");
                setupOutlookInstance(action);
            }
        }

        private MyAppointment createMyAppointment(Resource resource, Enums.AppointmentType apoType, string subject, string body, DateTime start, DateTime end, string[] recipients)
        {
            // Refs等の設定が指定されていた場合
            if (!string.IsNullOrEmpty(SettingsModel.Default.Appointment.Outlook.RefsKeywords))
            {
                var ticketNo = RedmineManager.Default.Value.GetTicketNo(SettingsModel.Default.Appointment.Outlook.RefsKeywords, body);
                if (!string.IsNullOrEmpty(ticketNo))
                {
                    return new MyAppointment(resource, Enums.AppointmentType.Schedule, subject, body, start, end, ticketNo)
                    {
                        Attenders = recipients,
                    };
                }
            }

            // 前回の設定を反映する場合
            if (SettingsModel.Default.Appointment.Outlook.IsReflectLastInput)
            {
                var entry = RedmineManager.Default.Value.GetTimeEntry(subject);
                if (entry != null)
                {
                    var ticketNo = entry.Issue.Id.ToString();
                    var issue = RedmineManager.Default.Value.GetTicketIncludeJournal(ticketNo, out var _);
                    return new MyAppointment(resource, Enums.AppointmentType.Schedule, subject, body, start, end, ticketNo, issue, entry.Activity.Name)
                    {
                        Attenders = recipients,
                    };
                }
            }

            // もし、題名／本文にチケット番号のようなものがあった場合
            {
                var ticketNo = RedmineManager.Default.Value.GetTicketNo(new[] { subject }.Concat(body.SplitLines()).ToArray());
                if(!string.IsNullOrEmpty(ticketNo))
                {
                    return new MyAppointment(resource, Enums.AppointmentType.Schedule, subject, body, start, end, ticketNo)
                    {
                        Attenders = recipients,
                    };
                }
            }

            return new MyAppointment(resource, Enums.AppointmentType.Schedule, subject, body, start, end, null)
            {
                Attenders = recipients,
            };
        }

        private IEnumerable<string> getEmailAddress(Recipients recipients)
        {
            foreach (var recipient in recipients)
            {
                if (recipient.AddressEntry.AddressEntryUserType == NetOffice.OutlookApi.Enums.OlAddressEntryUserType.olExchangeUserAddressEntry ||
                    recipient.AddressEntry.AddressEntryUserType == NetOffice.OutlookApi.Enums.OlAddressEntryUserType.olExchangeRemoteUserAddressEntry)
                {
                    var exchUser = recipient.AddressEntry.GetExchangeUser();
                    if (exchUser != null)
                        yield return exchUser.PrimarySmtpAddress;
                }
                else if (recipient.AddressEntry.AddressEntryUserType == NetOffice.OutlookApi.Enums.OlAddressEntryUserType.olSmtpAddressEntry)
                {
                    yield return recipient.Address;
                }
            }
        }

        private _Items getSentMails(MAPIFolder folder, DateTime start, DateTime end)
        {
            var startDate = start.ToString("yyyy/MM/dd hh:mm");
            var endDate = end.ToString("yyyy/MM/dd hh:mm");
            var filter = $"[SentOn] >= '{startDate}' AND [SentOn] <= '{endDate}'";
            var list = folder.Items.Restrict(filter);
            return list;
        }

        private _Items getScheduleItems(MAPIFolder folder, DateTime start, DateTime end)
        {
            var startDate = start.ToString("yyyy/MM/dd hh:mm");
            var endDate = end.ToString("yyyy/MM/dd hh:mm");
            var filter = $"[Start] < '{endDate}' AND [End] >= '{startDate}'";

            var items = folder.Items;
            items.Sort("[Start]");
            items.IncludeRecurrences = true;

            return items.Restrict(filter);
        }
    }
}
