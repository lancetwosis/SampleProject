using AutoMapper;
using LibRedminePower.Enums;
using LibRedminePower.Extentions;
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
using System.Reactive.Disposables;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.CreateTicket
{
    public class ReviewIssueListSettingModel : Bases.SettingsModelBase<ReviewIssueListSettingModel>
    {
        public bool ShowDescription { get; set; } = true;
        public bool ShowLastNote { get; set; } = true;
        public ObservableCollection<IssueProperty> SelectedProperties { get; set; } = new ObservableCollection<IssueProperty>();

        public IssueProperty SortBy { get; set; } = IssueProperty.NOT_SPECIFIED;
        public bool IsDESC { get; set; }

        public IssueProperty GroupBy { get; set; } = IssueProperty.NOT_SPECIFIED;

        public ReviewIssueListSettingModel()
        { }

        public string CreateShowAllPointIssuesUrl(Issue parent, int trackerId)
        {
            var sb = new StringBuilder();
            sb.Append(Managers.RedmineManager.Default.Value.GetIssuesUrl(parent.Project.Id));
            sb.Append($"?utf8=%E2%9C%93&set_filter=1");

            if (!SortBy.Equals(IssueProperty.NOT_SPECIFIED))
            {
                if (IsDESC)
                    sb.Append($"&sort={SortBy.Key}:desc");
                else
                    sb.Append($"&sort={SortBy.Key}");
            }
            else
            {
                sb.Append($"&sort=id:desc");
            }

            sb.Append($"&f[]=parent_id&op[parent_id]=~&v[parent_id][]={parent.Id}");
            sb.Append($"&f[]=tracker_id&op[tracker_id]==&v[tracker_id][]={trackerId}");

            sb.Append($"&f[]=");
            if (SelectedProperties.IsNotEmpty())
            {
                foreach (var prop in SelectedProperties)
                {
                    sb.Append($"&c[]={prop.Key}");
                }
            }
            else
            {
                // 一つも設定されていなかった場合、チケット一覧のデフォルトの項目を表示する
                sb.Append($"&c[]={RedmineKeys.STATUS}");
                sb.Append($"&c[]={RedmineKeys.PRIORITY}");
                sb.Append($"&c[]={RedmineKeys.SUBJECT}");
                sb.Append($"&c[]={RedmineKeys.ASSIGNED_TO}");
                sb.Append($"&c[]={RedmineKeys.FIXED_VERSION}");
                sb.Append($"&c[]={RedmineKeys.UPDATED_ON}");
            }

            if (!GroupBy.Equals(IssueProperty.NOT_SPECIFIED))
                sb.Append($"&group_by={GroupBy.Key}");
            else
                sb.Append($"&group_by=");

            if (ShowDescription)
                sb.Append($"&c[]=description");
            if (ShowLastNote)
                sb.Append($"&c[]=last_notes");

            return sb.ToString();
        }

        public override void SetupConfigure(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ReviewIssueListSettingModel, ReviewIssueListSettingModel>()
                .AfterMap((src, dest) =>
                {
                    // AutoMapperは、コレクション型プロパティでは、既存のインスタンスをそのまま利用して上書きコピーを行うため、
                    // プロパティの参照が変わらない限り PropertyChanged イベントは発行されない。
                    // 新しいインスタンスに置き換えることで、PropertyChanged イベントを発行させる
                    dest.SelectedProperties = new ObservableCollection<IssueProperty>(src.SelectedProperties);
                });
        }
    }
}
