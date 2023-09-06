using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize
{
    public class ResultModel : LibRedminePower.Models.Bases.ModelBase
    {
        public bool HasValue { get; set; }
        public bool IsEdited { get; set; }
        public DateTime CreateAt { get; set; }
        public string FileName { get; set; }

        public TicketFiltersModel Filters { get; set; } = new TicketFiltersModel();
        public ChartSettingsModel ChartSettings { get; set; } = new ChartSettingsModel();

        public List<Project> Projects { get; set; } = new List<Project>();
        public List<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
        public List<TicketModel> Tickets { get; set; } = new List<TicketModel>();
        public List<CategorySettingModel> Categories { get; set; } = new List<CategorySettingModel>();

        public ResultModel()
        {
        }
    }
}
