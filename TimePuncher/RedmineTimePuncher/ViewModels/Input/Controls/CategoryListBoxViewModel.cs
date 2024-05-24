using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels.Input.Controls
{
    public class CategoryListBoxViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public List<MyCategory> Items { get; set; }
        public ReactivePropertySlim<List<CategorySettingModel>> AllSettings { get; set; }
        public ReadOnlyReactivePropertySlim<ICollectionView> View { get; set; }
        public ReactivePropertySlim<string> SearchText { get; set; }
        public ReactiveCommand ClearSearchText { get; set; }

        private List<string> pinedCategoryNames;

        public CategoryListBoxViewModel(InputViewModel parent)
        {
            Items = new List<MyCategory>();
            try
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.PinedCategoryNames))
                    pinedCategoryNames = Properties.Settings.Default.PinedCategoryNames.Split(',').ToList();
                else
                    pinedCategoryNames = new List<string>();
            }
            catch
            {
                pinedCategoryNames = new List<string>();
            }

            AllSettings = new ReactivePropertySlim<List<CategorySettingModel>>().AddTo(disposables);
            View = this.ObserveProperty(a => a.Items).Select(a =>
            {
                var view = CollectionViewSource.GetDefaultView(Items);
                view.Filter = x =>
                {
                    if (string.IsNullOrEmpty(SearchText.Value)) return true;
                    if (x is MyCategory item)
                        return SearchPatern.Check(SearchText.Value, item.DisplayName);
                    return false;
                };
                return view;
            }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            SearchText = new ReactivePropertySlim<string>().AddTo(disposables);
            ClearSearchText = SearchText.Select(a => !string.IsNullOrEmpty(a)).ToReactiveCommand().WithSubscribe(() =>
            {
                SearchText.Value = null;
            }).AddTo(disposables);
            SearchText.SubscribeWithErr(_ => View.Value.Refresh()).AddTo(disposables);

            parent.Parent.Redmine.CombineLatest(AllSettings, (r, allCates) => (r, allCates)).SubscribeWithErr(p =>
            {
                // Redmine や設定が更新された場合は必ず update する
                update(p.r, p.allCates, parent.SelectedAppointments.Value);
            }).AddTo(disposables);
            parent.SelectedAppointments.SubscribeWithErr(a =>
            {
                // 選択が変更された場合は更新中でない場合のみ update する。
                if (parent.NowUpdating != null && !parent.NowUpdating.Value)
                    update(parent.Parent.Redmine.Value, AllSettings.Value, a);
            }).AddTo(disposables);

            CompositeDisposable myDisposable = null;
            parent.SelectedAppointments.Where(a => a != null).SubscribeWithErr(appointments =>
            {
                myDisposable?.Dispose();
                myDisposable = new CompositeDisposable().AddTo(disposables);

                foreach (var apo in appointments)
                {
                    apo.ObserveProperty(a => a.Ticket).SubscribeWithErr(_ =>
                    {
                        if (parent.NowUpdating != null && !parent.NowUpdating.Value)
                            update(parent.Parent.Redmine.Value, AllSettings.Value, appointments);
                    }).AddTo(myDisposable);
                }
            }).AddTo(disposables);
        }

        private CompositeDisposable onUpdateDisposable = null;
        private void update(RedmineManager redmine, List<CategorySettingModel> allSettings, List<MyAppointment> selectedAppos)
        {
            if (redmine == null || allSettings == null || selectedAppos == null)
                return;

            allSettings = allSettings.OrderByDescending(c => this.pinedCategoryNames.Contains(c.Name)).ThenBy(c => c.Order).ToList();

            onUpdateDisposable?.Dispose();
            onUpdateDisposable = new CompositeDisposable().AddTo(disposables);
            var pinCommand = new ReactiveCommand<RadToggleButton>().WithSubscribe(tb =>
            {
                var self = tb.DataContext as MyCategory;
                self.IsPined = tb.IsChecked.Value;
                if (tb.IsChecked.Value)
                    this.pinedCategoryNames.Add(self.DisplayName);
                else
                    this.pinedCategoryNames.Remove(self.DisplayName);

                Items = Items.OrderByDescending(c => this.pinedCategoryNames.Contains(c.DisplayName)).ThenBy(c => c.Model.Order).ToList();
            }).AddTo(onUpdateDisposable);

            var appointments = selectedAppos.Where(a => a.IsMyWork.Value && a.Ticket != null).ToList();
            if (redmine == null || !appointments.Any())
            {
                Items = allSettings.Select(c => new MyCategory(c, this.pinedCategoryNames.Contains(c.Name), pinCommand)).ToList();
                return;
            }

            var projects = redmine.Projects.Value;
            var notAsigned = appointments.Select(a => a.Ticket).FirstOrDefault(t => !projects.Any(pro => pro.Id == t.Project.Id));
            if (notAsigned != null)
            {
                Items = new List<MyCategory>();
                throw new ApplicationException(string.Format(Properties.Resources.msgErrFailedFindProject, $"#{notAsigned.Id} {notAsigned.Subject}"));
            }

            Items = allSettings
                .Where(c =>
                {
                    if (!c.TargetTrackers.Any())
                        return true;

                    // それぞれの予定の親チケットを含むチケットのリストを取り出して、
                    return appointments.Select(a => a.TicketTree.Items.Select(t => t.Issue).ToList())
                    // リストに対象トラッカーと等しいトラッカーを持つチケットがあった場合のみ、追加する
                    .Any(ts => ts.Any(t => c.IsTergetTracker(t.Tracker)));
                })
                // その作業分類が、選択されたいずれかのチケットのプロジェクトで有効になっていた場合のみ、追加する
                .Where(c => appointments.Select(a => a.Ticket).Any(ticket =>
                {
                    // Project.TimeEntryActivities には有効な EntryActivity のみが設定されている
                    // 各プロジェクトで「有効」を切り替えると選択肢の値で設定した作業分類とは別物として扱われる
                    // その結果、異なる Id を持つものとなるため名称（Name）で判定する
                    return projects.First(pro => pro.Id == ticket.Project.Id).TimeEntryActivities.Any(t => t.Name == c.Name);
                }))
                .Select(c => new MyCategory(c, this.pinedCategoryNames.Contains(c.Name), pinCommand))
                .ToList();
        }

        public void SavePinedCategories()
        {
            Properties.Settings.Default.PinedCategoryNames = string.Join(",", this.pinedCategoryNames);
        }
    }
}
