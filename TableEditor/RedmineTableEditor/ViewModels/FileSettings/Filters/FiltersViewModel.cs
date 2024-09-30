using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models;
using RedmineTableEditor.Models.FileSettings.Filters;
using RedmineTableEditor.Models.FileSettings.Filters.Custom;
using RedmineTableEditor.Properties;
using RedmineTableEditor.ViewModels.FileSettings.Filters.Bases;
using RedmineTableEditor.ViewModels.FileSettings.Filters.Custom;
using RedmineTableEditor.ViewModels.FileSettings.Filters.Standard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.ViewModels.FileSettings.Filters
{
    public class FiltersViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<bool> IsEdited { get; set; }
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public ExpandableTwinListBoxViewModel<IFilterViewModel> Filters { get; set; }

        public FiltersViewModel(FiltersModel model, Models.RedmineManager redmine)
        {
            var project = new ProjectFilterViewModel(model.Project, redmine).AddTo(disposables);
            var standardFilters = new List<IFilterViewModel>()
            {
                // 定義順が表示順になるので留意すること
                project,
                new StatusFilterViewModel(model.Status).AddTo(disposables),
                new TrackerFilterViewModel(model.Tracker).AddTo(disposables),
                new PriorityFilterViewModel(model.Priority).AddTo(disposables),
                new VersionFilterViewModel(model.Version).AddTo(disposables),
                new IssueCategoryFilterViewModel(model.Category).AddTo(disposables),
                new AuthorFilterViewModel(model.Author).AddTo(disposables),
                new AssigneeFilterViewModel(model.Assignee).AddTo(disposables),
                new SubjectFilterViewModel(model.Subject).AddTo(disposables),
                new DescriptionFilterViewModel(model.Description).AddTo(disposables),
                new CommentFilterViewModel(model.Comment).AddTo(disposables),
                new UpdaterFilterViewModel(model.Updater).AddTo(disposables),
                new LastUpdaterFilterViewModel(model.LastUpdater).AddTo(disposables),
                new CreatedOnFilterViewModel(model.CreatedOn).AddTo(disposables),
                new UpdatedOnFilterViewModel(model.UpdatedOn).AddTo(disposables),
                new StartDateFilterViewModel(model.StartDate).AddTo(disposables),
                new DueDateFilterViewModel(model.DueDate).AddTo(disposables),
                new ParentIssueFilterViewModel(model.ParentIssue).AddTo(disposables),
                new ChildIssueFilterViewModel(model.ChildIssue).AddTo(disposables),
            };

            IsEdited = new ReactivePropertySlim<bool>().AddTo(disposables);

            CompositeDisposable myDisposables = null;
            redmine.Cache.Updated.SubscribeWithErr(_ =>
            {
                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable();

                project.Setup(redmine);
                foreach (var f in standardFilters.Where(f => f != project))
                {
                    f.Setup(redmine, project.Projects);
                }

                // いずれかのプロジェクトで有効なカスタムフィールドを対象とする
                var customFields = redmine.Cache.CustomFields.Where(cf => cf.IsFilter && cf.CustomizedType == "issue")
                        .Where(cf => cf.Trackers.Any(t => redmine.Cache.Projects.Any(p => p.Trackers.Any(a => a.Id == t.Id))))
                        .ToList();
                model.SetCustomFieldFilters(customFields);

                var customFilters = new[]
                {
                    model.CfItemsFilters.Select(m => (IFilterViewModel) new CfItemsFilterViewModel(m).AddTo(myDisposables)),
                    model.CfUserFilters.Select(m => new CfUserFilterViewModel(m, redmine, project.Projects).AddTo(myDisposables)),
                    model.CfVersionFilters.Select(m => new CfVersionFilterViewModel(m, redmine, project.Projects).AddTo(myDisposables)),
                    model.CfDateFilters.Select(m => new CfDateFilterViewModel(m).AddTo(myDisposables)),
                    model.CfIntegerFilters.Select(m => new CfIntegerFilterViewModel(m).AddTo(myDisposables)),
                    model.CfFloatFilters.Select(m => new CfFloatFilterViewModel(m).AddTo(myDisposables)),
                    model.CfStringFilters.Select(m => new CfTextSearchFilterViewModel(m).AddTo(myDisposables)),
                    model.CfLinkFilters.Select(m => new CfTextSearchFilterViewModel(m).AddTo(myDisposables)),
                    model.CfLongTextFilters.Select(m => new CfTextSearchFilterViewModel(m).AddTo(myDisposables)),
                    model.CfBoolFilters.Select(m => new CfBoolFilterViewModel(m).AddTo(myDisposables)),
                    model.CfKeyValueFilters.Select(m => new CfItemsFilterViewModel(m).AddTo(myDisposables)),
                }.SelectMany(a => a).ToList();

                var allFilters = standardFilters.Concat(customFilters).ToList();

                var errMsgs = allFilters.Select(f => f.ErrorMessage).CombineLatest()
                    .Select(errs => errs.FirstOrDefault(e => e != null)).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
                var anyFilters = allFilters.Select(f => f.NeedsFilter).CombineLatestValuesAreAllFalse()
                    .Select(a => a ? Resources.FilterErrMsgSelectAnyFilter : null).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);
                ErrorMessage = errMsgs.CombineLatest(anyFilters, (e1, e2) => e1 ?? e2).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);

                allFilters.Select(f => f.IsEdited.Where(a => a)).Merge().SubscribeWithErr(__ =>
                {
                    IsEdited.Value = true;
                }).AddTo(myDisposables);
                IsEdited.SubscribeWithErr(isEdited =>
                {
                    if (!isEdited)
                        allFilters.ForEach(f => f.IsEdited.Value = false);
                }).AddTo(myDisposables);

                var selectedFilters = new ObservableCollection<IFilterViewModel>(allFilters.Where(f => f.IsEnabled.Value && f.IsSelected.Value));
                selectedFilters.CollectionChangedAsObservable().SubscribeWithErr(e =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            if (e.NewItems != null)
                                e.NewItems.OfType<IFilterViewModel>().ToList().ForEach(f => f.IsSelected.Value = true);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            if (e.OldItems != null)
                                e.OldItems.OfType<IFilterViewModel>().ToList().ForEach(f => f.IsSelected.Value = false);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                        case NotifyCollectionChangedAction.Move:
                        case NotifyCollectionChangedAction.Reset:
                            break;
                    }
                }).AddTo(myDisposables);

                Filters = new ExpandableTwinListBoxViewModel<IFilterViewModel>(allFilters, selectedFilters, (f) => f.IsEnabled.Value, true).AddTo(myDisposables);

                project.Projects.SubscribeWithErr(ps =>
                {
                    model.UpdateIsEnableds(ps != null ? ps.Select(a => a.Id).ToList() : null, redmine);

                    Filters.Refresh();
                }).AddTo(myDisposables);
            }).AddTo(disposables);
        }
    }
}
