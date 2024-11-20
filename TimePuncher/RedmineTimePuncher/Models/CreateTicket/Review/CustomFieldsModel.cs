using LibRedminePower.Enums;
using LibRedminePower.Models;
using LibRedminePower.Models.Bases;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Review
{
    public class CustomFieldsModel : ModelBaseSlim
    {
        public ObservableCollection<CustomFieldValueModel> Open { get; set; } = new ObservableCollection<CustomFieldValueModel>();
        public ObservableCollection<CustomFieldValueModel> Request { get; set; } = new ObservableCollection<CustomFieldValueModel>();
        public ObservableCollection<CustomFieldValueModel> Point { get; set; } = new ObservableCollection<CustomFieldValueModel>();

        public CustomFieldsModel()
        {
        }
    }
}