using RedmineTimePuncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Behaviors
{
    public class DraggedAppointment : MyAppointment
    {
        public enum DraggedType
        {
            Ticket,
            MyCategory,
        }

        public DraggedType From { get; }

        public DraggedAppointment(DraggedType from) :base()
        {
            From = from;
        }

        public DraggedAppointment(MyIssue ticket) : base()
        {
            From = DraggedType.Ticket;
            Subject = $"{ticket.Tracker.Name} ({ticket.Status.Name}) {ticket.Subject}";
            TicketNo = ticket.Id.ToString();
        }
    }
}
