using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.CustomFields
{
    public class OpenCustomFieldsViewModel : CustomFieldsViewModelBase
    {
        [Obsolete("For Serialize")]
        public OpenCustomFieldsViewModel() : base()
        {
        }

        public OpenCustomFieldsViewModel(CreateTicketViewModel parent, CreateTicketViewModel previous)
            : base(parent, previous, Resources.SettingsReviOpenTicket)
        {
        }

        protected override CustomFieldsViewModelBase getPrevious(CreateTicketViewModel previous)
        {
            return previous.OpenCustomFields;
        }

        protected override MyTracker getTracker(SettingsModel settings)
        {
            return settings.CreateTicket.OpenTracker;
        }
    }
}
