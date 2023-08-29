using RedmineTimePuncher.Models;
using RedmineTimePuncher.ViewModels.Input.Resources;
using System.Linq;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Behaviors
{
    public class MyReadOnlyBehavior : ReadOnlyBehavior
    {
        public override bool CanDeleteAppointment(IReadOnlySettings readOnlySettings, IOccurrence occurrence)
        {
            var apo = getMyApointmentFromOccurence(occurrence);
            if (apo != null) return apo.CanDelete.Value;
            return base.CanDeleteAppointment(readOnlySettings, occurrence);
        }

        public override bool CanResizeAppointment(IReadOnlySettings readOnlySettings, IOccurrence occurrence)
        {
            var apo = getMyApointmentFromOccurence(occurrence);
            if (apo != null) return apo.CanResize.Value;
            return base.CanResizeAppointment(readOnlySettings, occurrence);
        }

        public override bool CanDragAppointment(IReadOnlySettings readOnlySettings, IOccurrence occurrence)
        {
            var apo = getMyApointmentFromOccurence(occurrence);
            if (apo != null) return apo.CanDrag.Value;
            return base.CanDragAppointment(readOnlySettings, occurrence);
        }

        private MyAppointment getMyApointmentFromOccurence(IOccurrence occurrence)
        {
            if(occurrence is MyAppointment apo)
            {
                return apo;
            }
            else
            {
                var occ = (occurrence as Occurrence).Appointment as MyAppointment;
                return occ;
            }
        }
    }
}
