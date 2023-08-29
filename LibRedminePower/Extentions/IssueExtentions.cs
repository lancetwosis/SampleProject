using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class IssueExtentions
    {
        /// <summary>
        /// 例）#1011
        /// </summary>
        public static string GetLabel(this Issue i)
        {
            return $"#{i.Id}";
        }

        /// <summary>
        /// 例）#1011 工数実績の見える化対応
        /// </summary>
        public static string GetLongLabel(this Issue i)
        {
            return $"#{i.Id} {i.Subject}";
        }

        /// <summary>
        /// 例）機能 #1011: 工数実績の見える化対応
        /// </summary>
        public static string GetFullLabel(this Issue i)
        {
            return $"{i.Tracker.Name} #{i.Id}: {i.Subject}";
        }
    }
}
