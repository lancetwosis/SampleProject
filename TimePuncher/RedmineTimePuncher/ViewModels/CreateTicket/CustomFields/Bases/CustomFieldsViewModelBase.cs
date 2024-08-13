using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.CreateTicket.CustomFields.Fields;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.CustomFields.Bases
{
    public class CustomFieldsViewModelBase : ViewModelBase
    {
        public string TicketType { get; set; }
        public ObservableCollection<CustomFieldViewModelBase> Fields { get; set; }

        public CustomFieldsViewModelBase()
        {
            Fields = new ObservableCollection<CustomFieldViewModelBase>();
        }

        private CustomFieldsViewModelBase prev;
        public CustomFieldsViewModelBase(CreateTicketViewModel parent, CreateTicketViewModel previous, string ticketType) : this()
        {
            if (previous != null)
                this.prev = getPrevious(previous);

            TicketType = ticketType;

            parent.Parent.Redmine.CombineLatest(CacheManager.Default.Updated,
                parent.Parent.Settings.ObserveProperty(a => a.CreateTicket),
                parent.Parent.Settings.ObserveProperty(a => a.ReviewCopyCustomFields),
                parent.ObserveProperty(a => a.Ticket).Pairwise().Where(p =>
                {
                    // チケットのプロジェクトが変わった場合だけ更新する
                    return p.OldItem == null || p.NewItem == null || p.OldItem.Project.Id != p.NewItem.Project.Id;
                }),
                (r, _1, _2, _3, p) => (Redmine: r, Ticket: p?.NewItem)).SubscribeWithErr(p =>
                {
                    update(p.Redmine, p.Ticket, parent.Parent.Settings);
                }).AddTo(disposables);
        }

        protected virtual CustomFieldsViewModelBase getPrevious(CreateTicketViewModel parent)
        {
            throw new NotImplementedException();
        }

        protected virtual MyTracker getTracker(SettingsModel settings)
        {
            throw new NotImplementedException();
        }

        private CompositeDisposable myDisposables;
        private void update(Models.Managers.RedmineManager redmine, MyIssue ticket, SettingsModel settings)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            if (redmine == null || ticket == null)
            {
                 Fields.Clear();
                return;
            }

            var proj = redmine.GetProject(ticket.Project.Id.ToString());
            var settingFieldIds = settings.CreateTicket.GetSettingCustomFieldIds();
            var fields = CacheManager.Default.CustomFields
                .Where(cf =>
                     // レビューの設定で保存対象のカスタムフィールドに指定されていない
                     !settingFieldIds.Contains(cf.Id) &&
                     // 選択されたチケットのプロジェクトで有効
                     proj.CustomFields.Any(a => a.Id == cf.Id) &&
                     // 開催チケットなどに割り当てられたトラッカーで有効
                     cf.Trackers.Any(t => t.Id == getTracker(settings).Id))
                .Select(cf =>
                {
                    // attachment （ファイル）以外の型に対応
                    switch (cf.FieldFormat)
                    {
                        case RedmineKeys.CF_INT:
                            return (CustomFieldViewModelBase)new IntCustomFieldViewModel(cf).AddTo(myDisposables);
                        case RedmineKeys.CF_STRING:
                        case RedmineKeys.CF_LINK:
                            return new StringCustomFieldViewModel(cf).AddTo(myDisposables);
                        case RedmineKeys.CF_LIST:
                            return new ListCustomFieldViewModel(cf).AddTo(myDisposables);
                        case RedmineKeys.CF_FLOAT:
                            return new FloatCustomFieldViewModel(cf).AddTo(myDisposables);
                        case RedmineKeys.CF_USER:
                            return new UserCustomFieldViewModel(cf, ticket).AddTo(myDisposables);
                        case RedmineKeys.CF_VERSION:
                            return new VersionCustomFieldViewModel(cf, ticket).AddTo(myDisposables);
                        case RedmineKeys.CF_BOOL:
                            return new BoolCustomFieldViewModel(cf).AddTo(myDisposables);
                        case RedmineKeys.CF_TEXT:
                            return new TextCustomFieldViewModel(cf).AddTo(myDisposables);
                        case RedmineKeys.CF_DATE:
                            return new DateCustomFieldViewModel(cf).AddTo(myDisposables);
                        case RedmineKeys.CF_ENUMERATION:
                            return new KeyValueCustomFieldViewModel(cf).AddTo(myDisposables);
                        default:
                            return null;
                    }
                }).Where(a => a != null).ToList();

            Fields.Clear();
            foreach (var f in fields)
            {
                Fields.Add(f);
            }

            // 「カスタムフィールドのコピー」が設定されていたら値をチケットからコピーする
            foreach (var f in Fields)
            {
                var copied = settings.ReviewCopyCustomFields.SelectedCustomFields.FirstOrDefault(c => c.Id == f.CustomField.Id);
                if (copied != null)
                {
                    var srcCf = ticket.RawIssue.CustomFields.FirstOrDefault(a => a.Id == copied.Id);
                    if (srcCf != null)
                    {
                        f.SetValue(srcCf);
                    }
                }
            }

            // 前回の値を反映
            if (prev != null)
            {
                foreach (var f in Fields)
                {
                    var pf = prev.Fields.FirstOrDefault(pre => pre.CustomField.Id == f.CustomField.Id);
                    if (pf != null)
                        f.SetValue(pf.Value);
                }
                prev = null;
            }
        }

        public List<IssueCustomField> GetIssueCustomFields()
        {
            return Fields.Where(cf => cf.NeedsApply()).Select(cf => cf.ToIssueCustomField()).ToList();
        }

        public List<string> GetIssueCustomFieldQuries()
        {
            return Fields.Where(cf => cf.NeedsApply()).SelectMany(cf => cf.ToQueryStrings()).ToList();
        }

        public string Validate()
        {
            var invalids = Fields.Select(c => (ErrMsg: c.Validate(), Name: c.CustomField.Name)).Where(p => p.ErrMsg != null).ToList();
            if (invalids.Count == 0)
                return null;

            var sb = new StringBuilder();
            sb.AppendLine(TicketType);
            foreach (var p in invalids)
            {
                sb.AppendLine($"  {p.Name}: {p.ErrMsg}");
            }
            return sb.ToString();
        }
    }
}
