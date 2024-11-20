using LibRedminePower;
using LibRedminePower.ViewModels;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using RedmineTimePuncher.Enums;
using NetOffice.OutlookApi.Enums;
using NetOffice.OutlookApi;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using LibRedminePower.Applications;
using RedmineTimePuncher.Models.Managers;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using LibRedminePower.Enums;
using RedmineTimePuncher.Extentions;
using System.Reactive.Disposables;
using LibRedminePower.ViewModels.Bases;
using RedmineTimePuncher.Models.Settings.Bases;
using System.Linq.Expressions;
using RedmineTimePuncher.Models.CreateTicket;
using LibRedminePower.Models.Bases;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields.Bases;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases
{
    public abstract class SavableCustomFieldViewModelBase<TSetting, TField, TValue, TModel> : ViewModelBase
        where TSetting : CustomFieldSettingModelBase<TField, TValue>
        where TField : MyCustomField<TValue>
        where TValue : MyCustomFieldPossibleValue
        where TModel : ModelBaseSlim
    {
        public ReactivePropertySlim<TValue> SelectedValue { get; set; }
        public ReadOnlyReactivePropertySlim<List<TValue>> PossibleValues { get; set; }

        public ReadOnlyReactivePropertySlim<string> IsValid { get; set; }

        public ReadOnlyReactivePropertySlim<TSetting> Setting { get; set; }

        private string settingName { get; set; }

        protected SavableCustomFieldViewModelBase(string settingName, TModel model,
            Expression<Func<TModel, TValue>> valueSelector,
            Expression<Func<SettingsModel, TSetting>> settingSelector)
        {
            this.settingName = settingName;

            SelectedValue = model.ToReactivePropertySlimAsSynchronized(valueSelector).AddTo(disposables);
            Setting = SettingsModel.Default.ObserveProperty(settingSelector).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            CacheManager.Default.Updated.SubscribeWithErr(_ => Setting.Value?.ApplyCustomField()).AddTo(disposables);

            PossibleValues = Setting.Select(s => s.GetPossibleValues()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            PossibleValues.SubscribeWithErr(vs =>
            {
                if (SelectedValue.Value == null)
                {
                    SelectedValue.Value = PossibleValues.Value?.FirstOrDefault(v => v.IsDefault);
                }
                else
                {
                    var selected = vs.FirstOrDefault(v => v.Equals(SelectedValue.Value));
                    if (selected == null)
                    {
                        SelectedValue.Value = PossibleValues.Value?.FirstOrDefault(v => v.IsDefault);
                    }
                }
            }).AddTo(disposables);
        }

        protected string validate()
        {
            if (!Setting.Value.IsEnabled)
                return null;

            return SelectedValue.Value != null ? null : string.Format(Resources.ReviewErrMsgSelectXXX, settingName);
        }

        public virtual void Clear()
        {
            SelectedValue.Value = null;
        }

        public string CreatePrgForOutlook()
        {
            return Setting.Value.IsEnabled ? MarkupLangType.None.CreateParagraph(settingName, SelectedValue.Value.Label) : "";
        }

        protected string createPrgForTicket(string label)
        {
            return CacheManager.Default.MarkupLang.CreateParagraph(settingName, label);
        }

        public string GetQueryString()
        {
            if (!Setting.Value.IsEnabled || !Setting.Value.NeedsSaveToCustomField || Setting.Value.CustomField == null)
                return null;
            else
                return Setting.Value.CustomField.CreateQueryString(SelectedValue.Value.Value);
        }

        public IssueCustomField GetIssueCustomField()
        {
            if (!Setting.Value.IsEnabled || !Setting.Value.NeedsSaveToCustomField || SelectedValue.Value == null)
                return null;

            return Setting.Value.CreateIssueCustomField(SelectedValue.Value);
        }
    }
}
