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

        public IssueProperty SortBy { get; set; } = IssueProperty.NOT_SPECIFIED;
        public ObservableCollection<IssueProperty> CanSortByProperties { get; set; }
        public bool IsDESC { get; set; }

        public IssueProperty GroupBy { get; set; } = IssueProperty.NOT_SPECIFIED;
        public List<IssueProperty> CanGroupByProperties { get; set; }

        public ReviewIssueListSettingModel()
        {
            IsBusy = new ReactivePropertySlim<string>().AddTo(disposables);
            SelectedProperties = new ObservableCollection<IssueProperty>();
            AllProperties = new List<IssueProperty>();
            CanSortByProperties = new ObservableCollection<IssueProperty>();
            CanGroupByProperties = new List<IssueProperty>();
        }

        private CompositeDisposable myDisposables;

        public void Setup()
        {
            try
            {
                IsBusy.Value = Resources.SettingsMsgNowGettingData;

                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable().AddTo(disposables);

                AllProperties.Clear();
                AllProperties.Add(new IssueProperty(IssuePropertyType.Status));
                AllProperties.Add(new IssueProperty(IssuePropertyType.Priority));
                AllProperties.Add(new IssueProperty(IssuePropertyType.Subject));
                AllProperties.Add(new IssueProperty(IssuePropertyType.AssignedTo));
                AllProperties.Add(new IssueProperty(IssuePropertyType.FixedVersion));
                AllProperties.Add(new IssueProperty(IssuePropertyType.Updated));
                AllProperties.Add(new IssueProperty(IssuePropertyType.Author));
                AllProperties.Add(new IssueProperty(IssuePropertyType.Category));
                AllProperties.Add(new IssueProperty(IssuePropertyType.StartDate));
                AllProperties.Add(new IssueProperty(IssuePropertyType.DueDate));
                AllProperties.Add(new IssueProperty(IssuePropertyType.DoneRatio));
                AllProperties.AddRange(CacheManager.Default.TmpMyCustomFields.Select(a => new IssueProperty(a)));

                var notExists = SelectedProperties.Where(p => !AllProperties.Contains(p)).ToList();
                foreach (var i in notExists)
                {
                    SelectedProperties.Remove(i);
                }

                // 現在選択されている項目のみを並び替えの選択肢として表示する
                SelectedProperties.CollectionChangedAsObservable().StartWithDefault().SubscribeWithErr(_ =>
                {
                    // CanSortByProperties を Clear すると SortBy が null になるため現在値を保持しておく
                    var previous = SortBy;
                    CanSortByProperties.Clear();
                    CanSortByProperties.Add(IssueProperty.NOT_SPECIFIED);
                    foreach (var p in SelectedProperties)
                    {
                        CanSortByProperties.Add(p);
                    }
                    SortBy = CanSortByProperties.Contains(previous) ? previous : IssueProperty.NOT_SPECIFIED;
                }).AddTo(myDisposables);

                // グループ条件に設定できる項目は限られているため別途設定する
                CanGroupByProperties.Clear();
                CanGroupByProperties.Add(IssueProperty.NOT_SPECIFIED);
                CanGroupByProperties.Add(new IssueProperty(IssuePropertyType.Status));
                CanGroupByProperties.Add(new IssueProperty(IssuePropertyType.Priority));
                CanGroupByProperties.Add(new IssueProperty(IssuePropertyType.AssignedTo));
                CanGroupByProperties.Add(new IssueProperty(IssuePropertyType.FixedVersion));
                CanGroupByProperties.Add(new IssueProperty(IssuePropertyType.Updated));
                CanGroupByProperties.Add(new IssueProperty(IssuePropertyType.Author));
                CanGroupByProperties.Add(new IssueProperty(IssuePropertyType.Category));
                CanGroupByProperties.Add(new IssueProperty(IssuePropertyType.StartDate));
                CanGroupByProperties.Add(new IssueProperty(IssuePropertyType.DueDate));
                CanGroupByProperties.Add(new IssueProperty(IssuePropertyType.DoneRatio));
                CanGroupByProperties.AddRange(CacheManager.Default.TmpMyCustomFields.Where(c => c.CanGroupBy()).Select(a => new IssueProperty(a)));
                if (!CanGroupByProperties.Contains(GroupBy))
                    GroupBy = IssueProperty.NOT_SPECIFIED;
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
            if (SelectedProperties.Count > 0)
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
    }
}
