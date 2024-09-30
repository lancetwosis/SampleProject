using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRedminePower.Enums;
using Telerik.Windows.Controls;
using System.Windows.Data;
using LibRedminePower.Extentions;
using System.Collections.Concurrent;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using System.Collections.ObjectModel;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.ViewModels;
using RedmineTableEditor.Models.Bases;
using System.Threading;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models.FileSettings.Filters.Bases;
using System.Text.Json.Serialization;
using RedmineTableEditor.Models.FileSettings.Filters.Standard;
using RedmineTableEditor.Models.FileSettings.Filters.Custom;

namespace RedmineTableEditor.Models.FileSettings.Filters
{
    public class FiltersModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public ProjectFilterModel Project { get; set; } = new ProjectFilterModel();
        public StatusFilterModel Status { get; set; } = new StatusFilterModel();
        public TrackerFilterModel Tracker { get; set; } = new TrackerFilterModel();
        public PriorityFilterModel Priority { get; set; } = new PriorityFilterModel();
        public VersionFilterModel Version { get; set; } = new VersionFilterModel();
        public IssueCategoryFilterModel Category { get; set; } = new IssueCategoryFilterModel();
        public AssigneeFilterModel Assignee { get; set; } = new AssigneeFilterModel();
        public AuthorFilterModel Author { get; set; } = new AuthorFilterModel();
        public UpdaterFilterModel Updater { get; set; } = new UpdaterFilterModel();
        public LastUpdaterFilterModel LastUpdater { get; set; } = new LastUpdaterFilterModel();
        public SubjectFilterModel Subject { get; set; } = new SubjectFilterModel();
        public DescriptionFilterModel Description { get; set; } = new DescriptionFilterModel();
        public CommentFilterModel Comment { get; set; } = new CommentFilterModel();
        public CreatedOnFilterModel CreatedOn { get; set; } = new CreatedOnFilterModel();
        public UpdatedOnFilterModel UpdatedOn { get; set; } = new UpdatedOnFilterModel();
        public StartDateFilterModel StartDate { get; set; } = new StartDateFilterModel();
        public DueDateFilterModel DueDate { get; set; } = new DueDateFilterModel();
        public ParentIssueFilterModel ParentIssue { get; set; } = new ParentIssueFilterModel();
        public ChildIssueFilterModel ChildIssue { get; set; } = new ChildIssueFilterModel();

        public List<CfItemsFilterModel> CfItemsFilters { get; set; } = new List<CfItemsFilterModel>();
        public List<CfItemsFilterModel> CfUserFilters { get; set; } = new List<CfItemsFilterModel>();
        public List<CfItemsFilterModel> CfVersionFilters { get; set; } = new List<CfItemsFilterModel>();
        public List<CfDateFilterModel> CfDateFilters { get; set; } = new List<CfDateFilterModel>();
        public List<CfIntegerFilterModel> CfIntegerFilters { get; set; } = new List<CfIntegerFilterModel>();
        public List<CfFloatFilterModel> CfFloatFilters { get; set; } = new List<CfFloatFilterModel>();
        public List<CfTextSearchFilterModel> CfStringFilters { get; set; } = new List<CfTextSearchFilterModel>();
        public List<CfTextSearchFilterModel> CfLinkFilters { get; set; } = new List<CfTextSearchFilterModel>();
        public List<CfTextSearchFilterModel> CfLongTextFilters { get; set; } = new List<CfTextSearchFilterModel>();
        public List<CfItemsFilterModel> CfBoolFilters { get; set; } = new List<CfItemsFilterModel>();
        public List<CfItemsFilterModel> CfKeyValueFilters { get; set; } = new List<CfItemsFilterModel>();

        private List<FilterModelBase> standardFilters
        {
            get
            {
                return new List<FilterModelBase>()
                {
                    Project,
                    Status,
                    Tracker,
                    Priority,
                    Version,
                    Category,
                    Author,
                    Assignee,
                    Subject,
                    Description,
                    Comment,
                    Updater,
                    LastUpdater,
                    CreatedOn,
                    UpdatedOn,
                    StartDate,
                    DueDate,
                    ParentIssue,
                    ChildIssue,
                };
            }
        }

        private List<FilterModelBase> customFilters
        {
            get
            {
                return new[]
                {
                    CfItemsFilters.OfType<FilterModelBase>(),
                    CfUserFilters,
                    CfVersionFilters,
                    CfDateFilters,
                    CfIntegerFilters,
                    CfFloatFilters,
                    CfStringFilters,
                    CfLinkFilters,
                    CfLongTextFilters,
                    CfBoolFilters,
                    CfKeyValueFilters
                }.SelectMany(a => a).ToList();
            }
        }

        public FiltersModel()
        {
        }

        public void SetCustomFieldFilters(List<CustomField> customFields)
        {
            foreach (var cfs in customFields.GroupBy(cf => cf.FieldFormat))
            {
                switch (cfs.Key)
                {
                    case RedmineKeys.CF_LIST:
                        CfItemsFilters = cfs.Select(cf =>
                        {
                            var filter = CfItemsFilters.FirstOrDefault(f => f.CustomField.Id == cf.Id);
                            return filter != null ? filter : new CfItemsFilterModel(cf);
                        }).ToList();
                        break;
                    case RedmineKeys.CF_USER:
                        CfUserFilters = cfs.Select(cf =>
                        {
                            var filter = CfUserFilters.FirstOrDefault(f => f.CustomField.Id == cf.Id);
                            return filter != null ? filter : new CfItemsFilterModel(cf);
                        }).ToList();
                        break;
                    case RedmineKeys.CF_VERSION:
                        CfVersionFilters = cfs.Select(cf =>
                        {
                            var filter = CfVersionFilters.FirstOrDefault(f => f.CustomField.Id == cf.Id);
                            return filter != null ? filter : new CfItemsFilterModel(cf);
                        }).ToList();
                        break;
                    case RedmineKeys.CF_DATE:
                        CfDateFilters = cfs.Select(cf =>
                        {
                            var filter = CfDateFilters.FirstOrDefault(f => f.CustomField.Id == cf.Id);
                            return filter != null ? filter : new CfDateFilterModel(cf);
                        }).ToList();
                        break;
                    case RedmineKeys.CF_INT:
                        CfIntegerFilters = cfs.Select(cf =>
                        {
                            var filter = CfIntegerFilters.FirstOrDefault(f => f.CustomField.Id == cf.Id);
                            return filter != null ? filter : new CfIntegerFilterModel(cf);
                        }).ToList();
                        break;
                    case RedmineKeys.CF_FLOAT:
                        CfFloatFilters = cfs.Select(cf =>
                        {
                            var filter = CfFloatFilters.FirstOrDefault(f => f.CustomField.Id == cf.Id);
                            return filter != null ? filter : new CfFloatFilterModel(cf);
                        }).ToList();
                        break;
                    case RedmineKeys.CF_STRING:
                        CfStringFilters = cfs.Select(cf =>
                        {
                            var filter = CfStringFilters.FirstOrDefault(f => f.CustomField.Id == cf.Id);
                            return filter != null ? filter : new CfTextSearchFilterModel(cf);
                        }).ToList();
                        break;
                    case RedmineKeys.CF_LINK:
                        CfLinkFilters = cfs.Select(cf =>
                        {
                            var filter = CfLinkFilters.FirstOrDefault(f => f.CustomField.Id == cf.Id);
                            return filter != null ? filter : new CfTextSearchFilterModel(cf);
                        }).ToList();
                        break;
                    case RedmineKeys.CF_TEXT:
                        CfLongTextFilters = cfs.Select(cf =>
                        {
                            var filter = CfLongTextFilters.FirstOrDefault(f => f.CustomField.Id == cf.Id);
                            return filter != null ? filter : new CfTextSearchFilterModel(cf);
                        }).ToList();
                        break;
                    case RedmineKeys.CF_BOOL:
                        CfBoolFilters = cfs.Select(cf =>
                        {
                            var filter = CfBoolFilters.FirstOrDefault(f => f.CustomField.Id == cf.Id);
                            return filter != null ? filter : new CfItemsFilterModel(cf);
                        }).ToList();
                        break;
                    case RedmineKeys.CF_ENUMERATION:
                        CfKeyValueFilters = cfs.Select(cf =>
                        {
                            var filter = CfKeyValueFilters.FirstOrDefault(f => f.CustomField.Id == cf.Id);
                            return filter != null ? filter : new CfItemsFilterModel(cf);
                        }).ToList();
                        break;
                }
            }
        }

        public void UpdateIsEnableds(List<string> selectedProjectIds, RedmineManager redmine)
        {
            var projects = selectedProjectIds != null ?
                redmine.Cache.Projects.Where(p => selectedProjectIds.Any(a => a == p.Id.ToString())).ToList() :
                redmine.Cache.Projects;
            var enableCfKeys = projects.Where(p => p.CustomFields != null)
                .SelectMany(p => p.CustomFields.Select(c => c.Id))
                .Select(id => redmine.Cache.CustomFields.FirstOrDefault(c => c.Id == id))
                .Where(c => c != null)
                .Select(c => $"cf_{c.Id}")
                .ToList();

            customFilters.ForEach(c => c.IsEnabled = enableCfKeys.Contains(c.RedmineKey));
        }

        public List<Issue> GetIssues(RedmineManager redmine)
        {
            var filters = standardFilters.Concat(customFilters).Where(f => f.NeedsFilter).ToList();

            var prms = new NameValueCollection();
            if (Category.NeedsFilter)
            {
                prms.Add(RedmineKeys.PROJECT_ID, Project.GetSelectedProjectId());
                // 検索条件は additionalQueries で指定するため仮の値を設定する
                prms.Add(RedmineKeys.CATEGORY_ID, "1");
            }

            return redmine.GetIssues(prms, filters.Select(f => f.GetQuery()).ToList());
        }

        public string CreateIssuesUrl(RedmineManager redmine)
        {
            var sb = new StringBuilder();
            if (Category.NeedsFilter)
                sb.Append($"{redmine.UrlBase}projects/{Project.GetSelectedProjectId()}/issues?utf8=%E2%9C%93&set_filter=1&sort=id%3Adesc");
            else
                sb.Append($"{redmine.UrlBase}issues?utf8=%E2%9C%93&set_filter=1&sort=id%3Adesc");

            var filters = standardFilters.Concat(customFilters).Where(f => f.NeedsFilter).ToList();
            foreach (var f in filters)
            {
                sb.Append(f.GetQuery());
            }

            return sb.ToString();
        }
    }
}
