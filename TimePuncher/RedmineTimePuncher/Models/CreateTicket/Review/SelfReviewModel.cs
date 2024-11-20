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
    public class SelfReviewModel : ModelBaseSlim
    {
        public bool IsEnabled { get; set; }
        public MyIssue OpenTicket { get; set; }
        public MyIssue SelfTicket { get; set; }
        public IdentifiableName RequestTracker { get; set; }
        public IdentifiableName PointTracker { get; set; }
        public string Desctription { get; set; }
        public string ShowAllUrl { get; set; }

        public SelfReviewModel()
        {
        }
    }
}