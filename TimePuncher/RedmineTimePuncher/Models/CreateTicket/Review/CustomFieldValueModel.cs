using LibRedminePower;
using LibRedminePower.Models;
using LibRedminePower.Models.Bases;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.CreateTicket.Review
{
    public class CustomFieldValueModel : ModelBaseSlim
    {
        public CustomField CustomField { get; set; }
        public string Value { get; set; }

        [Obsolete("For Serialize", true)]
        public CustomFieldValueModel()
        { }

        public CustomFieldValueModel(CustomField cf, string value = null)
        {
            CustomField = cf;
            Value = value ?? CustomField.DefaultValue;
        }

        public override bool Equals(object obj)
        {
            return this.JsonEquals(obj);
        }

        public override int GetHashCode()
        {
            int hashCode = 1450533402;
            hashCode = hashCode * -1521134295 + this.GetJsonHashcode();
            return hashCode;
        }
    }
}
