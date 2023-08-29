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
    public class TicketModel : LibRedminePower.Models.Bases.ModelBase
    {
        public bool IsEnabled { get; set; } = true;
        public bool IsExpanded { get; set; } = true;
        public Issue RawIssue { get; set; }

        public TicketModel()
        {
        }

        public TicketModel(Issue issue)
        {
            RawIssue = issue;
        }
    }
}
