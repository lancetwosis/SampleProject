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
using System.Web;

namespace RedmineTimePuncher.Models.Settings
{
    public class ReviewCopyCustomFieldsSettingModel : Bases.SettingsModelBase<ReviewIssueListSettingModel>
    {
        [JsonIgnore]
        public ReactivePropertySlim<string> IsBusy { get; set; }

        public ObservableCollection<MyCustomField> SelectedCustomFields { get; set; }
        public List<MyCustomField> AllCustomFields { get; set; }

        public ReviewCopyCustomFieldsSettingModel()
        {
            IsBusy = new ReactivePropertySlim<string>().AddTo(disposables);
            SelectedCustomFields = new ObservableCollection<MyCustomField>();
            AllCustomFields = new List<MyCustomField>();
        }

        public async Task SetupAsync(Managers.RedmineManager r)
        {
            try
            {
                IsBusy.Value = Resources.SettingsMsgNowGettingData;

                var customFields = CacheManager.Default.CustomFields.Value;

                AllCustomFields.Clear();

                // 自分が担当しているプロジェクトで有効になっているカスタムフィールドのみを対象とする
                var enableCfIds = r.GetMyProjects().Where(p => p.CustomFields != null).SelectMany(p => p.CustomFields.Select(a => a.Id)).Distinct().ToList();
                var enableCfs = customFields.Where(c => c.IsIssueType() && enableCfIds.Contains(c.Id)).Select(a => new MyCustomField(a)).ToList();
                AllCustomFields.AddRange(enableCfs);

                var notExists = SelectedCustomFields.Where(sc => !AllCustomFields.Any(c => c.Id == sc.Id)).ToList();
                foreach(var i in notExists)
                {
                    SelectedCustomFields.Remove(i);
                }
            }
            finally
            {
                IsBusy.Value = null;
            }
        }

        public List<IssueCustomField> GetCopiedCustomFields(MyIssue parent)
        {
            if (parent.RawIssue.CustomFields == null)
                return new List<IssueCustomField>();

            return parent.RawIssue.CustomFields.Where(c => SelectedCustomFields.Any(sc => sc.Id == c.Id)).ToList();
        }

        public List<string> GetCopiedCustomFieldQuries(MyIssue parent)
        {
            var result = new List<string>();
            foreach (var cf in GetCopiedCustomFields(parent))
            {
                if (cf.Multiple)
                {
                    foreach (var v in cf.Values)
                    {
                        result.Add($"issue[custom_field_values][{cf.Id}][]={HttpUtility.UrlEncode(v.Info)}");
                    }
                }
                else
                {
                    result.Add($"issue[custom_field_values][{cf.Id}]={HttpUtility.UrlEncode(cf.Values[0].Info)}");
                }
            }
            return result;
        }
    }
}
