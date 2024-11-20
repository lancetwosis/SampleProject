using LibRedminePower.Enums;
using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Common.Bases
{
    public abstract class TargetTicketModelBase : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public MyIssue Ticket { get; set; }
        public IssueStatus StatusUnderRequest { get; set; }

        protected TargetTicketModelBase()
        {
        }
    }
}