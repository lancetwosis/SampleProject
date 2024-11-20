﻿using LibRedminePower.Enums;
using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.CreateTicket.Common;
using RedmineTimePuncher.Models.CreateTicket.Common.Bases;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Work
{
    public class WorkModel : RequestModelBase<TargetTicketModel, RequestTicketsModel, PeriodModel>
    {
        public WorkModel() : base()
        {
            Target = new TargetTicketModel();
            Requests = new RequestTicketsModel();
        }
    }
}