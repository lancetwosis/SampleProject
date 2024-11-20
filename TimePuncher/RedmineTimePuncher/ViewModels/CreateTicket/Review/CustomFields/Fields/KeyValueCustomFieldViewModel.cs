using LibRedminePower.Extentions;
using LibRedminePower.Models;
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
    public class KeyValueCustomFieldViewModel : IdNameCustomFieldViewModelBase<IdName>
    {
        public KeyValueCustomFieldViewModel(CustomFieldValueModel cf) : base(cf, null)
        {
        }

        protected override List<IdName> getValues(MyIssue _)
        {
            return Model.CustomField.PossibleValues.Select(v => new IdName() { Id = int.Parse(v.Value), Name = v.Label }).ToList();
        }

        protected override IdName getNotSpecified() => new IdName() { Id = IdName.INVALID_ID, Name = "" };
    }
}
