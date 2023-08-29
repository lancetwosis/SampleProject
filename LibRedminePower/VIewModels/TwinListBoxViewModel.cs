using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LibRedminePower.ViewModels
{
    public class TwinListBoxViewModel<T> : ViewModelBase
    {
        public ObservableCollection<T> FromItems { get; set; }
        public ObservableCollection<T> ToItems { get; set; }

        public string FromFilter { get; set; }
        public ReactiveCommand ClearFromFilterCommand { get; set; }

        public ListCollectionView FromItemsCVS { get; set; }
        public ListCollectionView ToItemsCVS { get; set; }

        public ObservableCollection<T> FromSelectedItems { get; set; }
        public ListCollectionView FromSelectedItemsCVS { get; set; }
        public ObservableCollection<T> ToSelectedItems { get; set; }

        public ReactiveCommand AddFromSelectedItems { get; set; }
        public ReactiveCommand AddAllItems { get; set; }
        public ReactiveCommand RemoveFromSelectedItems { get; set; }
        public ReactiveCommand RemoveAllItems { get; set; }

        public ReactiveCommand MoveTopToSelectedItems { get; set; }
        public ReactiveCommand MoveUpToSelectedItems { get; set; }
        public ReactiveCommand MoveDownToSelectedItems { get; set; }
        public ReactiveCommand MoveBotomToSelectedItems { get; set; }

        public ReactiveProperty<bool> IsEdited { get; set; }

        private List<(T v, int i, bool isFirst, bool isLast)> allItems;

        [Obsolete("Design Only", true)]
        public TwinListBoxViewModel(){}

        public TwinListBoxViewModel(IEnumerable<T> allItems, ObservableCollection<T> selectedItems)
        {
            this.allItems = allItems.Indexed().ToList();

            ClearFromFilterCommand = new ReactiveCommand().WithSubscribe(() => FromFilter = "").AddTo(disposables);

            var isBusy = new BusyNotifier();
            ToItems = selectedItems;
            ToItems.ObserveAddChanged().Where(_ => !isBusy.IsBusy).SubscribeWithErr(a =>
            {
                using (isBusy.ProcessStart())
                {
                    FromItems.Remove(a);
                }
            }).AddTo(disposables);
            ToItems.ObserveRemoveChanged().Where(_ => !isBusy.IsBusy).SubscribeWithErr(a =>
            {
                using (isBusy.ProcessStart())
                {
                    // 初期の並び順と同じところに戻す
                    var added =this.allItems.First(i => i.v.Equals(a));
                    var fromItems =this.allItems.Where(i => FromItems.Contains(i.v)).ToList();
                    fromItems.Add(added);

                    FromItems.Clear();
                    foreach (var sorted in fromItems.OrderBy(i => i.i).Select(i => i.v))
                    {
                        FromItems.Add(sorted);
                    }
                }
            }).AddTo(disposables);

            ToItemsCVS = new ListCollectionView(ToItems);

            FromItems = new ObservableCollection<T>(allItems.Except(selectedItems));
            FromItems.ObserveAddChanged().Where(_ => !isBusy.IsBusy).SubscribeWithErr(a =>
            {
                using (isBusy.ProcessStart())
                {
                    ToItems.Remove(a);
                }
            }).AddTo(disposables);
            FromItems.ObserveRemoveChanged().Where(_ => !isBusy.IsBusy).SubscribeWithErr(a =>
            {
                using (isBusy.ProcessStart())
                {
                    ToItems.Add(a);
                }
            }).AddTo(disposables);

            FromItemsCVS = new ListCollectionView(FromItems)
            {
                Filter = (o) =>
                {
                    if (string.IsNullOrEmpty(FromFilter)) return true;
                    if (o is T target)
                    {
                        if (target.ToString().Contains(FromFilter))
                            return true;
                        else
                            return false;
                    }
                    else
                        return true;
                },
            };
            this.ObserveProperty(a => a.FromFilter).SubscribeWithErr(_ => FromItemsCVS.Refresh()).AddTo(disposables);

            FromSelectedItems = new ObservableCollection<T>();
            FromSelectedItemsCVS = new ListCollectionView(FromSelectedItems);

            ToSelectedItems = new ObservableCollection<T>();

            AddFromSelectedItems = FromSelectedItems.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                FromSelectedItems.ToList().ForEach(a => ToItems.Add(a));
            }).AddTo(disposables);

            AddAllItems = FromItems.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                // FromItems が 0 になっても FromSelectedItems が空にならない問題がある（おそらく telerik:ListBoxSelectedItemsBehavior の問題）
                // 先に Clear しておくとうまく動作したので以下の処理を追加する
                FromSelectedItems.Clear();
                FromItems.ToList().ForEach(a => ToItems.Add(a));
            }).AddTo(disposables);

            RemoveFromSelectedItems = ToSelectedItems.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                ToSelectedItems.ToList().ForEach(a => ToItems.Remove(a));
            }).AddTo(disposables);

            RemoveAllItems = ToItems.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                ToSelectedItems.Clear();
                Enumerable.Range(1, ToItems.Count).ToList().ForEach(_ => ToItems.RemoveAt(0));
            }).AddTo(disposables);

            MoveTopToSelectedItems = ToSelectedItems.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                var temp = ToSelectedItems.ToList();
                ToSelectedItems.Select(a => ToItems.IndexOf(a)).OrderBy(a => a).Indexed().ToList().ForEach(a => ToItems.Move(a.v, a.i));
                ToSelectedItems.Clear();
                temp.ForEach(a => ToSelectedItems.Add(a));
            }).AddTo(disposables);

            MoveUpToSelectedItems = ToSelectedItems.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                var temp = ToSelectedItems.ToList();
                ToSelectedItems.Select(a => ToItems.IndexOf(a)).OrderBy(a => a).Indexed().Where(a => a.v >= 1).ToList().ForEach(a => ToItems.Move(a.v, a.v -1));
                ToSelectedItems.Clear();
                temp.ForEach(a => ToSelectedItems.Add(a));
            }).AddTo(disposables);

            MoveDownToSelectedItems = ToSelectedItems.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                var temp = ToSelectedItems.ToList();
                ToSelectedItems.Select(a => ToItems.IndexOf(a)).OrderByDescending(a => a).Indexed().Where(a => a.v < ToItems.Count()-1).ToList().ForEach(a => ToItems.Move(a.v, a.v + 1));
                ToSelectedItems.Clear();
                temp.ForEach(a => ToSelectedItems.Add(a));
            }).AddTo(disposables);

            MoveBotomToSelectedItems = ToSelectedItems.AnyAsObservable().ToReactiveCommand().WithSubscribe(() =>
            {
                var temp = ToSelectedItems.ToList();
                ToSelectedItems.Select(a => ToItems.IndexOf(a)).OrderByDescending(a => a).Indexed().ToList().ForEach(a => ToItems.Move(a.v, ToItems.Count() - a.i -1));
                ToSelectedItems.Clear();
                temp.ForEach(a => ToSelectedItems.Add(a));
            }).AddTo(disposables);

            // 変更を刈り取る。
            IsEdited = new[]
            {
                AddFromSelectedItems.Select(_ => true),
                RemoveFromSelectedItems.Select(_ => true),
                MoveTopToSelectedItems.Select(_ => true),
                MoveUpToSelectedItems.Select(_ => true),
                MoveDownToSelectedItems.Select(_ => true),
                MoveBotomToSelectedItems.Select(_ => true),
            }.Merge().Select(_ => true).ToReactiveProperty().AddTo(disposables);
        }
    }
}
