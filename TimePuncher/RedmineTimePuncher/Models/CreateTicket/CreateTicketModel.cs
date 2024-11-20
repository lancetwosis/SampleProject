using LibRedminePower.Enums;
using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.CreateTicket.Enums;
using RedmineTimePuncher.Models.CreateTicket.Review;
using RedmineTimePuncher.Models.CreateTicket.Work;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket
{
    public class CreateTicketModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public CreateTicketMode CreateMode { get; set; }
        public ReviewsModel Reviews { get; set; } = new ReviewsModel();
        public WorkModel Work { get; set; } = new WorkModel();

        public CreateTicketModel()
        {
        }
    }
}