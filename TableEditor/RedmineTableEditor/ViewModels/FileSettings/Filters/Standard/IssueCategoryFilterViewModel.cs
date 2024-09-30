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
    public class IssueCategoryFilterViewModel : ItemsFilterViewModelBase<IssueCategoryFilterModel>
    {
        private ReactivePropertySlim<string> errorMsg { get; set; }

        public IssueCategoryFilterViewModel(IssueCategoryFilterModel model) : base(model)
        {
            errorMsg = new ReactivePropertySlim<string>().AddTo(disposables);
            ErrorMessage = NeedsFilter.CombineLatest(errorMsg, ErrorMessage, (needsFilter, e1, e2) =>
            {
                return needsFilter ? (e1 ?? e2) : null;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public override void Setup(RedmineManager redmine, ReadOnlyReactivePropertySlim<List<FilterItemModel>> projects = null)
        {
            base.Setup(redmine, projects);

            projects.SubscribeWithErr(ps =>
            {
                if (ps == null || ps.Count > 1)
                {
                    errorMsg.Value = Properties.Resources.FilterErrMsgSingleProjectForCategory;
                    updateAllItems(new List<FilterItemModel>());
                }
                else
                {
                    var proj = redmine.Cache.Projects.First(p => p.Id.ToString() == ps[0].Id);
                    if (proj.IssueCategories == null || proj.IssueCategories.Count == 0)
                    {
                        errorMsg.Value = string.Format(Properties.Resources.FilterErrMsgNoCategories, proj.Name);
                        updateAllItems(new List<FilterItemModel>());
                    }
                    else
                    {
                        errorMsg.Value = null;
                        updateAllItems(proj.IssueCategories.Select(c => new FilterItemModel(c)).ToList());
                    }
                }
            }).AddTo(myDisposables);
        }
    }
}
