using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class CustomFieldSettingViewModel<TField, TValue> : ViewModelBase
        where TField : MyCustomField<TValue> where TValue : MyCustomFieldPossibleValue
    {
        public ReactivePropertySlim<bool> IsEnabled { get; set; }

        public ReactivePropertySlim<bool> NeedsSaveToCustomField { get; set; }
        public ReadOnlyReactivePropertySlim<List<TField>> PossibleCustomFields { get; set; }
        public ReactivePropertySlim<TField> CustomField { get; set; }
        public ReadOnlyReactivePropertySlim<string> NoCustomFieldErrMsg { get; set; }
        public string HelpMsg { get; set; }


        public CustomFieldSettingViewModel(CustomFieldSettingModelBase<TField, TValue> model)
        {
            IsEnabled = model.ToReactivePropertySlimAsSynchronized(m => m.IsEnabled).AddTo(disposables);

            NeedsSaveToCustomField = model.ToReactivePropertySlimAsSynchronized(m => m.NeedsSaveToCustomField).AddTo(disposables);
            PossibleCustomFields = model.ObserveProperty(a => a.PossibleCustomFields).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            CustomField = model.ToReactivePropertySlimAsSynchronized(m => m.CustomField).AddTo(disposables);
            NoCustomFieldErrMsg =
                PossibleCustomFields.Select(a => a.Any() ? null : string.Format(Resources.SettingsReviErrMsgNoCustomFields, model.Format.GetDescription()))
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);

            switch (model.Format)
            {
                case Enums.CustomFieldFormat.Bool:
                    HelpMsg = string.Format(Resources.SettingsReviMsgBoolCustomFieldHelp, model.PossibleValues.First(v => v.Value == MyCustomFieldPossibleValue.YES).Label);
                    break;
                case Enums.CustomFieldFormat.List:
                case Enums.CustomFieldFormat.User:
                    HelpMsg = string.Format(Resources.SettingsReviMsgCustomFieldHelp, model.Format.GetDescription());
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class CustomFieldSettingViewModel : CustomFieldSettingViewModel<MyCustomField, MyCustomFieldPossibleValue>
    {
        public CustomFieldSettingViewModel(CustomFieldSettingModelBase model) : base(model)
        {
        }
    }
}
