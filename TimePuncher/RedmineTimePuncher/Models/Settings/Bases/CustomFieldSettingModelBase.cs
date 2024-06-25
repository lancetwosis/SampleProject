using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.Bases
{
    public abstract class CustomFieldSettingModelBase<TField, TValue> : LibRedminePower.Models.Bases.ModelBase
        where TField : MyCustomField<TValue> where TValue : MyCustomFieldPossibleValue
    {
        [JsonIgnore]
        public CustomFieldFormat Format { get; set; }
        public bool IsEnabled { get; set; }
        [JsonIgnore]
        public List<TValue> PossibleValues { get; set; } = new List<TValue>();
        public TValue Value { get; set; }

        public bool NeedsSaveToCustomField { get; set; }
        public List<TField> PossibleCustomFields { get; set; }
        public TField CustomField { get; set; }

        public CustomFieldSettingModelBase(CustomFieldFormat format)
        {
            Format = format;
        }

        public virtual void Update(List<TField> possibleCustomFields)
        {
        }

        protected void updateValue()
        {
            if (Value != null)
            {
                var value = PossibleValues.FirstOrDefault(v => v.Value == Value.Value);
                Value = value != null ? value : getDefaultValue();
            }
            else
            {
                Value = getDefaultValue();
            }
        }

        protected void updateCustomFields(List<TField> possibleCustomFields)
        {
            PossibleCustomFields = possibleCustomFields;
            if (!PossibleCustomFields.Any())
            {
                NeedsSaveToCustomField = false;
                return;
            }

            if (CustomField != null)
            {
                var c = PossibleCustomFields.FirstOrDefault(a => a.Id == CustomField.Id);
                CustomField = c != null ? c : PossibleCustomFields.First();
            }
            else
            {
                CustomField = PossibleCustomFields.First();
            }
        }

        public bool GetIssueCustomFieldIfNeeded(out IssueCustomField customField)
        {
            if (!IsEnabled || !NeedsSaveToCustomField)
            {
                customField = null;
                return false;
            }

            var selected = CustomField.PossibleValues.FirstOrDefault(v => v.Value == Value.Value);
            if (selected != null)
            {
                customField = new IssueCustomField()
                {
                    Id = CustomField.Id,
                    Name = CustomField.Name,
                    Values = new List<CustomFieldValue>() { new CustomFieldValue() { Info = selected.Value } },
                };
                return true;
            }
            else
            {
                customField = null;
                return false;
            }
        }

        public bool IsTrue()
        {
            return Value != null && Value.Value == MyCustomFieldPossibleValue.YES;
        }

        public string GetQueryString()
        {
            if (!IsEnabled || !NeedsSaveToCustomField || CustomField == null)
                return null;
            else
                return CustomField.CreateQueryString(Value.Value);
        }

        protected TValue getDefaultValue()
        {
            if (!PossibleValues.Any())
                return null;

            var d = PossibleValues.FirstOrDefault(v => v.IsDefault);
            return d != null ? d : PossibleValues.First();
        }
    }

    public abstract class CustomFieldSettingModelBase : CustomFieldSettingModelBase<MyCustomField, MyCustomFieldPossibleValue>
    {
        protected CustomFieldSettingModelBase(CustomFieldFormat format) : base(format)
        {
        }
    }
}
