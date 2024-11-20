using LibRedminePower.Extentions;
using LibRedminePower.Models;
using LibRedminePower.ViewModels.Bases;
using NetOffice.OutlookApi.Enums;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Common;
using RedmineTimePuncher.Models.CreateTicket.Work;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Work
{
    public class RequestTicketsViewModel
        : RequestTicketsViewModelBase<AssigneesViewModel, PeriodViewModel, RequestTicketsModel, TargetTicketModel, PeriodModel>
    {
        public RequestTicketsViewModel(RequestTicketsModel requests, TargetTicketModel target) : base(requests)
        {
            Assignee = new AssigneesViewModel(requests.Assignees, target).AddTo(disposables);
            Period = new PeriodViewModel(requests.Period).AddTo(disposables);
        }
    }
}
