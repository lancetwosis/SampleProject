using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields
{
    public class ReviewSaveReviewerSettingModel : CustomFieldSettingModelBase
    {
        public ReviewSaveReviewerSettingModel() : base(CustomFieldFormat.User)
        {
            NeedsSaveToCustomField = true;
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
