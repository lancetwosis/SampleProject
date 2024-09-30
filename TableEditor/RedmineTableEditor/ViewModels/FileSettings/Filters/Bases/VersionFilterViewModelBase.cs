using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Models;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Enums;
using RedmineTableEditor.Models;
using RedmineTableEditor.Models.FileSettings.Filters;
using RedmineTableEditor.Models.FileSettings.Filters.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.ViewModels.FileSettings.Filters.Bases
{
    public class VersionFilterViewModelBase<T> : ItemsFilterViewModelBase<T>
        where T : ItemsFilterModelBase, new()
    {
        public VersionFilterViewModelBase(T model) : base(model)
        {
        }

        public override void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> projects)
        {
            base.Setup(redmine, projects);

            projects.SubscribeWithErr(ps =>
            {
                var allItems = ps != null && ps.Count > 0 ?
                    ps.Where(p => redmine.Cache.ProjectVersions.ContainsKey(int.Parse(p.Id)))
                        .Select(p => redmine.Cache.ProjectVersions[int.Parse(p.Id)])
                        .SelectMany(vs => vs.Select(v => new FilterItemModel(v.Id, $"{v.Project.Name} - {v.Name}")))
                        .ToList() :
                    redmine.Cache.ProjectVersions.Values
                        .SelectMany(vs => vs.Select(v => new FilterItemModel(v.Id, $"{v.Project.Name} - {v.Name}")))
                        .ToList();

                updateAllItems(allItems);
            }).AddTo(myDisposables);
        }
    }
}
