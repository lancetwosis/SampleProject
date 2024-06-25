using LibRedminePower.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class ReviewIssueListSettingModel : Bases.SettingsModelBase<ReviewIssueListSettingModel>
    {
        [JsonIgnore]
        public ReactivePropertySlim<string> IsBusy { get; set; }

        public bool ShowDescription { get; set; } = true;
        public bool ShowLastNote { get; set; } = true;
        public ObservableCollection<IssueProperty> SelectedProperties { get; set; }
        public List<IssueProperty> AllProperties { get; set; }

        public ReviewIssueListSettingModel()
        {
            IsBusy = new ReactivePropertySlim<string>().AddTo(disposables);
            SelectedProperties = new ObservableCollection<IssueProperty>();
            AllProperties = new List<IssueProperty>();
        }

        public async Task SetupAsync(Managers.RedmineManager r)
        {
            try
            {
                IsBusy.Value = Resources.SettingsMsgNowGettingData;

                var customFields = CacheManager.Default.GetTemporaryCustomFields();

                AllProperties.Clear();
                AllProperties.Add(new IssueProperty(IssuePropertyType.Status));
                AllProperties.Add(new IssueProperty(IssuePropertyType.AssignedTo));
                AllProperties.Add(new IssueProperty(IssuePropertyType.Author));
                AllProperties.Add(new IssueProperty(IssuePropertyType.Priority));
                AllProperties.Add(new IssueProperty(IssuePropertyType.Category));
                AllProperties.Add(new IssueProperty(IssuePropertyType.StartDate));
                AllProperties.Add(new IssueProperty(IssuePropertyType.DueDate));
                AllProperties.Add(new IssueProperty(IssuePropertyType.DoneRatio));

                // 自分が担当しているプロジェクトで有効になっているカスタムフィールドのみを対象とする
                var enableCfIds = r.GetMyProjects().Where(p => p.CustomFields != null).SelectMany(p => p.CustomFields.Select(a => a.Id)).Distinct().ToList();
                var cfProperties = customFields.Where(c => c.IsIssueType() && enableCfIds.Contains(c.Id)).Select(a => new IssueProperty(a)).ToList();
                AllProperties.AddRange(cfProperties);

                var notExists = SelectedProperties.Where(p => !AllProperties.Contains(p)).ToList();
                foreach(var i in notExists)
                {
                    SelectedProperties.Remove(i);
                }
            }
            finally
            {
                IsBusy.Value = null;
            }
        }

        public string CreateShowAllPointIssuesUrl(Managers.RedmineManager redmine, Issue parent, int trackerId)
        {
            var sb = new StringBuilder();
            sb.Append(redmine.GetIssuesUrl(parent.Project.Id));
            sb.Append($"?utf8=%E2%9C%93&set_filter=1&sort=id:desc");
            sb.Append($"&f[]=parent_id&op[parent_id]=~&v[parent_id][]={parent.Id}");
            sb.Append($"&f[]=tracker_id&op[tracker_id]==&v[tracker_id][]={trackerId}");

            sb.Append($"&f[]=");
            sb.Append($"&c[]={RedmineKeys.SUBJECT}");
            foreach (var prop in SelectedProperties)
            {
                sb.Append($"&c[]={prop.Key}");
            }
            sb.Append($"&group_by=");

            if (ShowDescription)
                sb.Append($"&c[]=description");
            if (ShowLastNote)
                sb.Append($"&c[]=last_notes");

            return sb.ToString();
        }
    }
}
