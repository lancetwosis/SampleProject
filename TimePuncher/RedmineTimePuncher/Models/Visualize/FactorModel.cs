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
using System.Windows.Media;

namespace RedmineTimePuncher.Models.Visualize
{
    public class FactorModel : LibRedminePower.Models.Bases.ModelBase
    {
        public FactorType Type { get; set; }
        public string Name { get; set; }
        public IComparable Value { get; set; }
        public object RawValue { get; set; }

        private FactorModel(FactorType type, string name, IComparable value, object rawValue)
        {
            Type = type;
            Name = name;
            Value = value;
            RawValue = rawValue;
        }

        public FactorModel(Issue issue) : this(FactorType.Issue, issue.GetLabel(), issue.Id, issue)
        {
        }

        public FactorModel(Project project) : this(FactorType.Project, project.Name, project.Id, project)
        {
        }

        public FactorModel(FactorType type, IdentifiableName idName) : this(type, idName.Name, idName.Id, idName)
        {
        }

        public FactorModel(CategorySettingModel category) : this(FactorType.Category, category.Name, category.TimeEntry.Id, category)
        {
        }

        public FactorModel(DateTime spentOn) : this(FactorType.Date, $"{spentOn.ToString("yy/MM/dd")}", spentOn, spentOn)
        {
        }

        public FactorModel(TimeEntryType type) : this(FactorType.OnTime, type.ToString(), type, type)
        {
        }

        public Brush GetColor()
        {
            switch (Type)
            {
                case FactorType.Issue:
                case FactorType.Project:
                case FactorType.User:
                    return Type.GetColor(Name, (int)Value);
                case FactorType.Category:
                    return ((CategorySettingModel)RawValue).GetBackground();
                case FactorType.OnTime:
                    return ((TimeEntryType)RawValue).GetColor();
                default:
                    return Type.GetColor(Name);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is FactorModel model &&
                   Type == model.Type &&
                   Name == model.Name &&
                   EqualityComparer<IComparable>.Default.Equals(Value, model.Value);
        }

        public override int GetHashCode()
        {
            int hashCode = 1519759645;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<IComparable>.Default.GetHashCode(Value);
            return hashCode;
        }
    }

}
