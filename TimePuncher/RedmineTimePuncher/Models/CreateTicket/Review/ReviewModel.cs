using LibRedminePower.Enums;
using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.CreateTicket.Enums;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Review
{
    public class ReviewModel : RequestModelBase<TargetTicketModel, RequestTicketsModel, PeriodModel>
    {
        public string Name { get; set; }
        public ReviewStatus Status { get; set; }

        public ReviewModel() : base()
        {
            Target = new TargetTicketModel();
            Requests = new RequestTicketsModel();
        }
    }
}