using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize.Factors;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize
{
    public class PersonHourModel<T> : LibRedminePower.Models.Bases.ModelBaseSlim where T : IPeriod
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
        public FactorModel FixedVersion { get; set; }
        public List<FactorModel> CustomFields { get; set; }
        public FactorModel Project { get; set; }
        public FactorModel User { get; set; }
        public FactorModel SpentOn { get; set; }
        public FactorModel Category { get; set; }
        public FactorModel OnTime { get; set; }

        private PersonHourModel(Issue parent) : base(true)
        {
            RawIssue = parent;

            Issue = new FactorModel(parent);
            CustomFields = new List<FactorModel>();
            if (parent.CustomFields != null && parent.CustomFields.Any())
            {
                var cfs = parent.CustomFields.Where(icf => FactorTypes.CustomFields.Any(c => c.CustomField.Id == icf.Id))
                                             .Select(icf => new FactorModel(icf))
                                             .ToList();
                CustomFields.AddRange(cfs);
            }
        }

        public PersonHourModel(Issue parent, MyProject project, IdentifiableName user, DateTime spentOn, CategorySettingModel category, TimeEntryType entryType, List<MyTimeEntry> times)
            : this(parent)
        {
            Times = times;
            TotalHours = (double)times.Sum(t => t.Entry.Hours);

            Project = new FactorModel(project);
            FixedVersion = new FactorModel(parent.FixedVersion, project);
            User = new FactorModel(FactorTypes.User, user);
            SpentOn = new FactorModel(spentOn);
            Category = new FactorModel(category);
            OnTime = new FactorModel(entryType);
        }

        public PersonHourModel(Issue parent, params PersonHourModel[] children) : this(parent)
        {
            Times = children.SelectMany(p => p.Times).ToList();
            TotalHours = (double)Times.Sum(t => t.Entry.Hours);

            Project = children[0].Project;
            FixedVersion = children[0].FixedVersion;
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
                case FactorValueType.FixedVersion:
                    return FixedVersion;
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
                    throw new NotSupportedException($"type.ValueType が {type.ValueType} はサポート対象外です。");
            }
        }
    }
}
