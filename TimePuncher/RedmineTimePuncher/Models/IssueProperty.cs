using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class IssueProperty : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public static IssueProperty NOT_SPECIFIED = new IssueProperty() { Name = "", Key = "" };

        public string Name { get; set; }
        public string Key { get; set; }

        public IssueProperty() { }

        public IssueProperty(CustomField cf)
        {
            Name = cf.Name;
            Key = $"cf_{cf.Id}";
        }

        public IssueProperty(IssuePropertyType type)
        {
            Name = type.GetDescription();
            switch (type)
            {
                case IssuePropertyType.Subject:
                    Key = RedmineKeys.SUBJECT;
                    break;
                case IssuePropertyType.Status:
                    Key = RedmineKeys.STATUS;
                    break;
                case IssuePropertyType.AssignedTo:
                    Key = RedmineKeys.ASSIGNED_TO;
                    break;
                case IssuePropertyType.Author:
                    Key = RedmineKeys.AUTHOR;
                    break;
                case IssuePropertyType.Priority:
                    Key = RedmineKeys.PRIORITY;
                    break;
                case IssuePropertyType.Category:
                    Key = RedmineKeys.CATEGORY;
                    break;
                case IssuePropertyType.StartDate:
                    Key = RedmineKeys.START_DATE;
                    break;
                case IssuePropertyType.DueDate:
                    Key = RedmineKeys.DUE_DATE;
                    break;
                case IssuePropertyType.DoneRatio:
                    Key = RedmineKeys.DONE_RATIO;
                    break;
                case IssuePropertyType.FixedVersion:
                    Key = RedmineKeys.FIXED_VERSION;
                    break;
                case IssuePropertyType.Updated:
                    Key = RedmineKeys.UPDATED_ON;
                    break;
                default:
                    throw new NotSupportedException($"'{type}' is not supported for additional columns in issue list");
            }
        }

        public override bool Equals(object obj)
        {
            return obj is IssueProperty property &&
                   Name == property.Name &&
                   Key == property.Key;
        }

        public override int GetHashCode()
        {
            int hashCode = -314821886;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Key);
            return hashCode;
        }

        public override string ToString() => Name;
    }
}
