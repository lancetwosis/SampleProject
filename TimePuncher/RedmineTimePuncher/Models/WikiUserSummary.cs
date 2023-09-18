using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class WikiUserSummary
    {
        public IdentifiableName Author { get; set; }
        public string Name => Author.Name;
        public int InsertNoOfLine { get; set; }
        public int InsertNoOfChar { get; set; }
        public int DeleteNoOfLine { get; set; }
        public int DeleteNoOfChar { get; set; }

    }
}
