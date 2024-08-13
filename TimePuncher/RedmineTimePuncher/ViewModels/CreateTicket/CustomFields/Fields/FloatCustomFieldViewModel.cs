using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.CreateTicket.CustomFields.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RedmineTimePuncher.ViewModels.CreateTicket.CustomFields.Fields
{
    public class FloatCustomFieldViewModel : CustomFieldViewModelBase
    {
        public FloatCustomFieldViewModel(CustomField cf) : base(cf)
        {
        }

        public override string Validate()
        {
            var result = base.Validate();
            if (result != null)
                return result;

            if (!string.IsNullOrEmpty(Value) && !float.TryParse(Value, out var _))
            {
                return Resources.ReviewCfErrMsgEnterNumber;
            }

            return null;
        }
    }
}
