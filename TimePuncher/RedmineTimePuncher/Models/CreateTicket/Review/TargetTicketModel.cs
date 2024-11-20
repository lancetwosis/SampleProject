using LibRedminePower.Enums;
using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Review
{
    public class TargetTicketModel : TargetTicketModelBase
    {
        public MyCustomFieldPossibleValue Process { get; set; }

        public TargetTicketModel() : base()
        {
        }
    }
}