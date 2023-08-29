using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
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
        public FactorModel Project { get; set; }
        public FactorModel User { get; set; }
        public FactorModel SpentOn { get; set; }
        public FactorModel Category { get; set; }
        public FactorModel OnTime { get; set; }

        public PersonHourModel(Issue parent, Project project, IdentifiableName user, DateTime spentOn, CategorySettingModel category, TimeEntryType entryType, List<MyTimeEntry> times)
            : base(true)
        {
            Times = times;
            RawIssue = parent;
            TotalHours = (double)times.Sum(t => t.Entry.Hours);

            Issue = new FactorModel(parent);
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
            Project = children[0].Project;
            User = children[0].User;
            SpentOn = children[0].SpentOn;
            Category = children[0].Category;
            OnTime = children[0].OnTime;
        }

        public FactorModel GetFactor(FactorType type)
        {
            switch (type)
            {
                case FactorType.Date:
                    return SpentOn;
                case FactorType.Issue:
                    return Issue;
                case FactorType.Project:
                    return Project;
                case FactorType.User:
                    return User;
                case FactorType.Category:
                    return Category;
                case FactorType.OnTime:
                    return OnTime;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
