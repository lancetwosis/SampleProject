using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Visualize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Input.Report
{
    public class TimesAreaViewModel<T> : LibRedminePower.ViewModels.Bases.ViewModelBase where T : IPeriod
    {
        public bool IsVisible { get; set; }
        public bool IsVisibleRemaining { get; set; }

        public string Name { get; set; }
        public string TotalHours { get; set; }

        public string Name1 { get; set; }
        public string Time1Hours { get; set; }
        public string Time1Percentage { get; set; }
        public string Name2 { get; set; }
        public string Time2Hours { get; set; }
        public string Time2Percentage { get; set; }

        public string TsvLines { get; set; } = string.Empty;

        private TimesAreaViewModel(bool isVisible, string name)
        {
            IsVisible = isVisible;
            Name = name;
        }

        public TimesAreaViewModel(bool isVisible, string name, PersonHourModel<T> total,
            string name1, PersonHourModel<T> time1, string name2, PersonHourModel<T> time2)
            : this(isVisible && total.IsEnabled, name)
        {
            if (!IsVisible)
                return;

            var all = total.Times.Select(a => (a.End - a.Start).TotalSeconds).Sum();
            var t1 = time1.Times.Select(a => (a.End - a.Start).TotalSeconds).Sum();
            var t2 = time2.Times.Select(a => (a.End - a.Start).TotalSeconds).Sum();

            setUp(all, name1, t1, name2, t2);
        }

        public TimesAreaViewModel(bool isVisible, string name, PersonHourModel<T> total,
            string name1, PersonHourModel<T> onTime, string name2, PersonHourModel<T> overTime, PersonHourModel<T> notWorked)
            : this(isVisible && total.IsEnabled, name)
        {
            if (!IsVisible)
                return;

            var all = total.Times.Select(a => (a.End - a.Start).TotalSeconds).Sum();
            var t1 = onTime.Times.Select(a => (a.End - a.Start).TotalSeconds).Sum();
            var t2 = overTime.Times.Select(a => (a.End - a.Start).TotalSeconds).Sum();

            if (notWorked.IsEnabled)
            {
                var t3 = notWorked.Times.Select(a => (a.End - a.Start).TotalSeconds).Sum();
                all = all - t3;
                t2 = t2 - t3;
            }

            setUp(all, name1, t1, name2, t2);
        }

        private void setUp(double allSec, string name1, double t1Sec, string name2, double t2Sec)
        {
            var all = TimeSpan.FromSeconds(allSec);
            var t1 = TimeSpan.FromSeconds(t1Sec);
            var t2 = TimeSpan.FromSeconds(t2Sec);

            TotalHours = createHours(all);
            Name1 = name1;
            Name2 = name2;
            Time1Hours = createHours(t1);
            Time2Hours = createHours(t2);
            Time1Percentage = createPercentage(all, t1, true);
            Time2Percentage = createPercentage(all, t2, false);

            TsvLines = string.Join(Environment.NewLine, new[]
            {
                $"{Name}\t{TotalHours}",
                $"  {Name1}\t{Time1Hours}\t{Time1Percentage}",
                $"  {Name2}\t{Time2Hours}\t{Time2Percentage}",
            });
        }

        public TimesAreaViewModel(bool isVisible,  string name,
            string name1, PersonHourModel<T> time1, string name2, PersonHourModel<T> time2)
            : this(isVisible && time1.IsEnabled, name)
        {
            if (!IsVisible)
                return;

            var t1 = TimeSpan.FromSeconds(time1.Times.Select(a => (a.End - a.Start).TotalSeconds).Sum());

            Name1 = name1;
            Time1Hours = createHours(t1);

            IsVisibleRemaining = time2.IsEnabled;
            if (IsVisibleRemaining)
            {
                var t2 = TimeSpan.FromSeconds(time2.Times.Select(a => (a.End - a.Start).TotalSeconds).Sum());
                Name2 = name2;
                Time2Hours = createHours(t2);
            }

            var lines = new string[]
            {
                $"{Name}",
                $"  {Name1}\t{Time1Hours}",
            }.ToList();
            if (IsVisibleRemaining)
                lines.Add($"  {Name2}\t{Time2Hours}");
            TsvLines = string.Join(Environment.NewLine, lines);
        }

        private string createHours(TimeSpan time)
        {
            return Math.Round(time.TotalHours, 2).ToString("0.00");
        }

        private string createPercentage(TimeSpan all, TimeSpan time, bool isDefault)
        {
            if (all.TotalHours == 0)
                return isDefault ? "100.0" : "0.0";
            else
                return Math.Round(time.TotalHours / all.TotalHours * 100, 1).ToString("0.0");
        }
    }
}
