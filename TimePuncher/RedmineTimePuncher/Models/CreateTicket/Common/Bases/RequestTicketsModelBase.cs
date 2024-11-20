using LibRedminePower.Enums;
using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Common.Bases
{
    public abstract class RequestTicketsModelBase<TPeriod> : LibRedminePower.Models.Bases.ModelBaseSlim
        where TPeriod : PeriodModelBase
    {
        public ObservableCollection<AssigneeModel> Assignees { get; set; } = new ObservableCollection<AssigneeModel>();
        public TPeriod Period { get; set; }
        public string OpenTicketTitle { get; set; }
        public string Description { get; set; }

        protected RequestTicketsModelBase()
        {
        }
    }
}