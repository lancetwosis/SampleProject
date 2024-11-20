using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Review;
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

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Bases
{

    public class CustomFieldViewModelBase : ViewModelBase
    {
        public CustomFieldValueModel Model { get; set; }

        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<bool> HasError { get; set; }

        public CustomFieldViewModelBase()
        {
        }

        public CustomFieldViewModelBase(CustomFieldValueModel model, MyIssue ticket = null)
        {
            Model = model;

            ErrorMessage = Model.ObserveProperty(a => a.Value).Select(_ => Validate()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            HasError = ErrorMessage.Select(m => m != null).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // setup の中で Model.Value をクリアしてしまう場合があるため、退避させる
            var prevValue = Model.Value;
            setup(model, ticket);

            SetValue(prevValue);
        }

        protected virtual void setup(CustomFieldValueModel cf, MyIssue ticket)
        {
        }

        public virtual void SetValue(string value)
        {
            Model.Value = value;
        }

        public void SetValue(IssueCustomField cf)
        {
            if (cf.Values == null)
                SetValue("");
            else if (cf.Multiple)
                SetValue(string.Join(",", cf.Values.Select(a => a.Info)));
            else
                SetValue(cf.Values[0].Info);
        }

        /// <summary>
        /// 型に応じた正しい値が設定されているかどうかを返す
        /// </summary>
        public virtual string Validate()
        {
            if (string.IsNullOrEmpty(Model.Value))
            {
                if (Model.CustomField.IsRequired)
                    return Resources.ReviewCfErrMsgIsRequired;
                else
                    return null;
            }

            if (!string.IsNullOrEmpty(Model.CustomField.Regexp) && !Regex.IsMatch(Model.Value, Model.CustomField.Regexp))
            {
                return string.Format(Resources.ReviewCfErrMsgRegex, Model.CustomField.Regexp);
            }

            if (Model.CustomField.MinLength.HasValue && Model.Value.Length < Model.CustomField.MinLength.Value)
            {
                return string.Format(Resources.ReviewCfErrMsgMinLength, Model.CustomField.MinLength.Value);
            }

            if (Model.CustomField.MaxLength.HasValue && Model.Value.Length > Model.CustomField.MaxLength.Value)
            {
                return string.Format(Resources.ReviewCfErrMsgMaxLength, Model.CustomField.MaxLength.Value);
            }

            return null;
        }

        /// <summary>
        /// チケットに反映する必要があるかどうかを返す
        /// </summary>
        public bool NeedsApply()
        {
            return !string.IsNullOrEmpty(Model.Value);
        }

        /// <summary>
        /// Issue の CustomFields に設定するための IssueCustomField を返す
        /// </summary>
        public IssueCustomField ToIssueCustomField()
        {
            var cf = new IssueCustomField()
            {
                Id = Model.CustomField.Id,
                Name = Model.CustomField.Name,
            };

            if (Model.CustomField.Multiple)
            {
                cf.Values = Model.Value.Split(',').Select(v => new CustomFieldValue() { Info = v }).ToList();
            }
            else
            {
                cf.Values = new List<CustomFieldValue>() { new CustomFieldValue() { Info = Model.Value } };
            }

            return cf;
        }

        /// <summary>
        /// 指摘チケット作成のURLに本カスタムフィールドを設定するための文字列を返す
        /// </summary>
        public List<string> ToQueryStrings()
        {
            if (Model.CustomField.Multiple)
            {
                return Model.Value.Split(',')
                    .Select(v => string.Format(MyCustomField.MULTI_QUERY_FORMAT, Model.CustomField.Id, HttpUtility.UrlEncode(v)))
                    .ToList();
            }
            else
            {
                return new List<string>()
                {
                    string.Format(MyCustomField.QUERY_FORMAT, Model.CustomField.Id, HttpUtility.UrlEncode(Model.Value))
                };
            }
        }
    }


}
