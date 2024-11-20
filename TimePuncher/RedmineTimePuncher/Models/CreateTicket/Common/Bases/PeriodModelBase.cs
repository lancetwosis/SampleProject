using LibRedminePower.Enums;
using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.CreateTicket.Common.Bases
{
    public abstract class PeriodModelBase : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }

        protected PeriodModelBase()
        {
        }
    }
}