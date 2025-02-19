using LibRedminePower;
using LibRedminePower.ViewModels;
using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using LibRedminePower.Applications;
using RedmineTimePuncher.Models.Managers;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using LibRedminePower.Enums;
using RedmineTimePuncher.Extentions;
using System.Reactive.Disposables;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields;
using RedmineTimePuncher.ViewModels.CreateTicket.Review.CustomFields.Bases;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.CreateTicket;
using ObservableCollectionSync;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.ViewModels.CreateTicket.Common;
using RedmineTimePuncher.Models.CreateTicket.Common;
using LibRedminePower.ViewModels.Bases;
using RedmineTimePuncher.Models.CreateTicket.Enums;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.Templates
{
    public class TemplateChildCommand : ChildCommand
    {
        private TemplateViewModel template { get; }
        public TemplateChildCommand(TemplateViewModel template,
                                    ReadOnlyReactivePropertySlim<ReviewViewModel> selectedReview)
            : base("", new ReactivePropertySlim<string>(), () =>
            {
                var selected = selectedReview.Value.Model.Target.Ticket;
                var temp = template.Model.Target.Ticket;
                if (selected == null)
                {
                    var r = MessageBoxHelper.ConfirmWarningOkCancel(Resources.ReviewTemplateMsgConfirmTicket);
                    if (!r.HasValue || r.Value == false)
                        return;
                }
                else if (temp.Project.Id != selected.Project.Id)
                {
                    var msg = string.Format(Resources.ReviewTemplateMsgConfirmProject, temp.Project.Name, selected.Project.Name);
                    var r = MessageBoxHelper.ConfirmWarningOkCancel(msg);
                    if (!r.HasValue || r.Value == false)
                        return;
                }

                TraceHelper.TrackCommand(nameof(ReviewsViewModel.ApplyTemplateCommand));
                selectedReview.Value.ApplyTemplate(template.Model);
            })
        {
            this.template = template;
        }

        public override RadMenuItem ToRadMenuItem()
        {
            var menu = base.ToRadMenuItem();
            menu.Header = template;
            menu.HeaderTemplate = (DataTemplate)Application.Current.Resources["TemplateMenuHeader"];
            return menu;
        }
    }
}
