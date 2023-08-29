using RedmineTimePuncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Telerik.Windows.Controls.GridView;

namespace RedmineTimePuncher.Selectors
{
    public class MyIssueStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {

            if (item is MyIssue issue && !issue.IsClosed)
            {
                var s = issue.Priority.ToRowStyle();
                if (s != null)
                    return s;

                if (issue.IsExpired)
                    return App.Current.Resources["ExpiredIssueStyle"] as Style;
            }
            return null;
        }
    }

}

