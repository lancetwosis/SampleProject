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
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.FileSettings
{
    public class ParentIssueSettingsModel : IssueSettingsModelBase
    {
        public bool UseQuery { get; set; } = true;
        public Query Query { get; set; }
        public string IssueId { get; set; }
        public bool Recoursive { get; set; }

        public ParentIssueSettingsModel() : base()
        {
        }
    }
}
