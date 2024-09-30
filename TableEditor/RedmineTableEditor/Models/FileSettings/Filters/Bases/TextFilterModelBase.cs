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

namespace RedmineTableEditor.Models.FileSettings.Filters.Bases
{
    public class TextFilterModelBase : FilterModelBase
    {
        public string Text { get; set; }

        public TextFilterModelBase()
        {
        }

        public TextFilterModelBase(string name, string key) : base(name, key)
        {
        }

        public override string GetQuery()
        {
            // 以下のような文字列を作る
            // &f[]=subject&op[subject]=~&v[subject][]=Outlook
            var query = new StringBuilder();
            query.Append($"&f[]={RedmineKey}&op[{RedmineKey}]");
            query.Append($"{CompareType.GetEqualSign()}");

            if (CompareType.Equals(CompareTypeModel.NONE))
                return query.ToString();

            query.Append($"&v[{RedmineKey}][]={HttpUtility.UrlEncode(Text)}");
            return query.ToString();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            Text = null;
        }
    }
}
