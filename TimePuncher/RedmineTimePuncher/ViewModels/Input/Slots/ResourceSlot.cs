using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.ViewModels.Input.Resources;
using RedmineTimePuncher.ViewModels.Input.Resources.Bases;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.ViewModels.Input.Slots
{
    public class ResourceSlot : Slot
    {
        public Color Color => resource.Color;
        public Brush Brush => resource.Brush;
        public bool IsBackground => resource.IsBackground;

        private MyResourceBase resource;

        public ResourceSlot(DateTime start, DateTime end, MyResourceBase resource)
            : base(start, end, new[] { resource })
        {
            this.resource = resource;
        }
    }
}
