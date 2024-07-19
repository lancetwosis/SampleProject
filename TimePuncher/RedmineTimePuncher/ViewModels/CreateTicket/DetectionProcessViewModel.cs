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

namespace RedmineTimePuncher.ViewModels.CreateTicket
{

    public class DetectionProcessViewModel : CustomFieldViewModelBase<ReviewDetectionProcessSettingModel, MyCustomField, MyCustomFieldPossibleValue>
    {
        public DetectionProcessViewModel() : base(Resources.ReviewTargetProcess)
        {
        }

        public void Setup(SettingsModel settings)
        {
            Model = settings.CreateTicket.DetectionProcess;
        }

        public string CreatePrgForTicket()
        {
            return Model.IsEnabled ? createPrgForTicket(Model.Value.Label) : "";
        }
    }
}
