using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize.FactorTypes;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize
{
    public class PersonHourModel<T> : LibRedminePower.Models.Bases.ModelBase where T : IPeriod
    {
        public bool IsEnabled { get; set; }
        public List<T> Times { get; set; } = new List<T>();

        public PersonHourModel(bool isEnabled = true)
        {
            IsEnabled = isEnabled;
        }
    }

    public class PersonHourModel : PersonHourModel<MyTimeEntry>
    {
        public Issue RawIssue { get; set; }
        public double TotalHours { get; set; }

        public FactorModel Issue { get; set; }
        public List<FactorModel> CustomFields { get; set; }
        public FactorModel Project { get; set; }
        public FactorModel User { get; set; }
        public FactorModel SpentOn { get; set; }
        public FactorModel Category { get; set; }
        public FactorModel OnTime { get; set; }

        public PersonHourModel(Issue parent, List<CustomField> customFields, Project project, IdentifiableName user, DateTime spentOn, CategorySettingModel category, TimeEntryType entryType, List<MyTimeEntry> times)
            : base(true)
        {
            Times = times;
            RawIssue = parent;
            TotalHours = (double)times.Sum(t => t.Entry.Hours);

            Issue = new FactorModel(parent);
            CustomFields = new List<FactorModel>();
            parent.CustomFields?.ToList().ForEach(c => CustomFields.Add(new FactorModel(c)));

            Project = new FactorModel(project);
            User = new FactorModel(FactorType.User, user);
            SpentOn = new FactorModel(spentOn);
            Category = new FactorModel(category);
            OnTime = new FactorModel(entryType);
        }

        public PersonHourModel(Issue parent, params PersonHourModel[] children)
        {
            Times = children.SelectMany(p => p.Times).ToList();
            RawIssue = parent;
            TotalHours = (double)Times.Sum(t => t.Entry.Hours);

            Issue = new FactorModel(parent);
            CustomFields = new List<FactorModel>();
            parent.CustomFields?.ToList().ForEach(c => CustomFields.Add(new FactorModel(c)));

            Project = children[0].Project;
            User = children[0].User;
            SpentOn = children[0].SpentOn;
            Category = children[0].Category;
            OnTime = children[0].OnTime;
        }

        public FactorModel GetFactor(FactorType type)
        {
            switch (type.ValueType)
            {
                case FactorValueType.Date:
                    return SpentOn;
                case FactorValueType.Issue:
                    return Issue;
                case FactorValueType.Project:
                    return Project;
                case FactorValueType.User:
                    return User;
                case FactorValueType.Category:
                    return Category;
                case FactorValueType.OnTime:
                    return OnTime;
                case FactorValueType.IssueCustomField:
                    var field = CustomFields.FirstOrDefault(f => f.Type.Name == type.Name);
                    if (field != null)
                        return field;

                    var dummy = new FactorModel(new IssueCustomField() { Name = type.Name }, true);
                    CustomFields.Add(dummy);
                    return dummy;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
