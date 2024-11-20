using LibRedminePower.Extentions;
using LibRedminePower.Models;
using LibRedminePower.ViewModels.Bases;
using NetOffice.OutlookApi.Enums;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases
{
    public abstract class TargetTicketViewModelBase<TModel> : ViewModelBase where TModel : TargetTicketModelBase
    {
        public ReactivePropertySlim<MyIssue> Ticket { get; set; }

        public ReactivePropertySlim<string> TicketNo { get; set; }
        public ReadOnlyReactivePropertySlim<string> Title { get; set; }

        public ReadOnlyReactivePropertySlim<List<IssueStatus>> Statuss { get; set; }
        public ReactivePropertySlim<IssueStatus> StatusUnderRequest { get; set; }

        public ReadOnlyReactivePropertySlim<string> IsValid { get; set; }

        protected TargetTicketViewModelBase(TargetTicketModelBase model, string statusType)
        {
            Ticket = model.ToReactivePropertySlimAsSynchronized(m => m.Ticket).AddTo(disposables);

            TicketNo = new ReactivePropertySlim<string>(model.Ticket?.Id.ToString()).AddTo(disposables);
            TicketNo.CombineLatest(RedmineManager.Default, (no, r) => (no, r)).Where(p => p.no != null && p.r != null).SubscribeWithErr(p =>
            {
                var no = p.no.Trim().TrimStart('#');
                if (!Regex.IsMatch(no, "^[0-9]+$"))
                {
                    Ticket.Value = null;
                    return;
                }

                MyIssue ticket = null;
                try
                {
                    ticket = RedmineManager.Default.Value.GetTicketsById(no);
                }
                catch
                {
                    Ticket.Value = null;
                    throw new ApplicationException(Resources.ReviewErrMsgFailedToGetIssue);
                }

                if (CacheManager.Default.IsMyProject(ticket.Project.Id))
                {
                    Ticket.Value = ticket;
                }
                else
                {
                    Ticket.Value = null;
                    throw new ApplicationException(Resources.ReviewErrMsgNotAssignedProject);
                }
            }).AddTo(disposables);
            Title = Ticket.CombineLatest(TicketNo, (t, no) =>
            {
                if (string.IsNullOrEmpty(no))
                    return null;
                else
                    return t != null ? t.Subject : Resources.ReviewErrMsgFailedToGetIssue;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Statuss = CacheManager.Default.Updated.Select(_ => CacheManager.Default.Statuss).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            StatusUnderRequest = model.ToReactivePropertySlimAsSynchronized(m => m.StatusUnderRequest).AddTo(disposables);
            Ticket.Skip(1).Where(t => t != null).SubscribeWithErr(t =>
            {
                StatusUnderRequest.Value = Statuss.Value?.FirstOrDefault(s => s.Id == t.Status.Id);
            }).AddTo(disposables);

            IsValid = Ticket.CombineLatest(StatusUnderRequest,(t, s) =>
            {
                if (t == null)
                    return string.Format(Resources.ReviewErrMsgSelectXXX, Resources.ReviewTargetTicket);
                else if (t.IsClosed)
                    return Resources.ReviewErrMsgTargetClosed;

                if (s == null)
                    return string.Format(Resources.ReviewErrMsgSelectXXX, statusType);

                return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public virtual void Clear()
        {
            TicketNo.Value = "";
            StatusUnderRequest.Value = null;
        }
    }
}
