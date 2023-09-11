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
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTimePuncher.Models.Visualize
{
    public class FactorModel : LibRedminePower.Models.Bases.ModelBase
    {
        public FactorType Type { get; set; }
        public string Name { get; set; }
        public long Value { get; set; }
        public string RawValueJson { get; set; }

        [JsonIgnore]
        public object RawValue
        {
            get
            {
                if (rawValue != null)
                    return rawValue;

                if (Type == FactorType.Issue)
                    rawValue = CloneExtentions.ToObject<Issue>(RawValueJson);
                else if (Type == FactorType.Project)
                    rawValue = CloneExtentions.ToObject<Project>(RawValueJson);
                else if (Type == FactorType.User)
                    rawValue = CloneExtentions.ToObject<IdentifiableName>(RawValueJson);
                else if (Type == FactorType.Category)
                    rawValue = CloneExtentions.ToObject<CategorySettingModel>(RawValueJson);
                else if (Type == FactorType.Date)
                    rawValue = DateTime.Parse(RawValueJson);
                else if (Type == FactorType.OnTime)
                    rawValue = FastEnumUtility.FastEnum.Parse<TimeEntryType>(RawValueJson);

                return rawValue;
            }
        }

        private object rawValue { get; set; }

        public FactorModel() { }

        private FactorModel(FactorType type, string name, long value, object rawValue)
        {
            Type = type;
            Name = name;
            Value = value;

            this.rawValue = rawValue;
            switch (type)
            {
                case FactorType.Issue:
                case FactorType.Project:
                case FactorType.User:
                case FactorType.Category:
                    RawValueJson = rawValue.ToJson();
                    break;
                case FactorType.Date:
                case FactorType.OnTime:
                    RawValueJson = rawValue.ToString();
                    break;
                default:
                    break;
            }
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

        public FactorModel(DateTime spentOn) : this(FactorType.Date, $"{spentOn.ToString("yy/MM/dd")}", spentOn.Ticks, spentOn)
        {
        }

        public FactorModel(TimeEntryType type) : this(FactorType.OnTime, type.ToString(), (long)type, type)
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

        public override string ToString()
        {
            return $"{Type} : {Name}";
        }
    }

}
