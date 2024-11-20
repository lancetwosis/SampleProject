using LibRedminePower;
using LibRedminePower.ViewModels;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using LibRedminePower.Applications;
using RedmineTimePuncher.Models.Managers;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using LibRedminePower.Enums;
using RedmineTimePuncher.Extentions;
using System.Reactive.Disposables;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Bases;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Common.Bases;
using LibRedminePower.ViewModels.Bases;
using RedmineTimePuncher.Models.CreateTicket.Common;
using RedmineTimePuncher.Models.Settings.CreateTicket;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases
{
    public abstract class RequestViewModelBase<TTarget, TAssignee, TMethod, TRequests, TTargetModel, TRequestsModel, TPeriodModel> : ViewModelBase
        where TTarget : TargetTicketViewModelBase<TTargetModel>
        where TAssignee : AssigneesViewModelBase<TTargetModel>
        where TMethod : IRequestPeriod
        where TRequests : RequestTicketsViewModelBase<TAssignee, TMethod, TRequestsModel, TTargetModel, TPeriodModel>
        where TTargetModel : TargetTicketModelBase
        where TRequestsModel : RequestTicketsModelBase<TPeriodModel>
        where TPeriodModel : PeriodModelBase
    {
        public BusyTextNotifier IsBusy { get; set; }

        public TTarget Target { get; set; }
        public TRequests Requests { get; set; }

        public ReactiveCommand GoToTicketCommand { get; set; }

        public ReadOnlyReactivePropertySlim<string> CanCreate { get; set; }

        protected RequestViewModelBase(TTarget target, TRequests requests)
        {
            IsBusy = new BusyTextNotifier();

            Target = target.AddTo(disposables);
            Requests = requests.AddTo(disposables);

            GoToTicketCommand = new[] { IsBusy.Inverse(), Target.Ticket.Select(t => t != null) }.CombineLatestValuesAreAllTrue()
                .ToReactiveCommand()
                .WithSubscribe(() => Target.Ticket.Value.GoToTicket()).AddTo(disposables);
        }

        public virtual void Clear()
        {
            Target.Clear();
            Requests.Clear();
        }

        protected async Task<List<string>> transcribeAsync(MyCustomFieldPossibleValue process, params TranscribeSettingModel[] settings)
        {
            try
            {
                var results = new List<string>();
                foreach (var s in settings)
                {
                    results.Add(await s.TranscribeAsync(Target.Ticket.Value, process));
                }
                return results;
            }
            catch (ApplicationException e)
            {
                var r = MessageBoxHelper.ConfirmWarning(string.Format(Resources.ReviewErrMsgFailedTranscribeDescription, e.Message), MessageBoxHelper.ButtonType.OkCancel);
                return r.HasValue && r.Value ? new List<string>() : null;
            }
        }

        protected List<IdentifiableName> convertTrackers(params (string TicketType, MyTracker Tracker)[] trackers)
        {
            var results = new List<IdentifiableName>();
            var disableTrackers = new List<(string TrackerName, string TicketType)>();
            foreach (var t in trackers)
            {
                var idName = t.Tracker.GetIdNameOrDefault(Target.Ticket.Value);
                if (idName == null)
                    disableTrackers.Add((t.Tracker.Name, t.TicketType));

                results.Add(idName);
            }
            if (disableTrackers.IsEmpty())
                return results;

            var tns = string.Join(", ", disableTrackers.Select(pair => pair.TrackerName));
            var tts = string.Join(", ", disableTrackers.Select(pair => pair.TicketType));
            var msg = string.Format(Resources.ReviewMsgConfirmDisableTrackers, Target.Ticket.Value.Project.Name, tns, tts, Target.Ticket.Value.RawIssue.Tracker.Name);
            var r = MessageBoxHelper.ConfirmWarning(msg, MessageBoxHelper.ButtonType.OkCancel);
            return r.HasValue && r.Value ? results : null;
        }

        protected string joinIfNotNullOrWhiteSpace(params string[] paragraphes)
        {
            return string.Join($"{Environment.NewLine}{Environment.NewLine}", paragraphes.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        protected virtual List<IssueCustomField> createCustomFields(List<IssueCustomField> fields, AssigneeModel assignee = null)
        {
            if (assignee != null)
            {
                var requierd = assignee.GetIssueCustomField();
                if (requierd != null)
                    fields.Add(requierd);
            }

            // 空のリストを CustomFields に設定すると以下の箇所で例外が発生するため、空の場合は null を返す
            // Redmine.Net.Api.Extensions.CollectionExtensions の Dump メソッド
            return fields.IsNotEmpty() ? fields : null;
        }

        protected async Task<Exception> applyStatusUnderRequestAsync()
        {
            // ステータスだけ更新したいので最新のチケットを再取得してから実施する
            var currentTicket = RedmineManager.Default.Value.GetTicketsById(Target.Ticket.Value.Id.ToString());
            currentTicket.RawIssue.Status = Target.StatusUnderRequest.Value;
            Exception failedToUpdate = null;
            try
            {
                await Task.Run(() => RedmineManager.Default.Value.UpdateTicket(currentTicket.RawIssue));
            }
            catch (Exception e)
            {
                failedToUpdate = e;
            }
            return failedToUpdate;
        }
    }
}
