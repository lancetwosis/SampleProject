using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models;
using RedmineTableEditor.Models.FileSettings.Filters;
using RedmineTableEditor.Models.FileSettings.Filters.Bases;
using RedmineTableEditor.Models.FileSettings.Filters.Standard;
using RedmineTableEditor.ViewModels.FileSettings.Filters.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.ViewModels.FileSettings.Filters.Standard
{
    public class PriorityFilterViewModel : ItemsFilterViewModelBase<PriorityFilterModel>
    {

        public PriorityFilterViewModel(PriorityFilterModel model) : base(model)
        {
        }

        public override void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> _ = null)
        {
            base.Setup(redmine, _);

            var allItems = redmine.Cache.Priorities.Select(a => new FilterItemModel(a)).ToList();
            updateAllItems(allItems);
        }
    }
}
