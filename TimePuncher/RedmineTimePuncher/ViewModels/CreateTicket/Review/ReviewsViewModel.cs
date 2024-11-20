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
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.ViewModels.CreateTicket.Common;
using RedmineTimePuncher.Models.CreateTicket.Common;
using LibRedminePower.ViewModels.Bases;
using RedmineTimePuncher.Models.CreateTicket.Enums;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.Templates;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review
{
    public class ReviewsViewModel : ViewModelBase
    {
        public ReactivePropertySlim<int> SelectedIndex { get; set; }
        public ObservableCollection<ReviewViewModel> Reviews { get; set; }

        public ReadOnlyReactivePropertySlim<ReviewViewModel> SelectedReview { get; set; }

        public TemplatesViewModel Templates { get; set; }

        public CommandBase AddReviewCommand { get; set; }
        public CommandBase RemoveReviewCommand { get; set; }
        public CommandBase AdjustScheduleCommand { get; set; }

        public CommandBase ApplyTemplateCommand { get; set; }
        public CommandBase SaveTemplateCommand { get; set; }
        public CommandBase SaveAsTemplateCommand { get; set; }
        public CommandBase ShowTemplatesCommand { get; set; }

        public CommandBase ImportTemplatesCommand { get; set; }
        public CommandBase ExportTemplatesCommand { get; set; }

        public ReadOnlyReactivePropertySlim<string> CanCreate { get; set; }

        public ReviewsViewModel(ReviewsModel model)
        {
            SelectedIndex = model.ToReactivePropertySlimAsSynchronized(m => m.SelectedIndex).AddTo(disposables);

            Reviews = new ObservableCollectionSync<ReviewViewModel, ReviewModel>(model.Reviews,
                m => m != null ? new ReviewViewModel(m) : null,
                vm => vm?.Model).AddTo(disposables);
            if (Reviews.IsEmpty())
                Reviews.Add(new ReviewViewModel(new ReviewModel()));
            Reviews.ObserveElementProperty(r => r.Status.Value).Subscribe(r =>
            {
                if (r.Value == ReviewStatus.Completed)
                {
                    if (Reviews.Count > 1)
                    {
                        Reviews.Remove(r.Instance);
                        SelectedIndex.Value = 0;
                    }
                    else
                    {
                        r.Instance.Clear();
                    }
                }
            }).AddTo(disposables);

            SelectedReview = SelectedIndex.CombineLatest(Reviews.CollectionChangedAsObservable().StartWithDefault(), (index, _) =>
            {
                if (0 <= index && index < Reviews.Count)
                    return Reviews[index];
                else
                    return Reviews.FirstOrDefault();
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            CanCreate = SelectedReview.SelectMany(r => r.CanCreate).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            AddReviewCommand = new CommandBase(Resources.RibbonCmdNew, Resources.icons8_add_review_48,
                new ReactivePropertySlim<string>().AddTo(disposables),
                () =>
                {
                    TraceHelper.TrackCommand(nameof(AddReviewCommand));
                    Reviews.Add(new ReviewViewModel(new ReviewModel()));
                    SelectedIndex.Value = Reviews.Count - 1;
                }).AddTo(disposables);

            RemoveReviewCommand = new CommandBase(Resources.RibbonCmdDelete, Resources.icons8_delete_review_48,
                new[] {
                    SelectedReview.SelectMany(r => r.NowSelfReviewing).Select(nowSelf => nowSelf ? "" : null),
                    Reviews.CollectionChangedAsObservable().StartWithDefault().Select(_ => Reviews.Count > 1 ? null : ""),
                }.CombineLatestFirstOrDefault(a => a != null),
                () =>
                {
                    TraceHelper.TrackCommand(nameof(RemoveReviewCommand));
                    Reviews.Remove(SelectedReview.Value);
                }).AddTo(disposables);

            AdjustScheduleCommand = new CommandBase(
                Resources.RibbonCmdAdjustSchedule, Resources.icons8_calendar_48_mod,
                CanCreate,
                () =>
                {
                    TraceHelper.TrackCommand(nameof(AdjustScheduleCommand));
                    SelectedReview.Value.AdjustSchedule();
                }).AddTo(disposables);

            Templates = new TemplatesViewModel(model.Templates).AddTo(disposables);

            var notSelfReviewing = SelectedReview.SelectMany(r => r.NowSelfReviewing)
                .Select(nowSelf => nowSelf ? "" : null).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            ApplyTemplateCommand = new CommandBase(Resources.RibbonCmdApply, Resources.apply_icon,
                new[] { Templates.HasTemplate, notSelfReviewing }.CombineLatestFirstOrDefault(a => a != null),
                Templates.Templates.CollectionChangedAsObservable().StartWithDefault().CombineLatest(
                Templates.Templates.ObserveElementProperty(t => t.Name.Value).StartWithDefault(), (_1, _2) =>
                {
                    return Templates.Templates
                        .Select(t => (ChildCommand) new TemplateChildCommand(t, SelectedReview))
                        .ToList();
                }).ToReadOnlyReactivePropertySlim().AddTo(disposables)).AddTo(disposables);

            var hasTicket = SelectedReview.SelectMany(r => r.Target.Ticket)
                .Select(t => t != null ? null : string.Format(Resources.ReviewErrMsgSelectXXX, Resources.ReviewTargetTicket))
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);
            SaveTemplateCommand = new CommandBase(Resources.RibbonCmdRegister, Resources.save,
                Resources.ReviewTemplateCmdMsgSave, hasTicket.CombineLatest(notSelfReviewing, (h, n) => h ?? n),
                () =>
                {
                    TraceHelper.TrackCommand(nameof(SaveTemplateCommand));
                    Templates.Save(SelectedReview.Value);
                }).AddTo(disposables);
            SaveAsTemplateCommand = new CommandBase(Resources.RibbonCmdRegisterAs, Resources.saveas_icon,
                Resources.ReviewTemplateCmdMsgSave, hasTicket.CombineLatest(notSelfReviewing, (h, n) => h ?? n),
                () =>
                {
                    TraceHelper.TrackCommand(nameof(SaveAsTemplateCommand));
                    Templates.SaveAs(SelectedReview.Value);
                }).AddTo(disposables);

            ShowTemplatesCommand = new CommandBase(Resources.RibbonCmdShowList, Resources.icons8_list_48,
                Resources.ReviewTemplateCmdMsgShowList, Templates.HasTemplate,
                () =>
                {
                    TraceHelper.TrackCommand(nameof(ShowTemplatesCommand));
                    Templates.ShowList();
                }).AddTo(disposables);

            ImportTemplatesCommand = new CommandBase(Resources.RibbonCmdImport, Resources.icons8_import_50,
                Resources.ReviewTemplateCmdMsgImport, new ReactivePropertySlim<string>(),
                () =>
                {
                    TraceHelper.TrackCommand(nameof(ImportTemplatesCommand));
                    Templates.Import();
                }).AddTo(disposables);
            ExportTemplatesCommand = new CommandBase(Resources.RibbonCmdExport, Resources.icons8_export_50,
                Resources.ReviewTemplateCmdMsgExport, Templates.HasTemplate,
                () =>
                {
                    TraceHelper.TrackCommand(nameof(ExportTemplatesCommand));
                    Templates.Export();
                }).AddTo(disposables);
        }

        public async Task CreateTicketAsync()
        {
            await SelectedReview.Value.CreateTicketAsync();
        }
    }
}
