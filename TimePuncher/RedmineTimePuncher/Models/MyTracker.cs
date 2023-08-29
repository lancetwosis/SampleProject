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
            Id = -1,
            Name = Properties.Resources.ReviewSameTracker
        };

        public static MyTracker NOT_SPECIFIED = new MyTracker()
        {
            Id = -1,
            Name = LibRedminePower.Properties.Resources.SettingsNotSpecified
        };

        public MyTracker() : base()
        {
        }

        public MyTracker(IdentifiableName identifiable) : base(identifiable)
        {
        }

        public IdentifiableName GetIdentifiableNameOrDefault(IdentifiableName defaultTracker)
        {
            return !this.Equals(USE_PARENT_TRACKER) ? this.ToIdentifiableName() : defaultTracker;
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
