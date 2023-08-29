using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ObservableCollectionSync
{

    public class ObservableCollectionSync<TViewModel, TModel> : ObservableCollection<TViewModel>, IDisposable
    {
        private ObservableCollection<TModel> sources;
        private Func<TModel, TViewModel> sourceToTarget;
        private Func<TViewModel, TModel>? targetToSource;

        public ObservableCollectionSync(ObservableCollection<TModel> sources, Func<TModel, TViewModel> sourceToTarget, Func<TViewModel, TModel>? targetToSource = null)
            : base(sources.Select(model => sourceToTarget(model)))
        {
            this.sources = sources;
            this.sourceToTarget = sourceToTarget;
            this.targetToSource = targetToSource;

            sources.CollectionChanged += syncSourceToTarget;
            this.CollectionChanged += syncTargetToSource;
        }
        private void syncSourceToTarget(object sender, NotifyCollectionChangedEventArgs e)
        {
            excuteIfNotChanging(() => syncByChangedEventArgs(sources, this, sourceToTarget, e));
        }
        private void syncTargetToSource(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(targetToSource != null)
                excuteIfNotChanging(() => syncByChangedEventArgs(this, sources, targetToSource, e));
        }

        //変更イベントループしてしまわないように、ローカル変数(isChanging)でチェック
        //ローカル変数(isChanging)にアクセスするため、ローカル関数で記述
        private bool isChanging;
        private void excuteIfNotChanging(Action action)
        {
            if (isChanging) return;
            isChanging = true;
            action.Invoke();
            isChanging = false;
        }

        private void syncByChangedEventArgs<OriginT, DestT>(ObservableCollection<OriginT> origin, ObservableCollection<DestT> dest,
        Func<OriginT, DestT> originToDest, NotifyCollectionChangedEventArgs originE)
        {
            switch (originE.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (originE.NewItems?[0] is OriginT addItem)
                        dest.Insert(originE.NewStartingIndex, originToDest(addItem));
                    return;

                case NotifyCollectionChangedAction.Remove:
                    if (originE.OldStartingIndex >= 0)
                        dest.RemoveAt(originE.OldStartingIndex);
                    return;

                case NotifyCollectionChangedAction.Replace:
                    if (originE.NewItems?[0] is OriginT replaceItem)
                        dest[originE.NewStartingIndex] = originToDest(replaceItem);
                    return;

                case NotifyCollectionChangedAction.Move:
                    dest.Move(originE.OldStartingIndex, originE.NewStartingIndex);
                    return;

                case NotifyCollectionChangedAction.Reset:
                    dest.Clear();
                    foreach (DestT item in origin.Select(originToDest))
                        dest.Add(item);
                    return;
            }
        }

        // EditableGridViewModel で override するために virtual に変更
        public virtual void Dispose()
        {
            sources.CollectionChanged -= syncSourceToTarget;
            this.CollectionChanged -= syncTargetToSource;
        }
    }
}