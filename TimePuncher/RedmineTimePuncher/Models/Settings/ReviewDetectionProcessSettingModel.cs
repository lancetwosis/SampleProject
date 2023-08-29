using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class ReviewDetectionProcessSettingModel : CustomFieldSettingModelBase
    {
        public ReviewDetectionProcessSettingModel() : base(CustomFieldFormat.List)
        {
            NeedsSaveToCustomField = true;

            this.ObserveProperty(a => a.CustomField).SubscribeWithErr(cf =>
            {
                if (cf == null)
                    return;

                PossibleValues.Clear();
                PossibleValues.AddRange(cf.PossibleValues);
                if (Value != null && Value.Value != null)
                {
                    var first = PossibleValues.FirstOrDefault(v => v.Value == Value.Value);
                    Value = first != null ? first : PossibleValues.First();
                }
                else
                {
                    Value = PossibleValues.First();
                }
            }).AddTo(disposables);
        }

        public override void Update(List<MyCustomField> possibleCustomFields)
        {
            this.updateCustomFields(possibleCustomFields);

            // リスト形式のカスタムフィールドがない場合、
            // レビュー工程の指定はできないため、機能自体を無効にする
            if (!PossibleCustomFields.Any())
            {
                IsEnabled = false;
                return;
            }
        }

        public string GetQueryString()
        {
            if (!IsEnabled || CustomField == null)
                return null;
            else
                return CustomField.CreateQueryString(Value.Value);
        }
    }
}
