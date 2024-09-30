using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace RedmineTimePuncher.Models
{
    public class MyPriority : IdName
    {
        public MyPriority() : base()
        {
        }

        public MyPriority(IdentifiableName identifiable) : base(identifiable)
        {
        }

        private static Dictionary<int, Style> styleDic;
        public Style ToRowStyle()
        {
            if (styleDic == null)
            {
                styleDic = new Dictionary<int, Style>();

                var priorities = CacheManager.Default.Priorities;
                var defaultPriority = priorities.First(p => p.IsDefault);

                var highs = priorities.Where(p => p.Id > defaultPriority.Id).OrderByDescending(p => p.Id).ToList();
                if (highs.Count >= 1)
                {
                    styleDic[highs[0].Id] = App.Current.Resources["FirstPriorityIssueStyle"] as Style;
                }
                if (highs.Count >= 2)
                {
                    styleDic[highs[1].Id] = App.Current.Resources["SecondPriorityIssueStyle"] as Style;
                }
                if (highs.Count >= 3)
                {
                    styleDic[highs[2].Id] = App.Current.Resources["ThirdPriorityIssueStyle"] as Style;
                }

                var lows = priorities.Where(p => p.Id < defaultPriority.Id).OrderBy(p => p.Id).ToList();
                if (lows.Count >= 1)
                {
                    styleDic[lows[0].Id] = App.Current.Resources["LowPriorityIssueStyle"] as Style;
                }
            }

            return styleDic.TryGetValue(Id, out var style) ? style : null;
        }
    }
}
