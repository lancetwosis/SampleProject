using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
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
            Value = getDefaultValue();

            // レビュー方法の指定はデフォルトで有効に
            IsEnabled = true;

            this.ObserveProperty(a => a.NeedsSaveToCustomField).CombineLatest(this.ObserveProperty(a => a.CustomField), (_1, _2) => (_1, _2)).Subscribe(_ =>
                {
                    if (NeedsSaveToCustomField && CustomField == null)
                        return;

                    PossibleValues = NeedsSaveToCustomField ? CustomField.PossibleValues : defaultValues;
                    this.updateValue();
                }).AddTo(disposables);

            this.fieldCreater = (cf) => new ReviewMethodCustomField(new MyCustomField(cf), CustomField);
        }

        public void Update(List<MyCustomField> possibleCustomFields)
        {
            PossibleCustomFields = possibleCustomFields.Select(cf =>
            {
                return PossibleCustomFields != null ?
                       new ReviewMethodCustomField(cf, PossibleCustomFields.FirstOrDefault(pre => pre.Id == cf.Id)) :
                       new ReviewMethodCustomField(cf, null);
            }).ToList();

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
    }

    public class ReviewMethodCustomField : MyCustomField<ReviewMethodValue>
    {
        public ReviewMethodCustomField()
        {
        }

        public ReviewMethodCustomField(MyCustomField customField, ReviewMethodCustomField prev)
        {
            if (customField.Format != CustomFieldFormat.List)
                throw new InvalidOperationException();

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
