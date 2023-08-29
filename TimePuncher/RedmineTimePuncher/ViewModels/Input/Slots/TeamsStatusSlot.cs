using LibRedminePower.Extentions;
using RedmineTimePuncher.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.ViewModels.Input.Slots
{
    public class TeamsStatusSlot : Slot
    {
        public Style MyStyle { get; set; }
        public TeamsStatus Status { get; set; }

        public TeamsStatusSlot(DateTime start, DateTime end, IEnumerable resources, TeamsStatus status)
            : base(start, end, resources)
        {
            Status = status;
            switch (status)
            {
                case TeamsStatus.undefined:
                case TeamsStatus.Unknown:
                    MyStyle = App.Current?.Resources["TeamsPurpleSlotStyle"] as Style;
                    break;
                case TeamsStatus.Offline:
                case TeamsStatus.ConnectionError:
                    MyStyle = App.Current?.Resources["TeamsGraySlotStyle"] as Style;
                    break;
                case TeamsStatus.OnThePhone:
                case TeamsStatus.Presenting:
                case TeamsStatus.InAMeeting:
                case TeamsStatus.Busy:
                case TeamsStatus.DoNotDisturb:
                    MyStyle = App.Current?.Resources["TeamsRedSlotStyle"] as Style;
                    break;
                case TeamsStatus.Away:
                case TeamsStatus.BeFightBack:
                    MyStyle = App.Current?.Resources["TeamsYellowSlotStyle"] as Style;
                    break;
                case TeamsStatus.Available:
                    MyStyle = App.Current?.Resources["TeamsGreenSlotStyle"] as Style;
                    break;
                case TeamsStatus.NewActivity:
                default:
                    throw new InvalidProgramException();
            }
        }

        public override string ToString()
        {
            return $"{Start} - {End} : {Status.GetDescription()}";
        }
    }    
}
