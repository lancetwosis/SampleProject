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

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class ParentIssueSettingsViewModel : IssueSettingsViewModelBase
    {
        public ReactivePropertySlim<bool> UseQuery { get; set; }
        public static List<Query> Queries { get; set; }
        public ReactivePropertySlim<Query> SelectedQuery { get; set; }
        public ReactiveCommand GoToQueryCommand { get; set; }
        public ReactivePropertySlim<string> ParentIssueId { get; set; }
        public ReactivePropertySlim<bool> Recoursive { get; set; }
        public ReactiveCommand GoToTicketCommand { get; set; }
        public ReadOnlyReactivePropertySlim<bool> HasProperties { get; set; }
        public ReadOnlyReactivePropertySlim<string> IsValid { get; set; }
        public ReactiveProperty<bool> IsEdited { get; set; }

        [Obsolete("Design Only", true)]
        public ParentIssueSettingsViewModel() {}

        public ParentIssueSettingsViewModel(ParentIssueSettingsModel model, RedmineManager redmine) : base(model, redmine)
        {
            UseQuery = model.ToReactivePropertySlimAsSynchronized(a => a.UseQuery).AddTo(disposables);

            Queries = redmine.Queries;
            if (model.Query != null)
                model.Query = Queries.FirstOrDefault(q => q.Id == model.Query.Id);

            SelectedQuery = model.ToReactivePropertySlimAsSynchronized(a => a.Query).AddTo(disposables);
            UseQuery.CombineLatest(SelectedQuery, (u, q) => (u, q)).Subscribe(async p =>
            {
                if (p.u && p.q != null)
                {
                    await Task.Run(async () => await redmine.UpdateByQueryAsync(p.q));
                }
            }).AddTo(disposables);

            GoToQueryCommand = SelectedQuery.Select(a => a != null).ToReactiveCommand().WithSubscribe(() =>
            {
                if (SelectedQuery.Value.ProjectId.HasValue)
                {
                    var proj = redmine.Projects.First(p => p.Id == SelectedQuery.Value.ProjectId.Value);
                    System.Diagnostics.Process.Start($"{redmine.UrlBase}projects/{proj.Identifier}/issues?query_id={SelectedQuery.Value.Id}");
                }
                else
                {
                    System.Diagnostics.Process.Start($"{redmine.UrlBase}issues?query_id={SelectedQuery.Value.Id}");
                }
            }).AddTo(disposables);

            ParentIssueId = model.ToReactivePropertySlimAsSynchronized(a => a.IssueId).AddTo(disposables);
            Recoursive = model.ToReactivePropertySlimAsSynchronized(a => a.Recoursive).AddTo(disposables);
            var parentIssue = ParentIssueId.StartWithDefault().Where(id => !string.IsNullOrEmpty(id))
                .Throttle(TimeSpan.FromMilliseconds(500)).ObserveOnUIDispatcher().Select(id =>
                {
                    var no = id.Trim().TrimStart('#');
                    if (Regex.IsMatch(no, "^[0-9]+$"))
                    {
                        try
                        {
                            return redmine.GetIssue(int.Parse(no));
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            UseQuery.CombineLatest(parentIssue, (u, i) => (u, i)).Subscribe(async p =>
            {
                if (!p.u && p.i != null)
                {
                    await Task.Run(async () => await redmine.UpdateByParentIssueAsync(p.i));
                }
            }).AddTo(disposables);

            GoToTicketCommand = parentIssue.Select(a => a != null).ToReactiveCommand().WithSubscribe(() =>
            {
                System.Diagnostics.Process.Start(redmine.GetIssueUrl(parentIssue.Value.Id));
            }).AddTo(disposables);

            CompositeDisposable myDisposables = null;
            this.ObserveProperty(a => a.VisibleProps).Where(v => v != null).Subscribe(v =>
            {
                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable().AddTo(disposables);

                HasProperties = v.ToItems.CollectionChangedAsObservable().StartWithDefault().Select(_ => v.ToItems.Any()).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);

                IsValid = UseQuery.CombineLatest(SelectedQuery, parentIssue, HasProperties, (_1, _2, _3, _4) => true).Select(_ =>
                {
                    if (UseQuery.Value && SelectedQuery.Value == null)
                        return Resources.ErrMsgSelectCustomQuery;
                    else if (!UseQuery.Value && parentIssue.Value == null)
                        return Resources.ErrMsgSelectIssueId;

                    return HasProperties.Value ? null : Resources.ErrMsgSelectFieldsToDisplay;
                }).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
            }).AddTo(disposables);

            // 変更を刈り取る。
            IsEdited = new[]
            {
                SelectedQuery.Skip(1).Select(_ => true),
                this.ObserveProperty(a => a.VisibleProps.IsEdited.Value).Where(a => a),
            }.Merge().Select(_ => true).ToReactiveProperty().AddTo(disposables);

            IsEdited.Where(a => !a).Subscribe(_ =>
            {
                if (VisibleProps != null)
                    VisibleProps.IsEdited.Value = false;
            });
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
    }
}
