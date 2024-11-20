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
    public abstract class ListCustomFieldSettingViewModelBase<TField, TValue> : CustomFieldSettingViewModelBase<TField, TValue>
        where TField : MyCustomField<TValue> where TValue : MyCustomFieldPossibleValue
    {
        protected ListCustomFieldSettingViewModelBase(CustomFieldSettingModelBase<TField, TValue> model, bool mustSave) : base(model)
        {
            if (mustSave)
            {
                NeedsSaveToCustomField.Value = true;
                PossibleCustomFields.Where(a => a != null).SubscribeWithErr(cfs =>
                {
                    // リスト形式のカスタムフィールドがない場合、機能自体を無効にする
                    if (cfs.IsEmpty())
                    {
                        IsEnabled.Value = false;
                    }
                }).AddTo(disposables);
            }

            HelpMsg = string.Format(Resources.SettingsReviMsgCustomFieldHelp, model.Format.GetDescription());
        }
    }
}
