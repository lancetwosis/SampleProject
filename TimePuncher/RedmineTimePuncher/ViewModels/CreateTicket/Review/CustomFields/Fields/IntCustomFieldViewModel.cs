using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Fields
{
    public class IntCustomFieldViewModel : CustomFieldViewModelBase
    {
        public IntCustomFieldViewModel(CustomFieldValueModel cf) : base(cf)
        {
        }

        public override string Validate()
        {
            var result = base.Validate();
            if (result != null)
                return result;

            if (!string.IsNullOrEmpty(Model.Value) &&
                (!int.TryParse(Model.Value, out var _) || Model.Value.Contains(".")))
            {
                return Resources.ReviewCfErrMsgEnterInteger;
            }

            return null;
        }
    }
}
