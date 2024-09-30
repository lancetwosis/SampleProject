using LibRedminePower.Extentions;
using LibRedminePower.Models;
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
    public class BoolCustomFieldViewModel : IdNameCustomFieldViewModelBase<IdName>
    {
        public BoolCustomFieldViewModel(CustomField cf) : base(cf, null)
        {
        }

        protected override List<IdName> getValues(MyIssue _)
        {
            return new List<IdName>()
            {
                new IdName() { Id = 1, Name = Resources.ReviewCfValueYes },
                new IdName() { Id = 0, Name = Resources.ReviewCfValueNo },
            };
        }

        protected override IdName getNotSpecified() => new IdName() { Id = IdName.INVALID_ID, Name = "" };
    }
}
