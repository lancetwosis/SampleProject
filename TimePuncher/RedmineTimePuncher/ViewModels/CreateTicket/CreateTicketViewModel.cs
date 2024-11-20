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
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.ViewModels.CreateTicket.Review;
using RedmineTimePuncher.ViewModels.CreateTicket.Work;
using RedmineTimePuncher.Models.CreateTicket.Enums;
using System.ComponentModel;

namespace RedmineTimePuncher.ViewModels.CreateTicket
{
    public class CreateTicketViewModel : FunctionViewModelBase
    {
        public BusyTextNotifier IsBusy { get; set; }

        public ReactivePropertySlim<CreateTicketMode> CreateMode { get; set; }
        public ReviewsViewModel Review { get; set; }
        public WorkViewModel Work { get; set; }
        public ReadOnlyReactivePropertySlim<object> ActiveViewModel { get; set; }

        public AsyncCommandBase CreateTicketCommand { get; set; }

        private CreateTicketModel model { get; }

        public CreateTicketViewModel() : base(ApplicationMode.TicketCreater)
        {
            IsBusy = new BusyTextNotifier();

            var json = Properties.Settings.Default.CreateTicket;
            try
            {
                if (!string.IsNullOrEmpty(json))
                {
                    model = CloneExtentions.ToObject<CreateTicketModel>(json);
                }
                else
                {
                    model = new CreateTicketModel();
                }
            }
            catch
            {
                Logger.Warn("Failed to read the json of CreateTicketModel.");
                model = new CreateTicketModel();
            }

            CreateMode = model.ToReactivePropertySlimAsSynchronized(m => m.CreateMode).AddTo(disposables);

            Review = new ReviewsViewModel(model.Reviews).AddTo(disposables);
            Work = new WorkViewModel(model.Work).AddTo(disposables);
            ActiveViewModel = CreateMode.Select(m => m == CreateTicketMode.Review ? (object)Review : Work).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            ErrorMessage = IsSelected.CombineLatest(RedmineManager.Default, SettingsModel.Default.ObserveProperty(a => a.CreateTicket), (isSelected, r, s) => (isSelected, r, s)).Select(t =>
            {
                // RedmineManager のチェックは MainWindowViewModel で行っているのでスルーする
                if (!t.isSelected || t.r == null)
                    return null;

                if (!t.r.CanUseAdminApiKey())
                    return Resources.ReviewErrMsgNeedAdminAPIKey;
                else if (!t.s.IsValid())
                    return Resources.ReviewErrMsgSetUpReview;
                else
                    return null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            CreateTicketCommand = new AsyncCommandBase(
                Resources.RibbonCmdCreateReviewTicket, Resources.icons8_review,
                new[] {
                    IsBusy.Select(i => i ? "" : null),
                    CreateMode.CombineLatest(Review.CanCreate, Work.CanCreate, (m, r, w) => m == CreateTicketMode.Review ? r : w)
                }.CombineLatestFirstOrDefault(a => a != null),
                async () =>
                {
                    TraceHelper.TrackCommand(nameof(CreateTicketCommand));
                    using (IsBusy.ProcessStart(Resources.ProgressMsgCreatingIssues))
                    {
                        if (CreateMode.Value == CreateTicketMode.Review)
                            await Review.CreateTicketAsync();
                        else if (CreateMode.Value == CreateTicketMode.Work)
                            await Work.CreateTicketAsync();
                    }
                }).AddTo(disposables);
        }

        public override void OnWindowClosing(CancelEventArgs e)
        {
            if (!e.Cancel && Review.Reviews.Any(r => r.NowSelfReviewing.Value))
            {
                var r = MessageBoxHelper.ConfirmInformation(Resources.ReviewMsgSelftConfirmAppClosing, MessageBoxHelper.ButtonType.OkCancel);
                if (!r.HasValue || !r.Value)
                    e.Cancel = true;
            }
        }

        public override void OnWindowClosed()
        {
            Properties.Settings.Default.CreateTicket = model.ToJson();
            Properties.Settings.Default.Save();
        }
    }
}
