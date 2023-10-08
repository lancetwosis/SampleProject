using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.ChartView;

namespace RedmineTimePuncher.ViewModels.WikiPage.Charts
{
    public class CustomSeriesDescriptorSelector : ChartSeriesDescriptorSelector
    {
        public ChartSeriesDescriptor wikiDescriptor { get; set; }
        public ChartSeriesDescriptor userDescriptor { get; set; }

        public override ChartSeriesDescriptor SelectDescriptor(ChartSeriesProvider provider, object context)
        {
            if(context is WikiSeriesViewModel)
                return this.wikiDescriptor;
            else if(context is UserSeriesViewModel) 
                return this.userDescriptor;
            throw new NotSupportedException("Not supported series viewmodel type.");
        }

    }
}
