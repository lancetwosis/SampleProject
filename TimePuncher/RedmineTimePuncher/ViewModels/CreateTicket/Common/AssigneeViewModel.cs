using LibRedminePower;
using LibRedminePower.Extentions;
using LibRedminePower.Models;
using LibRedminePower.ViewModels.Bases;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Common;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Common
{
    public class AssigneeViewModel : ViewModelBase
    {
        public ReactivePropertySlim<bool> IsRequired { get; set; }

        public AssigneeModel Model { get; set; }

        public AssigneeViewModel(IdentifiableName user) : this(new AssigneeModel(user))
        {
        }

        public AssigneeViewModel(AssigneeModel user)
        {
            Model = user;
            IsRequired = Model.ToReactivePropertySlimAsSynchronized(m => m.IsRequired).AddTo(disposables);
        }

        public int GetRecipientType()
        {
            if (SettingsModel.Default.CreateTicket.IsRequired.IsEnabled)
                return IsRequired.Value ? (int)OlMeetingRecipientType.olRequired : (int)OlMeetingRecipientType.olOptional;
            else
                return (int)OlMeetingRecipientType.olRequired;
        }

        public override string ToString()
        {
            return Model.Name;
        }

        public override bool Equals(object obj)
        {
            return this.JsonEquals(obj);
        }

        public override int GetHashCode()
        {
            int hashCode = -905171734;
            hashCode = hashCode * -1521134295 + this.GetJsonHashcode();
            return hashCode;
        }
    }
}
