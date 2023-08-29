using RedmineTimePuncher.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Converters
{
    public class AppointmentTypeToOpacityConverter : LibRedminePower.Converters.Bases.ConverterBase<Enums.AppointmentType, double>
    {
        public override double Convert(AppointmentType value, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case AppointmentType.Manual:
                case AppointmentType.Schedule:
                case AppointmentType.Mail:
                case AppointmentType.SkypeCall:
                case AppointmentType.TeamsCall:
                case AppointmentType.TeamsMeeting:
                case AppointmentType.RedmineActivity:
                case AppointmentType.RedmineTimeEntry:
                    return 1;
                case AppointmentType.RedmineTimeEntryMember:
                    return 0.6;
                default:
                    throw new InvalidOperationException($"{value} is not found.");
            }
        }

        public override AppointmentType ConvertBack(double value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
