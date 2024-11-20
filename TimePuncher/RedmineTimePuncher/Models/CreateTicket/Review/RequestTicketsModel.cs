using LibRedminePower.Enums;
using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.CreateTicket.Common;
using RedmineTimePuncher.Models.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Review
{
    public class RequestTicketsModel : RequestTicketsModelBase<PeriodModel>
    {
        public AssigneeModel Organizer { get; set; }
        public SelfReviewModel SelfReview { get; set; } = new SelfReviewModel();
        public CustomFieldsModel CustomFields { get; set; } = new CustomFieldsModel();
        public string MergeRequestUrl { get; set; }
        public string ReviewTarget { get; set; }

        public RequestTicketsModel() : base()
        {
            Period = new PeriodModel();
        }
    }
}