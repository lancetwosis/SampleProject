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
    public class TrackerFilterViewModel : ItemsFilterViewModelBase<TrackerFilterModel>
    {
        public TrackerFilterViewModel(TrackerFilterModel model) : base(model)
        {
        }

        public override void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> projects)
        {
            base.Setup(redmine, projects);

            projects.SubscribeWithErr(ps =>
            {
                var allItems = ps != null && ps.Count > 0 ?
                    ps.Select(p => redmine.Cache.Projects.First(a => a.Id.ToString() == p.Id))
                        .Where(p => p.Trackers != null)
                        .SelectMany(p => p.Trackers).Distinct()
                        .Select(t => new FilterItemModel(t))
                        .ToList() :
                    redmine.Cache.Trackers.Select(a => new FilterItemModel(a)).ToList();

                updateAllItems(allItems);
            }).AddTo(myDisposables);
        }
    }
}
