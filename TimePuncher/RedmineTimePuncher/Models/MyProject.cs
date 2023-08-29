using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class MyProject : IdName
    {
        public string Identifier { get; set; }

        public MyProject() { }

        public MyProject(Project rawProject) : base(rawProject)
        {
            Identifier = rawProject.Identifier;
        }

        public override bool Equals(object obj)
        {
            return obj is MyProject project &&
                   Id == project.Id &&
                   Identifier == project.Identifier;
        }

        public override int GetHashCode()
        {
            int hashCode = -2144243362;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Identifier);
            return hashCode;
        }
    }
}
