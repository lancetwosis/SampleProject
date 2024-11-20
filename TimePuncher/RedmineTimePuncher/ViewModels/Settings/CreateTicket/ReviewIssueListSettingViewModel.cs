using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.ViewModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.CreateTicket;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings.CreateTicket
{
    public class ReviewIssueListSettingViewModel : Bases.SettingsViewModelBase<ReviewIssueListSettingModel>
    {
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public ReactivePropertySlim<bool> ShowDescription { get; set; }
        public ReactivePropertySlim<bool> ShowLastNote { get; set; }

        public ReadOnlyReactivePropertySlim<TwinListBoxViewModel<IssueProperty>> Properties { get; set; }

        public ReactivePropertySlim<IssueProperty> SortBy { get; set; }
        public ReadOnlyReactivePropertySlim<bool> IsEnabledSorting { get; set; }
        public ReactivePropertySlim<bool> IsDESC { get; set; }
        public ReactivePropertySlim<IssueProperty> GroupBy { get; set; }

        public ReadOnlyReactivePropertySlim<List<IssueProperty>> AllProperties { get; set; }
        public ReadOnlyReactivePropertySlim<List<IssueProperty>> CanGroupByProperties { get; set; }
        public ReadOnlyReactivePropertySlim<List<IssueProperty>> CanSortByProperties { get; set; }

        public ReviewIssueListSettingViewModel(ReviewIssueListSettingModel model) : base(model)
        {
            ErrorMessage = CacheTempManager.Default.Message.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            AllProperties = CacheTempManager.Default.MyCustomFields.Where(a => a != null).Select(a => {
                return new[] {
                    new IssueProperty(IssuePropertyType.Status),
                    new IssueProperty(IssuePropertyType.Priority),
                    new IssueProperty(IssuePropertyType.Subject),
                    new IssueProperty(IssuePropertyType.AssignedTo),
                    new IssueProperty(IssuePropertyType.FixedVersion),
                    new IssueProperty(IssuePropertyType.Updated),
                    new IssueProperty(IssuePropertyType.Author),
                    new IssueProperty(IssuePropertyType.Category),
                    new IssueProperty(IssuePropertyType.StartDate),
                    new IssueProperty(IssuePropertyType.DueDate),
                    new IssueProperty(IssuePropertyType.DoneRatio),
                }.Concat(a.Select(b => new IssueProperty(b))).ToList();
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            AllProperties.Where(a => a != null).Subscribe(a =>
            {
                var notExists = model.SelectedProperties.Where(p => !a.Contains(p)).ToList();
                foreach (var i in notExists)
                {
                    model.SelectedProperties.Remove(i);
                }
            }).AddTo(disposables);

            CanGroupByProperties = CacheTempManager.Default.MyCustomFields.Where(a => a != null).Select(a => {
                return new[] {
                    IssueProperty.NOT_SPECIFIED,
                    new IssueProperty(IssuePropertyType.Status),
                    new IssueProperty(IssuePropertyType.Priority),
                    new IssueProperty(IssuePropertyType.AssignedTo),
                    new IssueProperty(IssuePropertyType.FixedVersion),
                    new IssueProperty(IssuePropertyType.Updated),
                    new IssueProperty(IssuePropertyType.Author),
                    new IssueProperty(IssuePropertyType.Category),
                    new IssueProperty(IssuePropertyType.StartDate),
                    new IssueProperty(IssuePropertyType.DueDate),
                    new IssueProperty(IssuePropertyType.DoneRatio),
                }.Concat(a.Where(b => b.CanGroupBy()).Select(c => new IssueProperty(c))).ToList();
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            CanGroupByProperties.Where(a => a != null).Subscribe(a =>
            {
                if (!a.Contains(model.GroupBy))
                    model.GroupBy = IssueProperty.NOT_SPECIFIED;
            }).AddTo(disposables);

            CanSortByProperties = model.SelectedProperties.CollectionChangedAsObservable().StartWithDefault().Select(_ =>
            {
                return new[] {
                    IssueProperty.NOT_SPECIFIED
                }.Concat(model.SelectedProperties).ToList();
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            CanSortByProperties.Where(a => a != null).Subscribe(a =>
            {
                if (!a.Contains(model.SortBy))
                    model.SortBy = IssueProperty.NOT_SPECIFIED;
            }).AddTo(disposables);

            ShowDescription = model.ToReactivePropertySlimAsSynchronized(m => m.ShowDescription).AddTo(disposables);
            ShowLastNote = model.ToReactivePropertySlimAsSynchronized(m => m.ShowLastNote).AddTo(disposables);
            Properties = 
                AllProperties.Where(a => a != null)
                .CombineLatest(model.ObserveProperty(a => a.SelectedProperties), 
                (all, sel) => new TwinListBoxViewModel<IssueProperty>(all, sel)).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);

            SortBy = model.ToReactivePropertySlimAsSynchronized(m => m.SortBy).AddTo(disposables);
            IsEnabledSorting = SortBy.Select(a => a != null && !a.Equals(IssueProperty.NOT_SPECIFIED)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            IsDESC = model.ToReactivePropertySlimAsSynchronized(m => m.IsDESC).AddTo(disposables);
            GroupBy = model.ToReactivePropertySlimAsSynchronized(m => m.GroupBy).AddTo(disposables);
        }
    }
}
