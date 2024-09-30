using RedmineTimePuncher.ViewModels.Input.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RedmineTimePuncher.Selectors
{
    public class MyTimeIndicatorStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is MyTimeIndicator) return App.Current.Resources["MyTimeIndicatorStyle"] as Style; 
            throw new NotSupportedException();
        }
    }
}
