using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class TicketTreeItemModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public int No { get; set; }
        public MyIssue Issue { get; set; }
        public bool IsLastChild { get; set; }

        public TicketTreeItemModel(int no, MyIssue issue, bool isLastChild )
        {
            No = no;
            Issue = issue;
            IsLastChild = isLastChild;
        }

        public override string ToString()
        {
            return Issue.ToString();
        }
    }
}
