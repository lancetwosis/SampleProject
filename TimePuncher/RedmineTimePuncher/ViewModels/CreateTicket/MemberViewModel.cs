using LibRedminePower.Extentions;
using LibRedminePower.Models;
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
using System.Reactive.Disposables;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket
{
    public class MemberViewModel : IdName, IDisposable
    {
        public IdentifiableName User { get; set; }

        [JsonIgnore]
        public ReactivePropertySlim<bool> IsRequiredReviewer { get; set; }

        public ReviewIsRequiredSettingModel IsRequired { get; set; }

        [NonSerialized]
        protected CompositeDisposable disposables = new CompositeDisposable();
        public virtual void Dispose() => disposables.Dispose();

        public MemberViewModel()
        { }

        public MemberViewModel(IdentifiableName user, ReviewIsRequiredSettingModel isRequired) : base(user)
        {
            User = user;

            IsRequired = isRequired.Clone();
            IsRequiredReviewer = IsRequired.ToReactivePropertySlimAsSynchronized(s => s.Value,
                v => v != null ? v.Value == MyCustomFieldPossibleValue.YES : false,
                required => required ? IsRequired.PossibleValues[0] : IsRequired.PossibleValues[1]).AddTo(disposables);
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

        public override string ToString()
        {
            return Name;
        }
    }
}
