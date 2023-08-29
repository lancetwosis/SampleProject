using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket
{
    public class MemberViewModel : IdName
    {
        public IdentifiableName User { get; set; }

        [JsonIgnore]
        public ReactivePropertySlim<bool> IsRequiredReviewer { get; set; }

        public ReviewIsRequiredSettingModel IsRequired { get; set; }

        public MemberViewModel()
        { }

        public MemberViewModel(IdentifiableName user, ReviewIsRequiredSettingModel isRequired) : base(user)
        {
            User = user;

            IsRequired = isRequired.Clone();
            IsRequiredReviewer = IsRequired.ToReactivePropertySlimAsSynchronized(s => s.Value,
                v => v != null ? v.Value == MyCustomFieldPossibleValue.YES : false,
                required => required ? IsRequired.PossibleValues[0] : IsRequired.PossibleValues[1]);
        }

        public int GetRecipientType()
        {
            if (IsRequired.IsEnabled)
                return IsRequiredReviewer.Value ? (int)OlMeetingRecipientType.olRequired : (int)OlMeetingRecipientType.olOptional;
            else
                return (int)OlMeetingRecipientType.olRequired;
        }

        public string GetPostFix()
        {
            var postFix = $"({Name})";
            if (IsRequired.IsEnabled && !IsRequired.IsTrue())
            {
                postFix += $" ({Resources.ReviewReviewerOptional})";
            }
            return postFix;
        }

        public List<IssueCustomField> GetCustomFieldIfNeeded()
        {
            if (IsRequired.IsEnabled &&
                IsRequired.NeedsSaveToCustomField &&
                IsRequired.GetIssueCustomField(out var cf))
            {
                return new List<IssueCustomField>() { cf };
            }

            return null;
        }

        public override string ToString()
        {
            return User.Name;
        }
    }
}
