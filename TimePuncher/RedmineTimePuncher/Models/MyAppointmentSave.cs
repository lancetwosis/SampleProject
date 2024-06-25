using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Models
{
    public class MyAppointmentSave
    {
        public string UniqueId { get; set; }
        public string Subject { get; set; }
        public ProjectStatusType ProjectStatus { get; set; }
        public string Body { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string ResourceName { get; set; }
        public string CategoryName { get; set; }
        public string TimeMarkerName { get; set; }

        public string TicketNo { get; set; }
        public string[] Attenders { get; set; }

        public Enums.AppointmentType ApoType { get; set; }
        public MyIssue Issue { get; set; }
        public TicketTreeModel TicketLinks { get; set; }
        public int TimeEntryId { get; set; }
        public int FromEntryId { get; set; }
        public List<string> MemberAppointmentIds { get; set; }
        public string TimeEntryType { get; set; }

        [Obsolete("For Serialize", false)]
        public MyAppointmentSave()
        {}

        public MyAppointmentSave(MyAppointment from)
        {
            this.UniqueId = from.UniqueId;
            this.Subject = from.Subject;
            this.ProjectStatus = from.ProjectStatus;
            this.Body = from.Body;
            this.Start = from.Start;
            this.End = from.End;
            this.ResourceName = from.Resources.First().ResourceName;
            this.CategoryName = from.Category?.CategoryName;
            this.TimeMarkerName = from.TimeMarker?.TimeMarkerName;
            this.TicketNo = from.TicketNo;
            this.Attenders = from.Attenders;
            this.ApoType = from.ApoType;
            this.Issue = from.Ticket;
            this.TicketLinks = from.TicketTree;
            this.MemberAppointmentIds = from.MemberAppointments.Select(a => a.UniqueId).ToList();
            this.TimeEntryId = from.TimeEntryId;
            this.FromEntryId = from.FromEntryId;
            this.TimeEntryType = from.TimeEntryType;
        }

        public MyAppointment ToMyAppointment(ObservableCollection<ResourceType> resourceTypes)
        {
            var res = resourceTypes[0].Resources.FirstOrDefault(a => ResourceName == a.ResourceName);
            if (res == null) return null;

            return new MyAppointment(res, this)
            {
                TimeEntryId = this.TimeEntryId,
                FromEntryId = this.FromEntryId,
                TimeEntryType = this.TimeEntryType,
            };
        }
    }
}
