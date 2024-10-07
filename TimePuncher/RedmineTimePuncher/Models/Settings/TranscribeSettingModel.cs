using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class TranscribeSettingModel : Bases.SettingsModelBase<TranscribeSettingModel>
    {
        public static MyCustomFieldPossibleValue NOT_SPECIFIED_PROCESS = new MyCustomFieldPossibleValue()
        {
            IsDefault = true,
            Label = LibRedminePower.Properties.Resources.SettingsNotSpecified,
            Value = "-1",
        };

        public bool IsEnabled { get; set; }

        public ObservableCollection<TranscribeSettingItemModel> Items { get; set; } = new ObservableCollection<TranscribeSettingItemModel>();

        public TranscribeSettingModel()
        { }
    }
}
