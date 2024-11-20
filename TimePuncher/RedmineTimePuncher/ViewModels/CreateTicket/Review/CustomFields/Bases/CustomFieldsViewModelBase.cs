using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Fields;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Bases
{
    public abstract class CustomFieldsViewModelBase : ViewModelBase
    {
        public string TicketType { get; set; }
        public ObservableCollection<CustomFieldViewModelBase> Fields { get; set; }

        public ReadOnlyReactivePropertySlim<string> IsValid { get; set; }

        private ObservableCollection<CustomFieldValueModel> customFieldValues { get; set; }
        private List<CustomFieldValueModel> prev { get; set; }

        public CustomFieldsViewModelBase(string ticketType, ObservableCollection<CustomFieldValueModel> customFieldValues, TargetTicketModel target)
        {
            TicketType = ticketType;

            this.customFieldValues = customFieldValues;
            this.prev = customFieldValues.ToList();
            Fields = new ObservableCollectionSync<CustomFieldViewModelBase, CustomFieldValueModel>(customFieldValues,
                m =>
                {
                    // attachment （ファイル）以外の型に対応
                    switch (m.CustomField.FieldFormat)
                    {
                        case RedmineKeys.CF_INT:
                            return (CustomFieldViewModelBase)new IntCustomFieldViewModel(m).AddTo(disposables);
                        case RedmineKeys.CF_STRING:
                        case RedmineKeys.CF_LINK:
                            return new StringCustomFieldViewModel(m).AddTo(disposables);
                        case RedmineKeys.CF_LIST:
                            return new ListCustomFieldViewModel(m).AddTo(disposables);
                        case RedmineKeys.CF_FLOAT:
                            return new FloatCustomFieldViewModel(m).AddTo(disposables);
                        case RedmineKeys.CF_USER:
                            return new UserCustomFieldViewModel(m, target.Ticket).AddTo(disposables);
                        case RedmineKeys.CF_VERSION:
                            return new VersionCustomFieldViewModel(m, target.Ticket).AddTo(disposables);
                        case RedmineKeys.CF_BOOL:
                            return new BoolCustomFieldViewModel(m).AddTo(disposables);
                        case RedmineKeys.CF_TEXT:
                            return new TextCustomFieldViewModel(m).AddTo(disposables);
                        case RedmineKeys.CF_DATE:
                            return new DateCustomFieldViewModel(m).AddTo(disposables);
                        case RedmineKeys.CF_ENUMERATION:
                            return new KeyValueCustomFieldViewModel(m).AddTo(disposables);
                        default:
                            throw new NotSupportedException($"m.CustomField.FieldFormat が {m.CustomField.FieldFormat} はサポート対象外です。");
                    }
                },
                vm => vm?.Model).AddTo(disposables);

            IsValid = Fields.ObserveElementProperty(f => f.ErrorMessage.Value).Select(_ =>
            {
                var invalids = Fields.Select(c => (ErrMsg: c.Validate(), Name: c.Model.CustomField.Name)).Where(p => p.ErrMsg != null).ToList();
                if (invalids.IsEmpty())
                    return null;

                var sb = new StringBuilder();
                sb.AppendLine(TicketType);
                sb.Append(string.Join(Environment.NewLine, invalids.Select(p => $"  {p.Name}: {p.ErrMsg}")));
                return sb.ToString();
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            CacheManager.Default.Updated.CombineLatest(
                SettingsModel.Default.ObserveProperty(a => a.CreateTicket),
                SettingsModel.Default.ObserveProperty(a => a.ReviewCopyCustomFields),
                target.ObserveProperty(m => m.Ticket).StartWithDefault().Pairwise(),
                (_1, _2, _3, p) => p).SubscribeWithErr(p =>
                {
                    if (p.NewItem == null)
                        return;

                    // チケットのプロジェクトが変わった場合だけカスタムフィールド全体を更新する
                    if (p.OldItem == null || p.OldItem.Project.Id != p.NewItem.Project.Id)
                    {
                        update(p.NewItem);
                    }

                    // 「カスタムフィールドのコピー」設定および「前回値」を適用する
                    updateValues(p.NewItem);
                }).AddTo(disposables);
        }

        protected virtual MyTracker getTracker()
        {
            throw new NotImplementedException();
        }

        private void update(MyIssue ticket)
        {
            var proj = CacheManager.Default.Projects.First(p => p.Id == ticket.Project.Id);
            var myTracker = getTracker();
            if (myTracker.Equals(MyTracker.USE_PARENT_TRACKER))
                myTracker = ticket.Tracker;

            if (!proj.Trackers.Any(t => t.Id == myTracker.Id))
            {
                Fields.Clear();
                return;
                // TODO 対応方針を決めて不要なら Resources からも削除すること #1916
                //throw new ApplicationException(string.Format(Resources.ReviewErrMsgInvalidTracker, TicketType, myTracker.Name, proj.Name));
            }

            var settingFieldIds = SettingsModel.Default.CreateTicket.GetSettingCustomFieldIds();
            var fields = CacheManager.Default.CustomFields
                .Where(cf =>
                     // レビューの設定で保存対象のカスタムフィールドに指定されていない、かつ、
                     !settingFieldIds.Contains(cf.Id) &&
                     // 選択されたチケットのプロジェクトで有効、かつ、
                     proj.CustomFields != null && proj.CustomFields.Any(a => a.Id == cf.Id) &&
                     // 作成するチケットのトラッカーで有効
                     cf.Trackers != null && cf.Trackers.Any(t => t.Id == myTracker.Id) &&
                     // attachment （ファイル）以外の型に対応
                     cf.FieldFormat != RedmineKeys.CF_ATTACHMENT)
                .Select(cf => new CustomFieldValueModel(cf)).ToList();

            this.customFieldValues.Clear();
            foreach (var f in fields)
            {
                this.customFieldValues.Add(f);
            }
        }

        private void updateValues(MyIssue ticket)
        {
            // 「カスタムフィールドのコピー」が設定されていたら値をチケットからコピーする
            foreach (var f in Fields)
            {
                var copied = SettingsModel.Default.ReviewCopyCustomFields.SelectedCustomFields.FirstOrDefault(c => c.Id == f.Model.CustomField.Id);
                if (copied != null)
                {
                    var srcCf = ticket.RawIssue.CustomFields.FirstOrDefault(a => a.Id == copied.Id);
                    if (srcCf != null)
                        f.SetValue(srcCf);
                    else
                        f.SetValue("");
                }
            }

            // 前回の値を反映
            SetPrevious(prev);
            prev = customFieldValues.ToList();
        }

        public void SetPrevious(IList<CustomFieldValueModel> previous)
        {
            if (previous != null)
            {
                foreach (var f in Fields)
                {
                    var pf = previous.FirstOrDefault(pre => pre.CustomField.Id == f.Model.CustomField.Id);
                    if (pf != null)
                        f.SetValue(pf.Value);
                }
            }
        }

        public void Clear()
        {
            Fields.Clear();
            this.prev = null;
        }

        public List<IssueCustomField> GetIssueCustomFields()
        {
            return Fields.Where(cf => cf.NeedsApply()).Select(cf => cf.ToIssueCustomField()).ToList();
        }

        public List<string> GetIssueCustomFieldQuries()
        {
            return Fields.Where(cf => cf.NeedsApply()).SelectMany(cf => cf.ToQueryStrings()).ToList();
        }
    }
}
