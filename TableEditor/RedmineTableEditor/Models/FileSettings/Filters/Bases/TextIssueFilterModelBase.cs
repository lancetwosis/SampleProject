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
using System.Web;
using System.Text.RegularExpressions;

namespace RedmineTableEditor.Models.FileSettings.Filters.Bases
{
    public class TextIssueFilterModelBase : TextFilterModelBase
    {
        public TextIssueFilterModelBase(string name, string key) : base(name, key)
        {
            CompareTypes = new List<CompareTypeModel>()
            {
                CompareTypeModel.EQUALS,
                CompareTypeModel.CONTAINS,
                CompareTypeModel.NONE
            };
        }

        public override string Validate()
        {
            if (!NeedsFilter || !CompareType.NeedsInput())
                return null;

            if (string.IsNullOrEmpty(Text) || !Regex.IsMatch(Text, "^[0-9]+$"))
                return string.Format(Properties.Resources.FilterErrMsgInputIssueNo, Name);

            return null;
        }
    }
}
