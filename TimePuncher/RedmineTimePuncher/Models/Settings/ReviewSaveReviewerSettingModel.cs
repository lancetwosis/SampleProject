using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class ReviewSaveReviewerSettingModel : CustomFieldSettingModelBase
    {
        public ReviewSaveReviewerSettingModel() : base(CustomFieldFormat.User)
        {
            NeedsSaveToCustomField = true;
        }

        public override void Update(List<MyCustomField> possibleCustomFields)
        {
            NeedsSaveToCustomField = true;

            this.updateCustomFields(possibleCustomFields);

            // リスト形式のカスタムフィールドがない場合、
            // レビュー工程の指定はできないため、機能自体を無効にする
            if (!PossibleCustomFields.Any())
            {
                IsEnabled = false;
                return;
            }
        }

        public string GetQueryString(string userId)
        {
            if (!IsEnabled || CustomField == null)
                return null;
            else
                return CustomField.CreateQueryString(userId);
        }
    }
}
