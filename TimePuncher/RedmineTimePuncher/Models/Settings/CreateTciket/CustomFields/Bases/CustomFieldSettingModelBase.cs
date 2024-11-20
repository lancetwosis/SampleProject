using LibRedminePower.Extentions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields.Bases
{
    public abstract class CustomFieldSettingModelBase<TField, TValue> : LibRedminePower.Models.Bases.ModelBase
        where TField : MyCustomField<TValue> where TValue : MyCustomFieldPossibleValue
    {
        [JsonIgnore]
        public CustomFieldFormat Format { get; set; }
        public bool IsEnabled { get; set; }

        public bool NeedsSaveToCustomField { get; set; }
        public TField CustomField { get; set; }

        public CustomFieldSettingModelBase(CustomFieldFormat format)
        {
            Format = format;
        }


        /// <summary>
        /// キャッシュのカスタムフィールドを設定に反映する。これによりユーザが画面で選択できる選択肢が更新される。
        /// </summary>
        public void ApplyCustomField()
        {
            if (!IsEnabled || !NeedsSaveToCustomField || CustomField == null)
                return;

            var newCf = CacheManager.Default.CustomFields.FirstOrDefault(c => c.Id == CustomField.Id);
            if (newCf == null)
                throw new ApplicationException(string.Format(Resources.SettingsReviMsgNotExistCustomField, CustomField.Name));

            CustomField = CreateMyCustomField(newCf);
        }

        public virtual TField CreateMyCustomField(CustomField cf)
        {
            throw new NotImplementedException();
        }

        public virtual List<TValue> GetPossibleValues()
        {
            return !IsEnabled || CustomField == null || CustomField.PossibleValues.IsEmpty() ?
                new List<TValue>() : CustomField.PossibleValues;
        }

        public IssueCustomField CreateIssueCustomField(TValue value)
        {
            return new IssueCustomField()
            {
                Id = CustomField.Id,
                Name = CustomField.Name,
                Values = new List<CustomFieldValue>() { new CustomFieldValue() { Info = value.Value } },
            };
        }
    }

    public abstract class CustomFieldSettingModelBase : CustomFieldSettingModelBase<MyCustomField, MyCustomFieldPossibleValue>
    {
        protected CustomFieldSettingModelBase(CustomFieldFormat format) : base(format)
        {
        }

        public override MyCustomField CreateMyCustomField(CustomField cf)
        {
            return new MyCustomField(cf);
        }
    }
}
