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
using NetOffice.OutlookApi.Enums;
using NetOffice.OutlookApi;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using LibRedminePower.Applications;
using RedmineTimePuncher.Models.Managers;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using LibRedminePower.Enums;
using RedmineTimePuncher.Extentions;
using System.Reactive.Disposables;
using LibRedminePower.ViewModels.Bases;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.Models.CreateTicket;
using RedmineTimePuncher.ViewModels.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields;

namespace RedmineTimePuncher.ViewModels.CreateTicket.Review
{
    public class DetectionProcessViewModel
        : SavableCustomFieldViewModelBase<ReviewDetectionProcessSettingModel, MyCustomField, MyCustomFieldPossibleValue, TargetTicketModel>
    {
        public DetectionProcessViewModel(TargetTicketModel model)
            : base(Resources.ReviewTargetProcess, model, m => m.Process, s => s.CreateTicket.DetectionProcess)
        {
            IsValid = Setting.CombineLatest(SelectedValue, (s, v) => s.IsEnabled ? validate() : null)
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public void ApplyTemplate(MyCustomFieldPossibleValue template)
        {
            if (Setting.Value.IsEnabled)
            {
                var process = PossibleValues.Value.FirstOrDefault(v => v.Equals(template));
                if (process != null)
                {
                    SelectedValue.Value = process;
                }
            }
        }

        public string CreatePrgForTicket()
        {
            return Setting.Value.IsEnabled ? createPrgForTicket(SelectedValue.Value.Label) : "";
        }
    }
}
