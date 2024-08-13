using Redmine.Net.Api.Types;
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
        /// this が「対象チケットと同じ」の場合、defaultTracker を idName に設定する。それ以外の場合 this を変換したものを設定する。
        /// this が引数のプロジェクトで無効になっていた場合、false を返す。それ以外の場合、true を返す。
        /// </summary>
        /// <param name="project">TACKERS を include していること</param>
        public bool TryGetIdNameOrDefault(Project project, IdentifiableName defaultTracker, out IdentifiableName idName)
        {
            if (this.Equals(USE_PARENT_TRACKER))
            {
                idName = defaultTracker;
                return true;
            }
            else
            {
                if (project.Trackers.Any(t => t.Id == Id))
                {
                    idName = this.ToIdentifiableName();
                    return true;
                }
                else
                {
                    idName = defaultTracker;
                    return false;
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
