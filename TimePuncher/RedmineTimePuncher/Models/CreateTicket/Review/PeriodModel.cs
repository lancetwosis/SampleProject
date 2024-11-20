using LibRedminePower.Enums;
using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Review
{
    public class PeriodModel : PeriodModelBase
    {
        public ReviewMethodValue Method { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime DueDateTime { get; set; }
        public bool NeedsCreateOutlookAppointment { get; set; }

        public PeriodModel() : base()
        {
        }
    }
}