using LibRedminePower.Converters;
using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize.Factors;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Visualize
{
    public class ResultFilterModel : LibRedminePower.Models.Bases.ModelBase
    {
        public bool IsEnabled { get; set; } = true;
        public FactorType Type { get; set; } = FactorTypes.Project;
        public ObservableCollection<FactorModel> Factors { get; set; } = new ObservableCollection<FactorModel>();
        public FilterType FilterType { get; set; }

        public ResultFilterModel() { }

        public bool IsMatch(PersonHourModel hour)
        {
            if (!IsEnabled)
                return true;

            var isMatch = Factors.Any(f => f.Value == hour.GetFactor(Type).Value);
            return FilterType == FilterType.Equals ? isMatch : !isMatch;
        }
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FilterType
    {
        [Description("等しい")]
        Equals,
        [Description("等しくない")]
        NotEquals
    }
}
