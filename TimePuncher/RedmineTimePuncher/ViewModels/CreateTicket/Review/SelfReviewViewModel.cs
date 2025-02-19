using LibRedminePower.Extentions;
using LibRedminePower.Models;
using LibRedminePower.ViewModels.Bases;
using NetOffice.OutlookApi.Enums;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.CreateTicket;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
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

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review
{
    public class SelfReviewViewModel : ViewModelBase
    {
        public ReactivePropertySlim<bool> IsEnabled { get; set; }

        public ReadOnlyReactivePropertySlim<string> TicketId { get; set; }
        public ReadOnlyReactivePropertySlim<string> TicketUrl { get; set; }

        public SelfReviewModel Model { get; set; }

        public SelfReviewViewModel(SelfReviewModel model)
        {
            Model = model;

            IsEnabled = model.ToReactivePropertySlimAsSynchronized(m => m.IsEnabled).AddTo(disposables);

            TicketId = model.ObserveProperty(m => m.SelfTicket).Select(t => $"#{t?.Id}").ToReadOnlyReactivePropertySlim().AddTo(disposables);
            TicketUrl = model.ObserveProperty(m => m.SelfTicket).Select(t => t?.Url).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public void ApplyTemplate(SelfReviewModel template)
        {
            IsEnabled.Value = template.IsEnabled;
        }
    }
}
