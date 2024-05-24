using LibRedminePower.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
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
        [JsonIgnore]
        public ReactivePropertySlim<string> IsBusy { get; set; }

        public ReviewDetectionProcessSettingModel DetectionProcess { get; set; }
        public bool NeedsOutlookIntegration { get; set; } = true;
        public bool NeedsGitIntegration { get; set; }

        public MyTracker OpenTracker { get; set; }
        public IdName OpenStatus { get; set; }
        public IdName DefaultStatus { get; set; }
        public ReviewNeedsFaceToFaceSettingModel NeedsFaceToFace { get; set; }

        public MyTracker RequestTracker { get; set; }
        public ReviewIsRequiredSettingModel IsRequired { get; set; }

        public MyTracker PointTracker { get; set; }
        public ReviewSaveReviewerSettingModel SaveReviewer { get; set; }

        [JsonIgnore]
        public List<MyTracker> Trackers { get; set; }
        [JsonIgnore]
        public List<IdName> Statuses { get; set; }

        public CreateTicketSettingsModel()
        {
            IsBusy = new ReactivePropertySlim<string>().AddTo(disposables);

            DetectionProcess = new ReviewDetectionProcessSettingModel().AddTo(disposables);
            NeedsFaceToFace = new ReviewNeedsFaceToFaceSettingModel().AddTo(disposables);
            IsRequired = new ReviewIsRequiredSettingModel().AddTo(disposables);
            SaveReviewer = new ReviewSaveReviewerSettingModel().AddTo(disposables);
        }

        public async Task SetupAsync(Managers.RedmineManager r)
        {
            try
            {
                IsBusy.Value = Resources.SettingsMsgNowGettingData;

                var trackers = await Task.Run(() => r.Trackers.Value);
                var customFields = await Task.Run(() => r.CustomFields.Value);
                var statuses = await Task.Run(() => r.Statuss.Value);

                if (!trackers.Any())
                    throw new ApplicationException(Properties.Resources.SettingsReviErrMsgNoTrackers);

                Trackers = trackers.Select(t => new MyTracker(t)).ToList();
                Trackers.Insert(0, MyTracker.USE_PARENT_TRACKER);

                OpenTracker = Trackers.FirstOrDefault(OpenTracker);
                RequestTracker = Trackers.FirstOrDefault(RequestTracker);
                PointTracker = Trackers.FirstOrDefault(PointTracker);

                Statuses = statuses.Select(s => new IdName(s)).ToList();
                OpenStatus = Statuses.FirstOrDefault(OpenStatus);
                DefaultStatus = Statuses.FirstOrDefault(DefaultStatus);

                var boolCustomFields = customFields.Where(c => c.IsIssueType() && c.IsBoolFormat()).Select(c => new MyCustomField(c)).ToList();
                var listCustomFields = customFields.Where(c => c.IsIssueType() && c.IsListFormat()).Select(c => new MyCustomField(c)).ToList();
                var userCustomFields = customFields.Where(c => c.IsIssueType() && c.IsUserFormat()).Select(c => new MyCustomField(c)).ToList();

                NeedsFaceToFace.Update(boolCustomFields);
                IsRequired.Update(boolCustomFields);
                DetectionProcess.Update(listCustomFields);
                SaveReviewer.Update(userCustomFields);
            }
            finally
            {
                IsBusy.Value = null;
            }
        }

        public bool IsValid()
        {
            return OpenTracker != null && RequestTracker != null && PointTracker != null;
        }
    }
}
