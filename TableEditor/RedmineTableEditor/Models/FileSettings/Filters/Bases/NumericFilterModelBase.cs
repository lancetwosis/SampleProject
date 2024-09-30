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
    public class NumericFilterModelBase<T> : FilterModelBase where T : IComparable<T>
    {
        public string Value { get; set; }
        public string Upper { get; set; }
        public string Lower { get; set; }

        public NumericFilterModelBase()
        {
        }

        public NumericFilterModelBase(string name, string key, bool isRequired) : base(name, key)
        {
            CompareTypes = new List<CompareTypeModel>()
            {
                CompareTypeModel.EQUALS,
                CompareTypeModel.GRATER_EQUAL,
                CompareTypeModel.LESS_EQUAL,
                CompareTypeModel.RANGE,
            };
            if (!isRequired)
            {
                CompareTypes.Add(CompareTypeModel.NONE);
                CompareTypes.Add(CompareTypeModel.ANY);
            }
            CompareType = CompareTypeModel.EQUALS;
        }

        public override string GetQuery()
        {
            // 以下のような文字列を作る
            // &f[]=subject&op[subject]=~&v[subject][]=Outlook
            var query = new StringBuilder();
            query.Append($"&f[]={RedmineKey}&op[{RedmineKey}]");
            query.Append($"{CompareType.GetEqualSign()}");

            if (CompareType.Equals(CompareTypeModel.EQUALS) ||
                CompareType.Equals(CompareTypeModel.GRATER_EQUAL) || CompareType.Equals(CompareTypeModel.LESS_EQUAL))
                query.Append($"&v[{RedmineKey}][]={parse(Value)}");
            else if (CompareType.Equals(CompareTypeModel.RANGE))
                query.Append($"&v[{RedmineKey}][]={parse(Lower)}&v[{RedmineKey}][]={parse(Upper)}");

            return query.ToString();
        }

        public override string Validate()
        {
            if (!NeedsFilter || !CompareType.NeedsInput())
                return null;

            if ((CompareType.Equals(CompareTypeModel.EQUALS) ||
                 CompareType.Equals(CompareTypeModel.GRATER_EQUAL) || CompareType.Equals(CompareTypeModel.LESS_EQUAL)) &&
                 !validate(Value, out var errMsg))
            {
                return errMsg;
            }

            if (CompareType.Equals(CompareTypeModel.RANGE))
            {
                if (!validate(Upper, out var err1))
                    return err1;
                else if (!validate(Lower, out var err2))
                    return err2;
                else if (parse(Lower).CompareTo(parse(Upper)) > 0)
                    return Properties.Resources.FilterErrMsgInvalidRange;
            }

            return null;
        }

        protected virtual bool validate(string value, out string errMsg)
        {
            throw new NotImplementedException();
        }

        protected virtual T parse(string value)
        {
            throw new NotImplementedException();
        }
    }
}
