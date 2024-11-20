using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields
{
    public class OpenCustomFieldsViewModel : CustomFieldsViewModelBase
    {
        public OpenCustomFieldsViewModel(CustomFieldsModel customFields, TargetTicketModel target)
            : base(Resources.SettingsReviOpenTicket, customFields.Open, target)
        {
        }

        protected override MyTracker getTracker()
        {
            return SettingsModel.Default.CreateTicket.OpenTracker;
        }
    }
}
