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
    public class DateFilterModelBase : FilterModelBase
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public DateTime From { get; set; } = DateTime.Today.AddDays(-3);
        public DateTime To { get; set; } = DateTime.Today;
        public string LastNDays { get; set; }
        public string NextNDays { get; set; }

        public DateFilterModelBase()
        {
        }

        public DateFilterModelBase(string name, string key, bool needsNextDays = false) : base(name, key)
        {
            if (needsNextDays)
            {
                CompareTypes = new List<CompareTypeModel>()
                {
                    CompareTypeModel.EQUALS,
                    CompareTypeModel.AFTER,
                    CompareTypeModel.BEFORE,
                    CompareTypeModel.RANGE,
                    CompareTypeModel.NEXT_N_DAYS,
                    CompareTypeModel.LAST_N_DAYS,
                    CompareTypeModel.TOMORROW,
                    CompareTypeModel.TODAY,
                    CompareTypeModel.YESTERDAY,
                    CompareTypeModel.NEXT_WEEK,
                    CompareTypeModel.THIS_WEEK,
                    CompareTypeModel.LAST_WEEK,
                    CompareTypeModel.LAST_2_WEEKS,
                    CompareTypeModel.NEXT_MONTH,
                    CompareTypeModel.THIS_MONTH,
                    CompareTypeModel.LAST_MONTH,
                    CompareTypeModel.THIS_YEAR,
                };
            }
            else
            {
                CompareTypes = new List<CompareTypeModel>()
                {
                    CompareTypeModel.EQUALS,
                    CompareTypeModel.AFTER,
                    CompareTypeModel.BEFORE,
                    CompareTypeModel.RANGE,
                    CompareTypeModel.LAST_N_DAYS,
                    CompareTypeModel.TODAY,
                    CompareTypeModel.YESTERDAY,
                    CompareTypeModel.THIS_WEEK,
                    CompareTypeModel.LAST_WEEK,
                    CompareTypeModel.LAST_2_WEEKS,
                    CompareTypeModel.THIS_MONTH,
                    CompareTypeModel.LAST_MONTH,
                    CompareTypeModel.THIS_YEAR,
                };
            }
        }

        public override string GetQuery()
        {
            // 以下のような文字列を作る
            // &f[]=updated_on&op[updated_on]=><&v[updated_on][]=2024-08-15&v[updated_on][]=2024-08-23
            var query = new StringBuilder();
            query.Append($"&f[]={RedmineKey}&op[{RedmineKey}]");
            query.Append($"{CompareType.GetEqualSign()}");

            if (CompareType.Equals(CompareTypeModel.EQUALS) ||
                CompareType.Equals(CompareTypeModel.AFTER) || CompareType.Equals(CompareTypeModel.BEFORE))
                query.Append($"&v[{RedmineKey}][]={Date.ToString("yyyy-MM-dd")}");
            else if (CompareType.Equals(CompareTypeModel.RANGE))
                query.Append($"&v[{RedmineKey}][]={From.ToString("yyyy-MM-dd")}&v[{RedmineKey}][]={To.ToString("yyyy-MM-dd")}");
            else if (CompareType.Equals(CompareTypeModel.LAST_N_DAYS))
                query.Append($"&v[{RedmineKey}][]={LastNDays}");
            else if (CompareType.Equals(CompareTypeModel.NEXT_N_DAYS))
                query.Append($"&v[{RedmineKey}][]={NextNDays}");

            return query.ToString();
        }

        public override string Validate()
        {
            if (!NeedsFilter || !CompareType.NeedsInput())
                return null;

            if (CompareType.Equals(CompareTypeModel.NEXT_N_DAYS) &&
                (string.IsNullOrEmpty(NextNDays) || !Regex.IsMatch(NextNDays, "^[0-9]+$")))
                return Properties.Resources.FilterErrMsgInputDate;
            if (CompareType.Equals(CompareTypeModel.LAST_N_DAYS) &&
                (string.IsNullOrEmpty(LastNDays) || !Regex.IsMatch(LastNDays, "^[0-9]+$")))
                return Properties.Resources.FilterErrMsgInputDate;

            if (CompareType.Equals(CompareTypeModel.RANGE) && From > To)
                return Properties.Resources.FilterErrMsgInvalidRange;

            return null;
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            Date = DateTime.Today;
            From = DateTime.Today.AddDays(-3);
            To = DateTime.Today;
            LastNDays = null;
            NextNDays = null;
        }
    }
}
