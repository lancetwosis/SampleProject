using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
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
    public class KeyValueCustomFieldViewModel : IdNameCustomFieldViewModelBase<IdName>
    {
        public KeyValueCustomFieldViewModel(CustomField cf) : base(cf, null)
        {
        }

        protected override List<IdName> getValues(MyIssue _)
        {
            return CustomField.PossibleValues.Select(v => new IdName() { Id = int.Parse(v.Value), Name = v.Label }).ToList();
        }

        protected override IdName getNotSpecified() => new IdName() { Id = IdName.INVALID_ID, Name = "" };
    }
}
