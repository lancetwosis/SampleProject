using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields.Bases;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings.CreateTicket.CustomFields.Bases
{
    public abstract class CustomFieldSettingViewModelBase<TField, TValue> : ViewModelBase
        where TField : MyCustomField<TValue> where TValue : MyCustomFieldPossibleValue
    {
        public ReactivePropertySlim<bool> IsEnabled { get; set; }

        public ReactivePropertySlim<bool> NeedsSaveToCustomField { get; set; }
        public ReadOnlyReactivePropertySlim<List<TField>> PossibleCustomFields { get; set; }
        public ReactivePropertySlim<TField> CustomField { get; set; }
        public ReadOnlyReactivePropertySlim<string> NoCustomFieldErrMsg { get; set; }
        public string HelpMsg { get; set; }

        public CustomFieldSettingViewModelBase(CustomFieldSettingModelBase<TField, TValue> model)
        {
            IsEnabled = model.ToReactivePropertySlimAsSynchronized(m => m.IsEnabled).AddTo(disposables);
            NeedsSaveToCustomField = model.ToReactivePropertySlimAsSynchronized(m => m.NeedsSaveToCustomField).AddTo(disposables);
            CustomField = model.ToReactivePropertySlimAsSynchronized(m => m.CustomField).AddTo(disposables);

            PossibleCustomFields = CacheTempManager.Default.CustomFields.Where(a => a != null).Select(cfs =>
            {
                return cfs.Where(cf => cf.IsIssueType() && cf.FieldFormat.ToCustomFieldFormat() == model.Format)
                          .Select(cf => model.CreateMyCustomField(cf))
                          .ToList();
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            PossibleCustomFields.Where(a =>  a != null).SubscribeWithErr(cfs =>
            {
                if (cfs.IsEmpty())
                {
                    CustomField.Value = null;
                    return;
                }

                if (CustomField.Value != null)
                {
                    var c = cfs.FirstOrDefault(a => a.Id == CustomField.Value.Id);
                    CustomField.Value = c != null ? c : cfs.First();
                }
                else
                {
                    CustomField.Value = cfs.First();
                }
            }).AddTo(disposables);
            NoCustomFieldErrMsg = PossibleCustomFields
                .Where(a => a != null)
                .Select(a => a.Any() ? null : string.Format(Resources.SettingsReviErrMsgNoCustomFields, model.Format.GetDescription()))
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
