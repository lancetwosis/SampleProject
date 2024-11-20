using LibRedminePower.Extentions;
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
    public class CustomFieldsViewModel : ViewModelBase
    {
        public OpenCustomFieldsViewModel Open { get; set; }
        public RequestCustomFieldsViewModel Request { get; set; }
        public PointCustomFieldsViewModel Point { get; set; }

        public ReadOnlyReactivePropertySlim<bool> IsVisible { get; set; }
        public ReadOnlyReactivePropertySlim<string> IsValid { get; set; }

        public CustomFieldsViewModel(CustomFieldsModel customFields, TargetTicketModel target)
        {
            Open = new OpenCustomFieldsViewModel(customFields, target).AddTo(disposables);
            Request = new RequestCustomFieldsViewModel(customFields, target).AddTo(disposables);
            Point = new PointCustomFieldsViewModel(customFields, target).AddTo(disposables);

            IsVisible = new[]
            {
                Open.Fields.AnyAsObservable(),
                Request.Fields.AnyAsObservable(),
                Point.Fields.AnyAsObservable(),
            }.CombineLatest().Select(a => a.Any(b => b)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsValid = new[] { Open.IsValid, Request.IsValid }.CombineLatest().Select(es =>
           {
               var errs = es.Where(e => e != null).ToList();
               if (errs.IsEmpty())
                   return null;

               return string.Format(Resources.ReviewErrMsgSetCustomFields, string.Join($"{Environment.NewLine}{Environment.NewLine}", errs));
           }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public void Clear()
        {
            Open.Clear();
            Request.Clear();
            Point.Clear();
        }

        public void ApplyTemplate(CustomFieldsModel template)
        {
            Open.SetPrevious(template.Open);
            Request.SetPrevious(template.Request);
            Point.SetPrevious(template.Point);
        }
    }
}
