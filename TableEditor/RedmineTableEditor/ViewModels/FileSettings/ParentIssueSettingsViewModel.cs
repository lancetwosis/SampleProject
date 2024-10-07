using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models.FileSettings;
using System.Collections.ObjectModel;
using LibRedminePower.ViewModels;
using LibRedminePower.Extentions;
using RedmineTableEditor.Models;
using ObservableCollectionSync;
using RedmineTableEditor.Models.TicketFields.Standard;
using System.Reactive.Disposables;
using System.Text.RegularExpressions;
using RedmineTableEditor.Properties;
using LibRedminePower.Enums;
using RedmineTableEditor.ViewModels.FileSettings.Filters;
using System.Diagnostics;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class ParentIssueSettingsViewModel : IssueSettingsViewModelBase
    {
        public ReactivePropertySlim<bool> UseQuery { get; set; }
        public static List<Query> Queries { get; set; }
        public ReactivePropertySlim<Query> SelectedQuery { get; set; }

        public FiltersViewModel Filters { get; set; }

        public ReadOnlyReactivePropertySlim<string> IsValid { get; set; }
        public ReactiveProperty<bool> IsEdited { get; set; }

        [Obsolete("Design Only", true)]
        public ParentIssueSettingsViewModel() {}

        private ParentIssueSettingsModel model { get; set; }
        private RedmineManager redmine { get; set; }

        public ParentIssueSettingsViewModel(ParentIssueSettingsModel model, RedmineManager redmine) : base(model, redmine)
        {
            this.model = model;
            this.redmine = redmine;

            UseQuery = model.ToReactivePropertySlimAsSynchronized(a => a.UseQuery).AddTo(disposables);

            Queries = redmine.Cache.Queries;
            if (model.Query != null)
                model.Query = Queries.FirstOrDefault(q => q.Id == model.Query.Id);
            SelectedQuery = model.ToReactivePropertySlimAsSynchronized(a => a.Query).AddTo(disposables);

            Filters = new FiltersViewModel(model.Filters, redmine).AddTo(disposables);

            CompositeDisposable myDisposables = null;
            this.ObserveProperty(a => a.VisibleProps).Where(v => v != null).SubscribeWithErr(v =>
            {
                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable().AddTo(disposables);

                IsValid = UseQuery.CombineLatest(SelectedQuery, Filters.ErrorMessage, (_1, _2, _3) => true).Select(_ =>
                {
                    if (UseQuery.Value && SelectedQuery.Value == null)
                        return Resources.ErrMsgSelectCustomQuery;
                    if (!UseQuery.Value && Filters.ErrorMessage.Value != null)
                        return Filters.ErrorMessage.Value;

                    return null;
                }).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
            }).AddTo(disposables);

            // 変更を刈り取る。
            IsEdited = new[]
            {
                UseQuery.Skip(1),
                SelectedQuery.Skip(1).Select(_ => true),
                Filters.IsEdited.Where(a => a),
                this.ObserveProperty(a => a.VisibleProps.IsEdited.Value).Where(a => a),
            }.Merge().Select(_ => true).ToReactiveProperty().AddTo(disposables);

            IsEdited.Where(a => !a).SubscribeWithErr(_ =>
            {
                if (VisibleProps != null)
                    VisibleProps.IsEdited.Value = false;

                Filters.IsEdited.Value = false;
            }).AddTo(disposables);
        }

        protected override List<FieldViewModel> getDefaultFields(ObservableCollectionSync<FieldViewModel, FieldModel> selectedFields, List<FieldViewModel> allFields)
        {
            var defaults = new List<FieldViewModel>();
            if (selectedFields.Any())
                return defaults;

            var id = allFields.FirstOrDefault(f => f.Field == IssuePropertyType.Id);
            if (id != null)
                defaults.Add(id);
            var subject = allFields.FirstOrDefault(f => f.Field == IssuePropertyType.Subject);
            if (subject != null)
                defaults.Add(subject);

            return defaults;
        }

        public void ShowIssuesOnRedmine()
        {
            Process.Start(model.CreateIssuesUrl(redmine));
        }
    }
}
