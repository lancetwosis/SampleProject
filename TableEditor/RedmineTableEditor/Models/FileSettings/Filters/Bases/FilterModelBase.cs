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

namespace RedmineTableEditor.Models.FileSettings.Filters.Bases
{
    public class FilterModelBase : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public bool IsEnabled { get; set; } = true;

        public bool IsSelected { get; set; }
        public bool IsChecked { get; set; } = true;
        public string Name { get; set; }
        public CompareTypeModel CompareType { get; set; }
        [JsonIgnore]
        public List<CompareTypeModel> CompareTypes { get; set; }

        public string RedmineKey { get; set; }

        public bool NeedsFilter => IsEnabled && IsSelected && IsChecked;

        public FilterModelBase()
        {
        }

        public FilterModelBase(string name, string key)
        {
            RedmineKey = key;
            Name = name;

            SetDefaultCompareType();
        }

        public virtual string GetQuery()
        {
            throw new NotImplementedException();
        }

        public virtual string Validate()
        {
            throw new NotImplementedException();
        }

        public virtual void SetDefaultCompareType()
        {
            CompareType = CompareTypeModel.EQUALS;
        }

        public virtual void SetDefaults()
        {
            IsChecked = true;
            SetDefaultCompareType();
        }
    }
}
