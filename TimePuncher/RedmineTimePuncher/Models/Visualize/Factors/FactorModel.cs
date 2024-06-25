using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using RedmineTimePuncher.Models.Visualize.Factors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;
using RedmineTimePuncher.Properties;

namespace RedmineTimePuncher.Models.Visualize.Factors
{
    public class FactorModel : LibRedminePower.Models.Bases.ModelBaseSlim
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

                if (string.IsNullOrEmpty(RawValueJson))
                    return null;

                if (Type.Equals(FactorTypes.Issue))
                    rawValue = CloneExtentions.ToObject<Issue>(RawValueJson);
                else if (Type.Equals(FactorTypes.Project))
                    rawValue = CloneExtentions.ToObject<MyProject>(RawValueJson);
                else if (Type.Equals(FactorTypes.User) || Type.Equals(FactorTypes.FixedVersion))
                    rawValue = CloneExtentions.ToObject<IdentifiableName>(RawValueJson);
                else if (Type.Equals(FactorTypes.Category))
                    rawValue = CloneExtentions.ToObject<CategorySettingModel>(RawValueJson);
                else if (Type.Equals(FactorTypes.Date))
                    rawValue = DateTime.Parse(RawValueJson);
                else if (Type.Equals(FactorTypes.OnTime))
                    rawValue = FastEnumUtility.FastEnum.Parse<TimeEntryType>(RawValueJson);

                return rawValue;
            }
        }

        private object rawValue { get; set; }

        public FactorModel() { }

        private FactorModel(FactorType type, object rawValue)
        {
            Type = type;

            this.rawValue = rawValue;
            switch (type.ValueType)
            {
                case FactorValueType.Issue:
                case FactorValueType.Project:
                case FactorValueType.User:
                case FactorValueType.Category:
                case FactorValueType.IssueCustomField:
                    RawValueJson = rawValue.ToJson();
                    break;
                case FactorValueType.FixedVersion:
                    if (rawValue != null)
                        RawValueJson = rawValue.ToJson();
                    break;
                case FactorValueType.Date:
                case FactorValueType.OnTime:
                    RawValueJson = rawValue.ToString();
                    break;
                default:
                    break;
            }
        }

        private FactorModel(FactorType type, string name, long value, object rawValue) : this(type, rawValue)
        {
            Name = name;
            Value = value;
        }

        public FactorModel(Issue issue) : this(FactorTypes.Issue, issue.GetLabel(), issue.Id, issue)
        {
        }

        private static long INVALID_VALUE = long.MaxValue;
        private static long NOT_SET_VALUE = long.MaxValue - 1;

        public FactorModel(IssueCustomField customField, bool isDummy = false)
            : this(FactorTypes.CustomFields.First(f => f.Name == customField.Name), customField)
        {
            if (isDummy)
            {
                Name = Resources.VisualizeCustomFieldInvalid;
                Value = INVALID_VALUE;
            }
            else if (customField.HasValue())
            {
                if (Type.CustomField.Format == CustomFieldFormat.List)
                {
                    // v.Info に表示用の文字列が定義されている
                    Name = string.Join(", ", customField.Values.Select(v => v.Info));

                    // 定義順にソートする
                    var first = customField.Values.First().Info;
                    Value = Type.CustomField.PossibleValues.Indexed().First(p => p.v.Label == first).i;
                }
                else if (Type.CustomField.Format == CustomFieldFormat.User)
                {
                    var userIds = customField.Values.Select(v => v.Info).ToList();
                    var users = Type.CustomField.PossibleValues.Where(v => userIds.Contains(v.Value)).ToList();
                    Name = string.Join(", ", users.Select(u => u.Label));
                    Value = int.Parse(users[0].Value);
                }
                else if (Type.CustomField.Format == CustomFieldFormat.Version)
                {
                    var versionIds = customField.Values.Select(v => v.Info).ToList();
                    // Project と Version の ID から構成されているので EndWith で判定（MyProject の CreateVersionValue 参照）
                    var versions = Type.CustomField.PossibleValues.Where(v => versionIds.Any(id => v.Value.EndsWith(id))).ToList();
                    Name = string.Join(", ", versions.Select(u => u.Label));
                    Value = int.Parse(versions[0].Value);
                }
            }
            else
            {
                Name = Resources.VisualizeCustomFieldNotSet;
                Value = NOT_SET_VALUE;
            }
        }

        public FactorModel(MyProject project) : this(FactorTypes.Project, project.Name, project.Id, project)
        {
        }

        public FactorModel(FactorType type, IdentifiableName idName)
            : this(type, idName != null ? idName.Name : Resources.VisualizeCustomFieldNotSet, idName != null ? idName.Id : NOT_SET_VALUE, idName)
        {
        }

        public FactorModel(IdentifiableName version, MyProject project) : this (FactorTypes.FixedVersion, version)
        {
            if (version != null)
            {
                Name = project.CreateVersionLabel(version.Id);
                Value = project.CreateVersionValue(version.Id);
            }
        }

        public FactorModel(CategorySettingModel category) : this(FactorTypes.Category, category.Name, category.TimeEntry.Id, category)
        {
        }

        public FactorModel(DateTime spentOn) : this(FactorTypes.Date, $"{spentOn.ToString("yy/MM/dd")}", spentOn.Ticks, spentOn)
        {
        }

        public FactorModel(TimeEntryType type) : this(FactorTypes.OnTime, type.ToString(), (long)type, type)
        {
        }

        public Brush GetColor()
        {
            switch (Type.ValueType)
            {
                case FactorValueType.Issue:
                case FactorValueType.Project:
                case FactorValueType.User:
                    return Type.GetColor(Name, (int)Value);
                case FactorValueType.Category:
                    return ((CategorySettingModel)RawValue).GetBackground();
                case FactorValueType.OnTime:
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
