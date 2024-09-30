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
    public class ProjectFilterViewModel : ItemsFilterViewModelBase<ProjectFilterModel>
    {
        public ReadOnlyReactivePropertySlim<List<FilterItemModel>> Projects { get; set; }

        public ProjectFilterViewModel(ProjectFilterModel model, RedmineManager redmine) : base(model)
        {
            Projects = Model.Items.CollectionChangedAsObservable().StartWithDefault().CombineLatest(
                    Model.ObserveProperty(m => m.IsSelected),
                    Model.ObserveProperty(m => m.IsChecked),
                    Model.ObserveProperty(m => m.IsMultiple),
                    Model.ObserveProperty(m => m.SelectedItem),
                    (_, iss, ise, ism, i) => (Filters: Model.Items, IsSelected: iss, IsEnabled: ise, IsMultiple: ism, Filter: i))
                .Select(p =>
                {
                    if (!p.IsSelected || !p.IsEnabled)
                        return null;

                    if (p.IsMultiple)
                        return p.Filters.Count > 0 ?
                            p.Filters.SelectMany(f => getFilters(f, redmine)).Distinct((f1, f2) => f1.Id == f2.Id).ToList() :
                            null;
                    else
                        return p.Filter != null ? getFilters(p.Filter, redmine) : null;
                }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        private List<FilterItemModel> getFilters(FilterItemModel filter, RedmineManager redmine)
        {
            if (filter.Equals(ProjectFilterModel.MINE))
            {
                return redmine.Cache.MyUser.Memberships
                                    .Select(m => Model.AllItems.FirstOrDefault(i => i.Id == m.Project.Id.ToString()))
                                    .Where(a => a != null).ToList();
            }
            else
            {
                return new List<FilterItemModel>() { filter };
            }
        }

        public override void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> _ = null)
        {
            base.Setup(redmine, _);

            var allItems = redmine.Cache.Projects.Select(p => new FilterItemModel(p)).ToList();
            allItems.Insert(0, ProjectFilterModel.MINE);
            updateAllItems(allItems);
        }
    }
}
