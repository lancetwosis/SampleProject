using LibRedminePower.Extentions;
using Redmine.Net.Api.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRedminePower.Properties;
using Redmine.Net.Api;

namespace LibRedminePower.Exceptions
{
    public class RedmineApiException : ApplicationException
    {
        public RedmineApiException(string method, Type targetType, RedmineException exception, string id = null)
            : base(createMessage(method, targetType,exception, id), exception)
        {
        }

        private static Dictionary<Type, string> displayNames = new Dictionary<Type, string>()
        {
            { typeof(Redmine.Net.Api.Types.Issue),              Resources.DisplayNameIssue },
            { typeof(Redmine.Net.Api.Types.CustomField),        Resources.DisplayNameCustomField },
            { typeof(Redmine.Net.Api.Types.Tracker),            Resources.DisplayNameTracker },
            { typeof(Redmine.Net.Api.Types.IssuePriority),      Resources.DisplayNameIssuePriority },
            { typeof(Redmine.Net.Api.Types.TimeEntry),          Resources.DisplayNameTimeEntry },
            { typeof(Redmine.Net.Api.Types.Project),            Resources.DisplayNameProject },
            { typeof(Redmine.Net.Api.Types.User),               Resources.DisplayNameUser },
            { typeof(Redmine.Net.Api.Types.ProjectMembership),  Resources.DisplayNameProjectMembership },
            { typeof(Redmine.Net.Api.Types.Version),            Resources.DisplayNameVersion },
            { typeof(Redmine.Net.Api.Types.IssueStatus),        Resources.DisplayNameIssueStatus },
            { typeof(Redmine.Net.Api.Types.Query),              Resources.DisplayNameQuery },
            { typeof(Redmine.Net.Api.Types.IssueCategory),      Resources.DisplayNameIssueCategory },
            { typeof(Redmine.Net.Api.Types.TimeEntryActivity),  Resources.DisplayNameTimeEntryActivity },
            { typeof(Redmine.Net.Api.Types.WikiPage),           Resources.DisplayNameWikiPage },
        };

        private static string createMessage(string method, Type targetType, RedmineException exception, string id)
        {
            var sb = new StringBuilder();
            if (method == nameof(RedmineManagerExtenstions.GetCurrentUserWithErrConv))
            {
                sb.Append(Resources.msgErrRedmineFailedToGetCurrentUser);
            }
            else
            {
                var target = id != null ? $"{displayNames[targetType]} (#{id}) " : displayNames[targetType];
                if (method == HttpVerbs.GET)
                {
                    sb.Append(string.Format(Resources.msgErrRedmineFailedToGet, target));
                }
                else if (method == HttpVerbs.POST)
                {
                    sb.Append(string.Format(Resources.msgErrRedmineFailedToPost, target));
                }
                else if (method == HttpVerbs.PUT)
                {
                    sb.Append(string.Format(Resources.msgErrRedmineFailedToPut, target));
                }
                else if (method == HttpVerbs.DELETE)
                {
                    sb.Append(string.Format(Resources.msgErrRedmineFailedToDelete, target));
                }
            }

            sb.AppendLine();
            sb.AppendLine();

            if (exception.Errors != null)
            {
                sb.Append(string.Join(Environment.NewLine, exception.Errors.Select(a => a.Info)));
            }
            else
            {
                sb.Append(exception.ToErrMsg());
            }

            return sb.ToString();
        }
    }
}
