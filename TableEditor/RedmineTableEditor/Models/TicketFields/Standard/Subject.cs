using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Standard
{
    public class Subject : Bases.FieldText
    {
        public Subject(Issue issue) :base(
                Properties.Resources.enumIssuePropertyTypeSubject,
                () => issue?.Subject,
                (v) => issue.Subject = v)
        {
        }
    }
}
