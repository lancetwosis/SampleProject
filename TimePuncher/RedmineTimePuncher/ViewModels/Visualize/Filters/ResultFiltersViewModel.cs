using LibRedminePower.Extentions;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
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
        public List<FactorModel> Dates { get; set; }
        public List<FactorModel> OnTimes { get; set; }

        public ObservableCollectionSync<ResultFilterViewModel, ResultFilterModel> Items { get; set; }

        private ResultViewModel parent { get; set; }

        public ResultFiltersViewModel(ResultViewModel parent, List<PersonHourModel> allEntries, bool needsClear)
        {
            this.parent = parent;

            AllTypes = new List<FactorType>() { FactorType.Project, FactorType.User, FactorType.Category, FactorType.Date, FactorType.OnTime };

            Projects = allEntries.Select(t => t.Project).Distinct().ToList();
            Users = allEntries.Select(t => t.User).Distinct().ToList();
            Categories = allEntries.Select(t => t.Category).Distinct().ToList();
            Dates = allEntries.Select(t => t.SpentOn).Distinct().ToList();
            OnTimes = allEntries.Select(t => t.OnTime).Distinct().ToList();

            if (needsClear)
                parent.Model.ResultFilters.Clear();

            Items = new ObservableCollectionSync<ResultFilterViewModel, ResultFilterModel>(parent.Model.ResultFilters,
                m => new ResultFilterViewModel(this, m).AddTo(disposables), vm => vm.Model).AddTo(disposables);
        }

        public void AddNewFilter()
        {
            var m = new ResultFilterModel();
            m.Type = FactorType.Project;

            var vm = new ResultFilterViewModel(this, m).AddTo(disposables);
            vm.NowEditing.Value = true;

            Items.Add(vm);
        }
    }
}
