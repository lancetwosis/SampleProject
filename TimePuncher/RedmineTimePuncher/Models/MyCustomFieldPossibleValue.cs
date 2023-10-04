using LibRedminePower.Extentions;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models
{
    public class MyCustomFieldPossibleValue : LibRedminePower.Models.Bases.ModelBase
    {
        public static string YES = "1";
        public static string NO  = "0";

        public string Label { get; set; }
        public string Value { get; set; }
        public bool IsDefault { get; set; }

        public MyCustomFieldPossibleValue() { }

        public MyCustomFieldPossibleValue(CustomFieldPossibleValue cfpv)
        {
            Label = cfpv.Label;
            Value = cfpv.Value;
        }

        public MyCustomFieldPossibleValue(string label, bool value, bool isDefault = false)
        {
            Label = label;
            Value = value ? YES : NO;
            IsDefault = isDefault;
        }

        public MyCustomFieldPossibleValue(MyUser user)
        {
            Label = user.Name;
            Value = user.Id.ToString();
        }

        public MyCustomFieldPossibleValue(MyProject project, Redmine.Net.Api.Types.Version version, bool multiProject)
        {
            Label = multiProject ? project.CreateVersionLabel(version.Id) : version.Name;
            Value = $"{project.CreateVersionValue(version.Id)}";
        }


        public override string ToString()
        {
            return $"Label={Label}, Value={Value}";
        }

        public override bool Equals(object obj)
        {
            return obj is MyCustomFieldPossibleValue value &&
                   Label == value.Label &&
                   Value == value.Value;
        }

        public override int GetHashCode()
        {
            int hashCode = 197784975;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Label);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }
    }
}
