using LibRedminePower;
using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields.Bases;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields
{
    public class ReviewMethodSettingModel : CustomFieldSettingModelBase<ReviewMethodCustomField, ReviewMethodValue>
    {
        private List<ReviewMethodValue> defaultValues = new List<ReviewMethodValue>()
        {
            new ReviewMethodValue(Resources.ReviewMethodDesktop, true, false),
            new ReviewMethodValue(Resources.ReviewMethodFaceToFace, false, true),
            new ReviewMethodValue($"{Resources.ReviewMethodDesktop} + {Resources.ReviewMethodFaceToFace}", true, true),
        };

        public ReviewMethodSettingModel() : base(CustomFieldFormat.List)
        {
            // レビュー方法の指定はデフォルトで有効に
            IsEnabled = true;
        }

        public override ReviewMethodCustomField CreateMyCustomField(CustomField cf)
        {
            return new ReviewMethodCustomField(new MyCustomField(cf), CustomField);
        }

        public override List<ReviewMethodValue> GetPossibleValues()
        {
            if (!IsEnabled)
                return new List<ReviewMethodValue>();

            if (NeedsSaveToCustomField)
            {
                return CustomField != null ? CustomField.PossibleValues : new List<ReviewMethodValue>();
            }
            else
            {
                return defaultValues;
            }
        }
    }

    public class ReviewMethodCustomField : MyCustomField<ReviewMethodValue>
    {
        public ReviewMethodCustomField()
        {
        }

        public ReviewMethodCustomField(MyCustomField customField, ReviewMethodCustomField prev)
        {
            if (customField.Format != CustomFieldFormat.List)
                throw new NotSupportedException();

            Id = customField.Id;
            Name = customField.Name;

            Format = CustomFieldFormat.List;
            PossibleValues = customField.PossibleValues.Select((v, i) =>
            {
                if (prev != null)
                {
                    var pValue =prev.PossibleValues.FirstOrDefault(pre => pre.Label == v.Label);
                    if (pValue != null)
                        return pValue;
                }

                if (i == 0)
                    return new ReviewMethodValue(v, true, false);
                else if (i == 1)
                    return new ReviewMethodValue(v, false, true);
                else if (i == 2)
                    return new ReviewMethodValue(v, true, true);
                else
                    return new ReviewMethodValue(v, false, false);
            }).ToList();
        }

        public override bool Equals(object obj)
        {
            return this.JsonEquals(obj);
        }

        public override int GetHashCode()
        {
            int hashCode = 1513809992;
            hashCode = hashCode * -1521134295 + this.GetJsonHashcode();
            return hashCode;
        }
    }

    public class ReviewMethodValue : MyCustomFieldPossibleValue
    {
        public bool NeedsOnDesktop { get; set; }
        public bool NeedsFaceToFace { get; set; }

        public ReviewMethodValue()
        {
        }

        public ReviewMethodValue(string label, bool needsOnDesktop, bool needsFaceToFace)
        {
            Label = label;
            Value = label;

            NeedsOnDesktop = needsOnDesktop;
            NeedsFaceToFace = needsFaceToFace;
        }

        public ReviewMethodValue(MyCustomFieldPossibleValue value, bool needsOnDesktop, bool needsFaceToFace)
            :this(value.Label, needsOnDesktop, needsFaceToFace)
        {
        }
    }
}
