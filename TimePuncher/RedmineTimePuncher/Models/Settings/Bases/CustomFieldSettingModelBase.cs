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
    public abstract class CustomFieldSettingModelBase : LibRedminePower.Models.Bases.ModelBase
    {
        public CustomFieldFormat Format { get; set; }
        public bool IsEnabled { get; set; }
        [JsonIgnore]
        public List<MyCustomFieldPossibleValue> PossibleValues { get; set; } = new List<MyCustomFieldPossibleValue>();
        public MyCustomFieldPossibleValue Value { get; set; }

        public bool NeedsSaveToCustomField { get; set; }
        public List<MyCustomField> PossibleCustomFields { get; set; }
        public MyCustomField CustomField { get; set; }

        public CustomFieldSettingModelBase(CustomFieldFormat format)
        {
            Format = format;
        }

        public virtual void Update(List<MyCustomField> possibleCustomFields)
        {
        }

        protected void updateValue()
        {
            if (Value != null)
            {
                var value = PossibleValues.FirstOrDefault(v => v.Value == Value.Value);
                Value = value != null ? value : GetDefaultValue();
            }
            else
            {
                Value = GetDefaultValue();
            }
        }

        protected void updateCustomFields(List<MyCustomField> possibleCustomFields)
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

        public bool GetIssueCustomField(out IssueCustomField customField)
        {
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

        public MyCustomFieldPossibleValue GetDefaultValue()
        {
            if (!PossibleValues.Any())
                return null;

            var d = PossibleValues.FirstOrDefault(v => v.IsDefault);
            return d != null ? d : PossibleValues.First();
        }
    }
}
