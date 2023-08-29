using RedmineTimePuncher.ViewModels.Input.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Selectors
{
    public class MyGroupHeaderContentTemplateSelector : ScheduleViewDataTemplateSelector
    {
        public DataTemplate HorizontalTemplate { set; get; }
        public DataTemplate HorizontalMyWorksResourceTemplate { set; get; }
        public DataTemplate HorizontalRedmineResourceTemplate { set; get; }
        public DataTemplate HorizontalOutlookTeamsResourceTemplate { set; get; }
        public DataTemplate HorizontalMemberResourceTemplate { set; get; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container, ViewDefinitionBase activeViewDeifinition)
        {
            if (activeViewDeifinition.GetOrientation() == Orientation.Vertical)
            {
                var cvg = item as CollectionViewGroup;
                if (cvg != null)
                {
                    if (cvg.Name is MyWorksResource)
                        return this.HorizontalMyWorksResourceTemplate;
                    else if (cvg.Name is RedmineResource)
                        return this.HorizontalRedmineResourceTemplate;
                    else if (cvg.Name is OutlookTeamsResource)
                        return this.HorizontalOutlookTeamsResourceTemplate;
                    else if (cvg.Name is MemberResource)
                        return this.HorizontalMemberResourceTemplate;
                    else if (cvg.Name is DateTime)
                        return this.HorizontalTemplate;
                }
            }
            return base.SelectTemplate(item, container, activeViewDeifinition);
        }
    }
}