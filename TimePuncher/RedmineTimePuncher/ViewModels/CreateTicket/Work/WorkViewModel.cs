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
using NetOffice.OutlookApi.Enums;
using NetOffice.OutlookApi;
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
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.CreateTicket;
using ObservableCollectionSync;
using RedmineTimePuncher.Models.CreateTicket.Work;
using RedmineTimePuncher.ViewModels.CreateTicket.Common;
using RedmineTimePuncher.Models.CreateTicket.Common;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Work
{
    public class WorkViewModel : RequestViewModelBase<TargetTicketViewModel, AssigneesViewModel, PeriodViewModel, RequestTicketsViewModel,
                                                      TargetTicketModel, RequestTicketsModel, PeriodModel>
    {
        public WorkViewModel(WorkModel model)
            : base(new TargetTicketViewModel(model.Target), new RequestTicketsViewModel(model.Requests, model.Target))
        {
            CanCreate = new[] {
                IsBusy.Select(i => i ? "" : null),
                Target.IsValid,
                Requests.Period.IsValid,
                Requests.Assignee.IsValid,
            }.CombineLatestFirstOrDefault(a => a != null).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public async Task CreateTicketAsync()
        {
            using (IsBusy.ProcessStart(Resources.ProgressMsgCreatingIssues))
            {
                // 説明の転記機能のチェック
                var transPrgs = await transcribeAsync(null, SettingsModel.Default.RequestWork.RequestTranscribe);
                if (transPrgs == null)
                    return;
                var requestTransPrg = transPrgs.IsNotEmpty() ? transPrgs[0] : "";

                // 設定のトラッカーが選択中のチケットのプロジェクトで有効かどうかのチェック
                var idNames = convertTrackers((Resources.SettingsReviRequestTicket, SettingsModel.Default.RequestWork.RequestTracker));
                if (idNames == null)
                    return;
                var requestTracker = idNames[0];

                // 依頼チケットの作成
                var c = Target.Ticket.Value.CreateChildTicket();
                c.Author = CacheManager.Default.MyUser.ToIdentifiableName();
                c.Tracker = requestTracker;
                c.StartDate = Requests.Period.StartDate.Value;
                c.DueDate = Requests.Period.DueDate.Value;

                foreach (var a in Requests.Assignee.SelectedAssignees)
                {
                    c.AssignedTo = a.Model.ToIdentifiableName();
                    c.Subject = $"{Resources.AppModeTicketCreaterRequestWork} : {Target.Ticket.Value.Subject} {a.Model.GetPostFix()}";
                    c.CustomFields = createCustomFields(SettingsModel.Default.ReviewCopyCustomFields.GetCopiedCustomFields(Target.Ticket.Value), a.Model);
                    c.Description = joinIfNotNullOrWhiteSpace(
                        string.IsNullOrEmpty(Requests.Description.Value) ?
                            string.Format(Resources.ReviewMsgRequestFollowings, CacheManager.Default.MarkupLang.CreateTicketLink(Target.Ticket.Value)) :
                            Requests.Description.Value,
                        requestTransPrg);
                    await Task.Run(() => RedmineManager.Default.Value.CreateTicket(c));
                }

                // 作業対象チケットのステータスの更新
                var failedToUpdate = await applyStatusUnderRequestAsync();

                // 作業対象のチケットを開く
                Target.Ticket.Value.GoToTicket();

                // 各設定を初期化する
                Clear();

                if (failedToUpdate != null)
                    throw failedToUpdate;
            }
        }
    }
}
