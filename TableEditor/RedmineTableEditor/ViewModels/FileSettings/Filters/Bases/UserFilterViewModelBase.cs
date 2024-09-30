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
    public class UserFilterViewModelBase<T> : ItemsFilterViewModelBase<T>
        where T : ItemsFilterModelBase, new()
    {
        public UserFilterViewModelBase(T model) : base(model)
        {
        }

        public override void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> projects)
        {
            base.Setup(redmine, projects);

            projects.SubscribeWithErr(ps =>
            {
                var allItems = ps != null && ps.Count > 0 ?
                    ps.Where(p => redmine.Cache.ProjectMemberships.ContainsKey(int.Parse(p.Id)))
                        .SelectMany(p => redmine.Cache.ProjectMemberships[int.Parse(p.Id)]).Distinct()
                        .Where(m => m.User != null)
                        .Select(m => new FilterItemModel(m.User.Id, m.User.Name)).ToList() :
                    redmine.Cache.Users.Select(u => new FilterItemModel(u.Id, u.Name)).ToList();

                allItems.Insert(0, UsersFilterModelBase.ME);

                updateAllItems(allItems);
            }).AddTo(myDisposables);
        }
    }
}
