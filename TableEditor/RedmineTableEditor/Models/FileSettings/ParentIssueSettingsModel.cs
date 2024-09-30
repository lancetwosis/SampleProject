using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.FileSettings.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTableEditor.Models.FileSettings
{
    public class ParentIssueSettingsModel : IssueSettingsModelBase
    {
        public bool UseQuery { get; set; }
        public Query Query { get; set; }
        public string IssueId { get; set; }
        public bool ShowParentIssue { get; set; }
        public bool Recoursive { get; set; }
        public FiltersModel Filters { get; set; }

        public ParentIssueSettingsModel() : base()
        {
            Filters = new FiltersModel();
        }

        public async Task<List<Issue>> GetIssuesAsync(RedmineManager redmine, CancellationToken ct)
        {
            return await Task.Run(async () =>
            {
                var results = UseQuery ?
                    redmine.GetIssues(Query) :
                    Filters.GetIssues(redmine);

                if (results == null || !results.Any())
                    return new List<Issue>();

                return results;
            });
        }

        public List<GridViewBoundColumnBase> CreateColumns(RedmineManager redmine)
        {
            var id = Properties.FirstOrDefault(p => p.IsType(IssuePropertyType.Id));
            if (id != null)
                Properties.Remove(id);
            var subject = Properties.FirstOrDefault(p => p.IsType(IssuePropertyType.Subject));
            if (subject != null)
                Properties.Remove(subject);

            var props = Properties.ToList();
            props.Insert(0, new FieldModel(IssuePropertyType.Id));
            props.Insert(1, new FieldModel(IssuePropertyType.Subject));
            return props.Select(p => p.CreateColumn(redmine)).Where(a => a != null).ToList();
        }

        public string CreateIssuesUrl(RedmineManager redmine)
        {
            if (UseQuery)
            {
                if (Query.ProjectId.HasValue)
                {
                    var proj = redmine.Cache.Projects.First(p => p.Id == Query.ProjectId.Value);
                    return $"{redmine.UrlBase}projects/{proj.Identifier}/issues?query_id={Query.Id}";
                }
                else
                {
                    return $"{redmine.UrlBase}issues?query_id={Query.Id}";
                }
            }
            else
            {
                return Filters.CreateIssuesUrl(redmine);
            }
        }
    }
}
