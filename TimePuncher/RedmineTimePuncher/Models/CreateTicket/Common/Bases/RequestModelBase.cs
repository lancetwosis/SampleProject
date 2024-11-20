using LibRedminePower.Enums;
using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Common.Bases
{
    public abstract class RequestModelBase<TTarget, TRequests, TPeriod> : LibRedminePower.Models.Bases.ModelBaseSlim
        where TTarget : TargetTicketModelBase
        where TRequests : RequestTicketsModelBase<TPeriod>
        where TPeriod : PeriodModelBase
    {
        public TTarget Target { get; set; }
        public TRequests Requests { get; set; }

        protected RequestModelBase()
        {
        }
    }
}