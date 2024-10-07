using LibRedminePower.Enums;
using LibRedminePower.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class CreateTicketSettingsModel : Bases.SettingsModelBase<CreateTicketSettingsModel>
    {
        public ReviewDetectionProcessSettingModel DetectionProcess { get; set; } = new ReviewDetectionProcessSettingModel();
        public bool NeedsOutlookIntegration { get; set; } = true;
        public bool NeedsGitIntegration { get; set; }

        public MyTracker OpenTracker { get; set; }
        public IdName OpenStatus { get; set; }
        public IdName DefaultStatus { get; set; }
        public ReviewMethodSettingModel ReviewMethod { get; set; } = new ReviewMethodSettingModel();

        public MyTracker RequestTracker { get; set; }
        public ReviewIsRequiredSettingModel IsRequired { get; set; } = new ReviewIsRequiredSettingModel();

        public MyTracker PointTracker { get; set; }
        public ReviewSaveReviewerSettingModel SaveReviewer { get; set; } = new ReviewSaveReviewerSettingModel();

        public CreateTicketSettingsModel()
        { }

        public void Update(List<MyCustomField> boolCustomFields, List<MyCustomField> listCustomFields, List<MyCustomField> userCustomFields)
        {
            ReviewMethod.Update(listCustomFields);
            IsRequired.Update(boolCustomFields);
            DetectionProcess.Update(listCustomFields);
            SaveReviewer.Update(userCustomFields);
        }

        public bool IsValid()
        {
            return OpenTracker != null && RequestTracker != null && PointTracker != null;
        }

        public List<int> GetSettingCustomFieldIds()
        {
            var result = new List<int>();
            if (DetectionProcess.IsEnabled)
                result.Add(DetectionProcess.CustomField.Id);

            if (ReviewMethod.IsEnabled && ReviewMethod.NeedsSaveToCustomField)
                result.Add(ReviewMethod.CustomField.Id);

            if (IsRequired.IsEnabled && IsRequired.NeedsSaveToCustomField)
                result.Add(IsRequired.CustomField.Id);

            if (SaveReviewer.IsEnabled)
                result.Add(SaveReviewer.CustomField.Id);

            return result;
        }
    }
}
