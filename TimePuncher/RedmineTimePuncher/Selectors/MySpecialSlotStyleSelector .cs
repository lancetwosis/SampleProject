using LibRedminePower.Extentions;
using RedmineTimePuncher.ViewModels.Input.Slots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Selectors
{
    public class MySpecialSlotStyleSelector : ScheduleViewStyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container, ViewDefinitionBase activeViewDefinition)
        {
            if (item is ResourceSlot) return App.Current.Resources["ResourceSlotStyle"] as Style; 
            else if (item is TeamsStatusSlot t)
            {
                container.SetValue(HighlightItem.TagProperty, t.Status.GetDescription());
                return t.MyStyle;
            }
            return base.SelectStyle(item, container, activeViewDefinition);
        }
    }
}
