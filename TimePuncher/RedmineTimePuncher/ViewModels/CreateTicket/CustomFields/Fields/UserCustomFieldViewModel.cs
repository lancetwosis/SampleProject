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
    public class UserCustomFieldViewModel : IdNameCustomFieldViewModelBase<MyUser>
    {
        public UserCustomFieldViewModel(CustomField cf, MyIssue ticket) : base(cf, ticket)
        {
        }

        protected override List<MyUser> getValues(MyIssue ticket)
        {
            var memberShips = CacheManager.Default.ProjectMemberships[ticket.Project.Id];
            return CacheManager.Default.Users.Where(u => memberShips.Any(m => m.User.Id == u.Id)).ToList();
        }

        protected override MyUser getNotSpecified() => MyUser.NOT_SPECIFIED;
    }

}
