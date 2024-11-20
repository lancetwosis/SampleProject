using LibRedminePower;
using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.CreateTicket.Common
{
    public class AssigneeModel : IdName
    {
        public bool IsRequired { get; set; } = true;

        [Obsolete("For Serialize", true)]
        public AssigneeModel()
        { }

        public AssigneeModel(IdentifiableName identifiable) : base(identifiable)
        {
        }

        public string GetPostFix(bool isSelfReview = false)
        {
            var postFix = $"({Name})";
            if (isSelfReview)
            {
                postFix += $" ({Resources.ReviewSelfReview})";
            }
            else
            {
                if (SettingsModel.Default.CreateTicket.IsRequired.IsEnabled && !IsRequired)
                    postFix += $" ({Resources.ReviewReviewerOptional})";
            }
            return postFix;
        }

        public IssueCustomField GetIssueCustomField()
        {
            if (!SettingsModel.Default.CreateTicket.IsRequired.IsEnabled ||
                !SettingsModel.Default.CreateTicket.IsRequired.NeedsSaveToCustomField)
            {
                return null;
            }

            var v = IsRequired ? ReviewIsRequiredSettingModel.REQUIRED : ReviewIsRequiredSettingModel.OPTIONAL;
            return SettingsModel.Default.CreateTicket.IsRequired.CreateIssueCustomField(v);
        }

        public override bool Equals(object obj)
        {
            return this.JsonEquals(obj);
        }

        public override int GetHashCode()
        {
            int hashCode = -601266842;
            hashCode = hashCode * -1521134295 + this.GetJsonHashcode();
            return hashCode;
        }
    }
}
