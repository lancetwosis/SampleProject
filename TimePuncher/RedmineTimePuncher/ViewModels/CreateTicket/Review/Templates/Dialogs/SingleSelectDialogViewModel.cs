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
using RedmineTimePuncher.Views.CreateTicket.Review;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review.Templates.Dialogs
{

    public class SingleSelectDialogViewModel : TemplatesDialogViewModelBase
    {
        public ReactivePropertySlim<TemplateViewModel> SelectedTemplate { get; set; }

        public ReactiveCommandSlim<TemplatesDialog> OkCommand { get; private set; }
        public ReactiveCommandSlim<TemplatesDialog> CancelCommand { get; private set; }

        public SingleSelectDialogViewModel(ObservableCollection<TemplateViewModel> templates, string messeage)
            : base(templates, messeage)
        {
            SelectedTemplate = new ReactivePropertySlim<TemplateViewModel>().AddTo(disposables);
            OkCommand = SelectedTemplate.Select(t => t != null)
                .ToReactiveCommandSlim<TemplatesDialog>().WithSubscribe(dialog => clickOk(dialog)).AddTo(disposables);
            CancelCommand = new ReactiveCommandSlim<TemplatesDialog>().WithSubscribe(dialog => clickCancle(dialog)).AddTo(disposables);
        }
    }
}
