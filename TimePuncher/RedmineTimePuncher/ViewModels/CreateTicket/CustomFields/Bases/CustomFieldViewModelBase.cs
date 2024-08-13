using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RedmineTimePuncher.ViewModels.CreateTicket.CustomFields.Bases
{

    public class CustomFieldViewModelBase : ViewModelBase
    {
        public string Value { get; set; }

        public CustomField CustomField { get; set; }

        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<bool> HasError { get; set; }

        public CustomFieldViewModelBase()
        {
        }

        public CustomFieldViewModelBase(CustomField cf, MyIssue ticket = null)
        {
            CustomField = cf;


            ErrorMessage = this.ObserveProperty(a => a.Value).Select(_ => Validate()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            HasError = ErrorMessage.Select(m => m != null).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            setup(cf, ticket);

            if (string.IsNullOrEmpty(Value) && CustomField != null)
                SetValue(CustomField.DefaultValue);
        }

        protected virtual void setup(CustomField cf, MyIssue ticket)
        {
        }

        public virtual void SetValue(string value)
        {
            Value = value;
        }

        public void SetValue(IssueCustomField cf)
        {
            if (cf.Values == null)
                return;

            if (cf.Multiple)
            {
                SetValue(string.Join(",", cf.Values.Select(a => a.Info)));
            }
            else
            {
                SetValue(cf.Values[0].Info);
            }
        }

        /// <summary>
        /// 型に応じた正しい値が設定されているかどうかを返す
        /// </summary>
        public virtual string Validate()
        {
            if (string.IsNullOrEmpty(Value))
            {
                if (CustomField.IsRequired)
                    return Resources.ReviewCfErrMsgIsRequired;
                else
                    return null;
            }

            if (!string.IsNullOrEmpty(CustomField.Regexp) && !Regex.IsMatch(Value, CustomField.Regexp))
            {
                return string.Format(Resources.ReviewCfErrMsgRegex, CustomField.Regexp);
            }

            if (CustomField.MinLength.HasValue && Value.Length < CustomField.MinLength.Value)
            {
                return string.Format(Resources.ReviewCfErrMsgMinLength, CustomField.MinLength.Value);
            }

            if (CustomField.MaxLength.HasValue && Value.Length > CustomField.MaxLength.Value)
            {
                return string.Format(Resources.ReviewCfErrMsgMaxLength, CustomField.MaxLength.Value);
            }

            return null;
        }

        /// <summary>
        /// チケットに反映する必要があるかどうかを返す
        /// </summary>
        public bool NeedsApply()
        {
            return !string.IsNullOrEmpty(Value);
        }

        /// <summary>
        /// Issue の CustomFields に設定するための IssueCustomField を返す
        /// </summary>
        public IssueCustomField ToIssueCustomField()
        {
            var cf = new IssueCustomField()
            {
                Id = CustomField.Id,
                Name = CustomField.Name,
            };

            if (CustomField.Multiple)
            {
                cf.Values = Value.Split(',').Select(v => new CustomFieldValue() { Info = v }).ToList();
            }
            else
            {
                cf.Values = new List<CustomFieldValue>() { new CustomFieldValue() { Info = Value } };
            }

            return cf;
        }

        /// <summary>
        /// 指摘チケット作成のURLに本カスタムフィールドを設定するための文字列を返す
        /// </summary>
        public List<string> ToQueryStrings()
        {
            if (CustomField.Multiple)
            {
                return Value.Split(',')
                    .Select(v => string.Format(MyCustomField.MULTI_QUERY_FORMAT, CustomField.Id, HttpUtility.UrlEncode(v)))
                    .ToList();
            }
            else
            {
                return new List<string>()
                {
                    string.Format(MyCustomField.QUERY_FORMAT, CustomField.Id, HttpUtility.UrlEncode(Value))
                };
            }
        }
    }


}
