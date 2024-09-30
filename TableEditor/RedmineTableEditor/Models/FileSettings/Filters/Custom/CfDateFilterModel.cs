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
using System.Text.Json.Serialization;
using LibRedminePower.Models;
using RedmineTableEditor.Models.FileSettings.Filters.Bases;

namespace RedmineTableEditor.Models.FileSettings.Filters.Custom
{
    public class CfDateFilterModel : DateFilterModelBase
    {
        public CustomField CustomField { get; set; }

        public CfDateFilterModel()
        {
        }

        [JsonConstructor]
        public CfDateFilterModel(CustomField customField)
            : base(customField.Name, $"cf_{customField.Id}", true)
        {
            CustomField = customField;
            if (!customField.IsRequired)
            {
                CompareTypes.Add(CompareTypeModel.NONE);
                CompareTypes.Add(CompareTypeModel.ANY);
            }
        }
    }
}
