using RedmineTimePuncher.ViewModels;
using RedmineTimePuncher.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Calendar;

namespace RedmineTimePuncher.Selectors
{
    public class MyDayTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate SelectedTemplate { get; set; }
        public DataTemplate DisplayedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var date = item as CalendarButtonContent;
            if (date.ButtonType == CalendarButtonType.Date ||
                date.ButtonType == CalendarButtonType.TodayDate)
            {
                var view = (container as ContentPresenter).ParentOfType<InputView>();
                if (view.DataContext == null)
                    return DefaultTemplate;

                var vm = (view.DataContext as MainWindowViewModel).Input;
                if (vm.SelectedDate.Value.Date == date.Date.Date)
                {
                    return SelectedTemplate;
                }
                // StartTime と EndTime は DayStartTime を加味した時刻となっているため以下の処理とする
                // （例えば表示期間が3日で選択日が 8/14 なら Start:8/12 00:05:00, End:8/15 00:05:00 となる）
                else if (vm.DisplayStartTime.Value.Date <= date.Date && date.Date < vm.DisplayEndTime.Value.Date)
                {
                    return DisplayedTemplate;
                }
            }

            return DefaultTemplate;
        }
    }
}
