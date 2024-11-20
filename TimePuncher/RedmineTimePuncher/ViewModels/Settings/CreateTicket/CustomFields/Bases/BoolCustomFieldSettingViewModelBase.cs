using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields;
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
    public abstract class BoolCustomFieldSettingViewModelBase<TField, TValue> : CustomFieldSettingViewModelBase<TField, TValue>
        where TField : MyCustomField<TValue> where TValue : MyCustomFieldPossibleValue
    {
        protected BoolCustomFieldSettingViewModelBase(CustomFieldSettingModelBase<TField, TValue> model) : base(model)
        {
            switch (model)
            {
                case ReviewIsRequiredSettingModel ir:
                    HelpMsg = string.Format(Resources.SettingsReviMsgBoolCustomFieldHelp, ReviewIsRequiredSettingModel.REQUIRED.Label);
                    break;
                default:
                    throw new NotSupportedException($"model の型が {model.GetType().Name} はサポート対象外です。");
            }
        }
    }
}
