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
    public class VersionCustomFieldViewModel : IdNameCustomFieldViewModelBase<MyVersion>
    {
        public VersionCustomFieldViewModel(CustomFieldValueModel cf, MyIssue ticket) : base(cf, ticket)
        {
        }

        protected override List<MyVersion> getValues(MyIssue ticket)
        {
            return CacheManager.Default.ProjectVersions[ticket.Project.Id].Select(v => new MyVersion(v)).ToList();
        }

        protected override MyVersion getNotSpecified() => MyVersion.NOT_SPECIFIED;
    }
}
