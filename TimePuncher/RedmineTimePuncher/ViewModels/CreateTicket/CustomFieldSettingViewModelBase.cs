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
    public class CustomFieldSettingViewModelBase<TModel, TField, TValue> : ViewModelBase
        where TModel : CustomFieldSettingModelBase<TField, TValue>
        where TField : MyCustomField<TValue> where TValue : MyCustomFieldPossibleValue
    {
        public TModel Model { get; set; }

        private string header { get; set; }

        [Obsolete("For Serialize", false)]
        public CustomFieldSettingViewModelBase()
        {
        }

        public CustomFieldSettingViewModelBase(string header)
        {
            this.header = header;
        }

        public void SetPreviousModel(TModel previous)
        {
            if (previous == null || previous.Value == null)
                return;

            var preValue = Model.PossibleValues.FirstOrDefault(v => v != null && v.Value == previous.Value.Value);
            if (preValue != null)
                Model.Value = preValue;
        }

        public string CreatePrgForOutlook()
        {
            return Model.IsEnabled ? MarkupLangType.None.CreateParagraph(header, Model.Value.Label) : "";
        }

        protected string createPrgForTicket(string label)
        {
            return CacheManager.Default.MarkupLang.CreateParagraph(header, label);
        }
    }
}
