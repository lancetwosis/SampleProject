using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using NetOffice.OutlookApi.Enums;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Visualize;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Visualize;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTimePuncher.ViewModels.Visualize.Filters
{
    public class SpecifyProjectsViewModel : FilterGroupViewModelBase
    {
        public ExpandableTwinListBoxViewModel<MyProject> Projects { get; set; }

        // https://www.colordic.org/colorsample/fff1e6
        public SpecifyProjectsViewModel(TicketFiltersModel model, ReactivePropertySlim<RedmineManager> redmine)
            : base(model.ToReactivePropertySlimAsSynchronized(a => a.SpecifyProjects), Color.FromRgb(0xFF, 0xF1, 0xE6))
        {
            CompositeDisposable myDisposables = null;
            redmine.Where(r => r != null).Subscribe(r =>
            {
                myDisposables?.Dispose();
                myDisposables = new CompositeDisposable().AddTo(disposables);

                var allProjects = r.Projects.Value.Select(p => new MyProject(p)).ToList();
                var selectedProjects = model.Projects;

                if (model.Projects.Count == 0)
                {
                    foreach (var m in r.MyUser.Memberships)
                    {
                        selectedProjects.Add(allProjects.First(p => p.Id == m.Project.Id));
                    }
                }

                Projects = new ExpandableTwinListBoxViewModel<MyProject>(allProjects, selectedProjects).AddTo(myDisposables);
                IsEnabled.Skip(1).Subscribe(i =>
                {
                    Projects.Expanded = i;
                    if (i && model.Projects.Count == 0)
                    {
                        foreach (var m in r.MyUser.Memberships)
                        {
                            selectedProjects.Add(allProjects.First(p => p.Id == m.Project.Id));
                        }
                    }
                }).AddTo(myDisposables);
            });

            var projectsChanged = model.Projects.CollectionChangedAsObservable().StartWithDefault();
            IsValid = projectsChanged.Select(_ =>
            {
                if (model.Projects.Any())
                    return null;
                else
                    return "プロジェクトを選択してください。";
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Label = IsEnabled.CombineLatest(IsValid, projectsChanged, (_1, _2, _3) => true).Select(_ =>
            {
                if (!IsEnabled.Value)
                    return $"プロジェクト: 指定なし";
                else if (IsValid.Value != null)
                    return $"プロジェクト: {NAN}";

                if (model.Projects.Count <= 3)
                    return $"{string.Join(", ", model.Projects)}";
                else
                    return $"{string.Join(", ", model.Projects.Take(3))}, ...";
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Tooltip = IsEnabled.CombineLatest(IsValid, projectsChanged, (_1, _2, _3) => true).Select(_ =>
            {
                if (!IsEnabled.Value)
                    return "親チケットのプロジェクトもしくはあなたにアサインされているプロジェクトが対象となります";
                else if (IsValid.Value != null)
                    return null;
                else
                    return $"{string.Join(Environment.NewLine, model.Projects)}";
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
