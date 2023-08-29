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
    public abstract class IssueSettingsModelBase : LibRedminePower.Models.Bases.ModelBase
    {
        public ObservableCollection<FieldModel> Properties { get; set; }

        public IssueSettingsModelBase()
        {
            Properties = new ObservableCollection<FieldModel>();
        }
    }
}
