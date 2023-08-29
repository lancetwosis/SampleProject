using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.ViewModels
{
    public class EditableGridViewModel<T1> : EditableGridViewModel<T1, T1>, IDisposable where T1 : IDisposable, new()
    {
        public EditableGridViewModel(ObservableCollection<T1> source)
            :base (source, a => a, a => a)
        {
        }

        /// <summary>
        /// リストへの追加や削除が行われない場合に使用
        /// </summary>
        public EditableGridViewModel(List<T1> source)
            :this (new ObservableCollection<T1>(source))
        {
            CanInsert = false;
            CanDelete = false;
        }
    }

    public class EditableGridViewModel<T1, T2> : ObservableCollectionSync<T1, T2> where T1 : IDisposable, new()
    {
        public ReactivePropertySlim<T1> SelectedItem { get; set; }
        public bool CanInsert { get; set; } = true;
        public ReactiveCommand InsertCommand { get; set; }
        public bool CanDelete { get; set; } = true;
        public ReactiveCommand DeleteCommand { get; set; }
        public ReactiveCommand MoveUpCommand { get; set; }
        public ReactiveCommand MoveDownCommand { get; set; }

        private CompositeDisposable disposables = new CompositeDisposable();

        public EditableGridViewModel(ObservableCollection<T2> source, Func<T2, T1> convert, Func<T1, T2> convertBack)
            : base(source, convert, convertBack)
        {
            SelectedItem = new ReactivePropertySlim<T1>().AddTo(disposables);

            InsertCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                this.Add(new T1().AddTo(disposables));
                SelectedItem.Value = this.Last();
            }).AddTo(disposables);

            DeleteCommand = SelectedItem.Select(a => a != null).ToReactiveCommand().WithSubscribe(() =>
            {
                this.Remove(SelectedItem.Value);
            }).AddTo(disposables);

            MoveUpCommand = SelectedItem.Select(a => a != null).ToReactiveCommand().WithSubscribe(() =>
            {
                var preIndex = Items.IndexOf(SelectedItem.Value);
                if (preIndex <= 0)
                    return;

                var item = Items[preIndex];
                Items.RemoveAt(preIndex);
                Items.Insert(preIndex - 1, item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                SelectedItem.Value = item;
            }).AddTo(disposables);

            MoveDownCommand = SelectedItem.Select(a => a != null).ToReactiveCommand().WithSubscribe(() =>
            {
                var preIndex = Items.IndexOf(SelectedItem.Value);
                if (preIndex < 0 || preIndex == (Items.Count - 1))
                    return;

                var item = Items[preIndex];
                Items.RemoveAt(preIndex);
                Items.Insert(preIndex + 1, item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                SelectedItem.Value = item;
            }).AddTo(disposables);
        }

        public override void Dispose()
        {
            base.Dispose();

            disposables.Dispose();
        }
    }
}
