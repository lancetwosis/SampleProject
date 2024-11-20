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
    public class MyTracker : IdName
    {
        public static MyTracker USE_PARENT_TRACKER = new MyTracker()
        {
            Id = INVALID_ID,
            Name = Properties.Resources.ReviewSameTracker
        };

        public static MyTracker NOT_SPECIFIED = new MyTracker()
        {
            Id = INVALID_ID,
            Name = LibRedminePower.Properties.Resources.SettingsNotSpecified
        };

        public MyTracker() : base()
        {
        }

        public MyTracker(IdentifiableName identifiable) : base(identifiable)
        {
        }


        /// <summary>
        /// this が「対象チケットと同じ」の場合、ticket のトラッカーを返す。それ以外の場合 this を変換したものを返す。
        /// this が ticket のプロジェクトで無効になっていた場合、null を返す。
        /// </summary>
        /// <param name="project">TACKERS を include していること</param>
        public IdentifiableName GetIdNameOrDefault(MyIssue ticket)
        {
            var project = CacheManager.Default.Projects.First(proj => proj.Id == ticket.Project.Id);
            if (this.Equals(USE_PARENT_TRACKER))
            {
                return ticket.RawIssue.Tracker;
            }
            else
            {
                if (project.Trackers.Any(t => t.Id == Id))
                {
                    return this.ToIdentifiableName();
                }
                else
                {
                    return null;
                }
            }
        }

        public override bool Equals(object obj)
        {
            return obj is MyTracker tracker &&
                   Id == tracker.Id &&
                   Name == tracker.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = -1919740922;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
