using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models.FileSettings;
using System.Collections.ObjectModel;
using LibRedminePower.ViewModels;
using LibRedminePower.Extentions;
using RedmineTableEditor.Models;
using ObservableCollectionSync;
using RedmineTableEditor.Models.TicketFields.Standard;
using System.Reactive.Disposables;
using LibRedminePower.Enums;
using RedmineTableEditor.Extentions;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public abstract class IssueSettingsViewModelBase : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public TwinListBoxViewModel<FieldViewModel> VisibleProps { get; set; }

        [Obsolete("Design Only", true)]
        public IssueSettingsViewModelBase() { }

        public IssueSettingsViewModelBase(IssueSettingsModelBase model, RedmineManager redmine, bool isSubIssues = false)
        {
            var allFields = redmine.Cache.Updated.Select(_ =>
            {
                var fields = Enum.GetValues(typeof(IssuePropertyType)).Cast<IssuePropertyType>()
                    .Where(a => a.IsPropertyColumn())
                    .OrderBy(a => a.ToPropertyColumnOrder())
                    .Select(a => new FieldViewModel(new FieldModel(a), null)).ToList();

                // 親チケットには Id と Subject を必ず入れたいので選択肢からは削除する
                if (!isSubIssues)
                {
                    fields = fields.Where(f => !f.Model.IsType(IssuePropertyType.Id) &&
                                               !f.Model.IsType(IssuePropertyType.Subject)).ToList();
                }

                fields.Add(new FieldViewModel(new FieldModel(MyIssuePropertyType.DiffEstimatedSpent), null));
                fields.Add(new FieldViewModel(new FieldModel(MyIssuePropertyType.MySpentHours), null));
                fields.Add(new FieldViewModel(new FieldModel(MyIssuePropertyType.ReplyCount), null));
                fields.Add(new FieldViewModel(new FieldModel(MyIssuePropertyType.RequiredDays), null));
                fields.Add(new FieldViewModel(new FieldModel(MyIssuePropertyType.DaysUntilCreated), null));

                // いずれかのプロジェクトで有効なカスタムフィールドをすべて表示する
                // http://133.242.159.37/issues/1588#note-6
                var cfFields = redmine.Cache.CustomFields.Where(cf => cf.IsEnabled() && cf.ToFieldFormat().IsSupported())
                                                         .Select(cf => new FieldViewModel(new FieldModel(cf.Id), cf))
                                                         .ToList();
                fields.AddRange(cfFields);

                return fields;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            CompositeDisposable myDisposables = null;
            allFields.ObserveOnUIDispatcher().SubscribeWithErr(_ =>
            {
                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable().AddTo(disposables);

                var invalidFields = model.Properties.Where(p => !allFields.Value.Any(f => f.Model.Equals(p))).ToList();
                foreach (var f in invalidFields)
                {
                    model.Properties.Remove(f);
                }

                var selectedFields = new ObservableCollectionSync<FieldViewModel, FieldModel>(model.Properties,
                    a => allFields.Value.SingleOrDefault(b => b.Model.Equals(a)), a => a.Model).AddTo(myDisposables);

                var defaultFields = getDefaultFields(selectedFields, allFields.Value);
                foreach (var f in defaultFields)
                {
                    selectedFields.Add(f);
                }

                VisibleProps = new TwinListBoxViewModel<FieldViewModel>(allFields.Value, selectedFields).AddTo(myDisposables);
            }).AddTo(disposables);
        }

        protected virtual List<FieldViewModel> getDefaultFields(ObservableCollectionSync<FieldViewModel, FieldModel> selectedFields, List<FieldViewModel> allFields)
        {
            return new List<FieldViewModel>();
        }
    }
}
