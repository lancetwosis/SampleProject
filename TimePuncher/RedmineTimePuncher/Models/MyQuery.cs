using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class MyQuery
    {
        public int Id { get; set; }
        public int? ProjectId { get; set; }
        public string Name { get; set; }

        [Obsolete("For Serialize", true)]
        public MyQuery() {}

        public MyQuery(Query query)
        {
            Id = query.Id;
            ProjectId = query.ProjectId;
            Name = query.Name;
        }

        public override string ToString() => Name;
    }
}
