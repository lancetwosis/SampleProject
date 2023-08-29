using AutoMapper;
using LibRedminePower.Extentions;
using LibRedminePower.Logging;
using LibRedminePower.ViewModels;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.Views.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class PersonHourReportSettingsViewModel : Bases.SettingsViewModelBase<PersonHourReportSettingsModel>
    {
        public EditableGridViewModel<PersonHourReportSettingModel> Items { get; set; }

        public PersonHourReportSettingsViewModel(PersonHourReportSettingsModel model) : base(model)
        {
            Items = new EditableGridViewModel<PersonHourReportSettingModel>(model.Items).AddTo(disposables);
        }
    }
}