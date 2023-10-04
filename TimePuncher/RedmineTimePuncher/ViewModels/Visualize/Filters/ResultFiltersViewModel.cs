using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Models.Visualize.Factors;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Visualize;
using RedmineTimePuncher.ViewModels.Visualize.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ObservableCollectionSync;

namespace RedmineTimePuncher.ViewModels.Visualize.Filters
{
    public class ResultFiltersViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public List<FactorType> AllTypes { get; set; }
        public List<FactorModel> Projects { get; set; }
        public List<FactorModel> Users { get; set; }
        public List<FactorModel> Categories { get; set; }
        public List<FactorModel> Versions { get; set; }
        public List<FactorModel> Dates { get; set; }
        public List<FactorModel> OnTimes { get; set; }
        public Dictionary<FactorType, List<FactorModel>> CustomFields { get; set; }

        public ObservableCollectionSync<ResultFilterViewModel, ResultFilterModel> Items { get; set; }

        public ResultFiltersViewModel(ResultViewModel parent, List<PersonHourModel> allEntries, bool needsClear)
        {
            Projects     = allEntries.Select(t => t.Project).Distinct().OrderBy(f => f.Value).ToList();
            Users        = allEntries.Select(t => t.User).Distinct().OrderBy(f => f.Value).ToList();
            Categories   = allEntries.Select(t => t.Category).Distinct().OrderBy(f => f.Value).ToList();
            Versions     = allEntries.Select(t => t.FixedVersion).Distinct().OrderBy(f => f.Value).ToList();
            Dates        = allEntries.Select(t => t.SpentOn).Distinct().OrderBy(f => f.Value).ToList();
            OnTimes      = allEntries.Select(t => t.OnTime).Distinct().OrderBy(f => f.Value).ToList();
            CustomFields = allEntries.SelectMany(t => t.CustomFields)
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => g.Distinct().OrderBy(f => f.Value).ToList());

            AllTypes = new List<FactorType>() { FactorTypes.Project, FactorTypes.User, FactorTypes.Category, FactorTypes.FixedVersion, FactorTypes.Date, FactorTypes.OnTime, };
            AllTypes.AddRange(CustomFields.Keys);

            if (needsClear)
                parent.Model.ResultFilters.Clear();

            Items = new ObservableCollectionSync<ResultFilterViewModel, ResultFilterModel>(parent.Model.ResultFilters,
                m => new ResultFilterViewModel(this, m).AddTo(disposables), vm => vm.Model).AddTo(disposables);
        }

        public void AddNewFilter()
        {
            var m = new ResultFilterModel();
            m.Type = FactorTypes.Project;

            var vm = new ResultFilterViewModel(this, m).AddTo(disposables);
            vm.NowEditing.Value = true;

            Items.Add(vm);
        }
    }
}
