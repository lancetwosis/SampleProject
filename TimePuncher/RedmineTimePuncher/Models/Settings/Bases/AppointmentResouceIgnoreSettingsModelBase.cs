using LibRedminePower.Extentions;
using RedmineTimePuncher.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.Bases
{
    public abstract class AppointmentResouceIgnoreSettingsModelBase : AppointmentResouceSettingsModelBase
    {
        public bool IsIgnoreMinutes { get; set; } = false;
        public int IgnoreMinutes { get; set; } = 1;


        public virtual List<MyAppointment> Filter(List<MyAppointment> apos)
        {
            if (!IsIgnoreMinutes) return apos;

            var span = TimeSpan.FromMinutes(IgnoreMinutes);
            var result = new List<MyAppointment>();
            if (apos.Count > 1)
            {
                foreach (var apo in apos.Indexed().OrderBy(a => a.v.End).ThenBy(a => a.i).Select(a => a.v).Pairs().Indexed())
                {
                    if (apo.isFirst) result.Add(apo.v.First());
                    if (apo.v.Last().End - apo.v.First().End > span) result.Add(apo.v.Last());
                }
            }
            else
            {
                result.AddRange(apos);
            }
            return result;
        }
    }

}
