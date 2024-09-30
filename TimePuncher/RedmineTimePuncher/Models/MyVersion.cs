using LibRedminePower.Models;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class MyVersion : IdName
    {
        public static MyVersion NOT_SPECIFIED = new MyVersion() { Name = "", Id = INVALID_ID };

        public MyVersion() : base()
        {
        }

        public MyVersion(Redmine.Net.Api.Types.Version version)
        {
            Id = version.Id;
            Name = version.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is MyVersion version &&
                   Id == version.Id &&
                   Name == version.Name;
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
