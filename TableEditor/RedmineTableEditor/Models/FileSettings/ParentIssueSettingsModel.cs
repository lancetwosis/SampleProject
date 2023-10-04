using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
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

namespace RedmineTableEditor.Models.FileSettings
{
    public class ParentIssueSettingsModel : IssueSettingsModelBase
    {
        public bool UseQuery { get; set; } = true;
        public Query Query { get; set; }
        public string IssueId { get; set; }
        public bool ShowParentIssue { get; set; }
        public bool Recoursive { get; set; }

        public ParentIssueSettingsModel() : base()
        {
        }

        public async Task<List<Issue>> GetIssuesAsync(RedmineManager redmine, CancellationToken ct)
        {
            return await Task.Run(async () =>
            {
                var results = UseQuery ?
                    redmine.GetIssues(Query) :
                    redmine.GetChildIssues(int.Parse(IssueId), Recoursive, ShowParentIssue);

                if (results == null || !results.Any())
                    return new List<Issue>();

                var needsDetail = Properties.Any(a => a.IsDetail());
                if (needsDetail)
                {
                    var tmp = await Task.WhenAll(results.Select(a => Task.Run(() => redmine.GetIssue(a.Id)))).WithCancel(ct);
                    return tmp.ToList();
                }
                else
                {
                    return results;
                }
            });
        }
    }
}
