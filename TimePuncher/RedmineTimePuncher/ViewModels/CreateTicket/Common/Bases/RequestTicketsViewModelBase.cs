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
using RedmineTimePuncher.Models.CreateTicket.Common.Bases;
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

namespace RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases
{
    public abstract class RequestTicketsViewModelBase<TAssignee, TPeriod, TRequestsModel, TTargetModel, TPeriodModel> : ViewModelBase
        where TAssignee : AssigneesViewModelBase<TTargetModel>
        where TPeriod : IRequestPeriod
        where TRequestsModel : RequestTicketsModelBase<TPeriodModel>
        where TTargetModel : TargetTicketModelBase
        where TPeriodModel : PeriodModelBase
    {
        public TAssignee Assignee { get; set; }
        public TPeriod Period { get; set; }

        public ReactivePropertySlim<string> Title { get; set; }
        public ReactivePropertySlim<string> Description { get; set; }

        protected RequestTicketsViewModelBase(TRequestsModel model)
        {
            Title = model.ToReactivePropertySlimAsSynchronized(m => m.OpenTicketTitle).AddTo(disposables);
            Description = model.ToReactivePropertySlimAsSynchronized(m => m.Description).AddTo(disposables);
        }

        public virtual void Clear()
        {
            Title.Value = "";
            Period.Clear();
            Description.Value = "";
        }
    }
}
