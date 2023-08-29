using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using System.ComponentModel;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.Models;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ExportItems
    {
        [LocalizedDescription(nameof(Resources.enumExportItemsStartTime), typeof(Resources))]
        StartTime,
        [LocalizedDescription(nameof(Resources.enumExportItemsEndTime), typeof(Resources))]
        EndTime,
        [LocalizedDescription(nameof(Resources.enumExportItemsProject), typeof(Resources))]
        Project,
        [LocalizedDescription(nameof(Resources.enumExportItemsTicketFullName), typeof(Resources))]
        TicketFullName,
        [LocalizedDescription(nameof(Resources.enumExportItemsWorkCategory), typeof(Resources))]
        WorkCategory,
        [LocalizedDescription(nameof(Resources.enumExportItemsCategory), typeof(Resources))]
        Category,
        [LocalizedDescription(nameof(Resources.enumExportItemsVersion), typeof(Resources))]
        Version,
        [LocalizedDescription(nameof(Resources.enumExportItemsSubject), typeof(Resources))]
        Subject,
    }

    public static class ExportItemsEx
    {
        public  static string GetString(this ExportItems item, MyAppointment apo)
        {
            switch (item)
            {
                case ExportItems.StartTime:
                    return $"{apo.Start}";
                case ExportItems.EndTime:
                    return $"{apo.End}";;
                case ExportItems.Project:
                    return $"{apo.Ticket.Project.Name}";
                case ExportItems.TicketFullName:
                    return $"{apo.TicketTree}";
                case ExportItems.WorkCategory:
                    return $"{apo.Category.CategoryName}";
                case ExportItems.Category:
                    return $"{apo.Ticket.Category.Name}";
                case ExportItems.Version:
                    return $"{apo.Ticket.FixedVersion.Name}";
                case ExportItems.Subject:
                    return $"{apo.Subject}";
                default:
                    throw new ApplicationException();
            }

        }
    }
}
