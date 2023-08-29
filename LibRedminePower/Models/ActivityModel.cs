using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Models
{
    public class ActivityModel
    {
        public string ProjectName { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string IssueId { get; set; }
        public string ChangeNo { get; set; }
        public string Url { get; set; }
        public string UserNo { get; set; }
    }
}
