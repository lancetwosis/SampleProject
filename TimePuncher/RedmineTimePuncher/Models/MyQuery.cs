using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class MyQuery : IdName
    {
        public int? ProjectId { get; set; }

        [Obsolete("For Serialize", true)]
        public MyQuery() {}

        public MyQuery(Query query) : base(query)
        {
            ProjectId = query.ProjectId;
        }

        public override string ToString() => Name;
    }
}
