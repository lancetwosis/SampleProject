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
using RedmineTimePuncher.ViewModels.Settings.CreateTicket.CustomFields.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings.CreateTicket.CustomFields
{
    public class DetectionProcessSettingViewModel : ListCustomFieldSettingViewModelBase<MyCustomField, MyCustomFieldPossibleValue>
    {
        public DetectionProcessSettingViewModel(CustomFieldSettingModelBase model) : base(model, true)
        {
        }
    }
}
