using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace TelerikEx.Helpers
{
    /// <summary>
    /// RadGridView または RadTreeListView の SeletedItems をバインドするためのユーティリティ。
    /// RadGridView と RadTreeListView の両方に対応するため、以下のコードから RadGridView を GridViewDataControl に置き換えた。
    /// https://github.com/telerik/xaml-sdk/blob/master/GridView/BindingSelectedItemsFromViewModel/GridViewSelectionUtilities.cs
    /// https://docs.telerik.com/devtools/wpf/api/telerik.windows.controls.gridview.gridviewdatacontrol
    /// </summary>
    public class GridViewSelectionUtilities
    {
        private static bool isSyncingSelection;
        private static List<Tuple<WeakReference, List<GridViewDataControl>>> collectionToGridViews = new List<Tuple<WeakReference, List<GridViewDataControl>>>();

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(INotifyCollectionChanged),
            typeof(GridViewSelectionUtilities),
            new PropertyMetadata(null, OnSelectedItemsChanged));

        public static INotifyCollectionChanged GetSelectedItems(DependencyObject obj)
        {
            return (INotifyCollectionChanged)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, INotifyCollectionChanged value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }

        private static void OnSelectedItemsChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            var gridView = (GridViewDataControl)target;

            var oldCollection = args.OldValue as INotifyCollectionChanged;
            if (oldCollection != null)
            {
                gridView.SelectionChanged -= GridView_SelectionChanged;
                oldCollection.CollectionChanged -= SelectedItems_CollectionChanged;
                RemoveAssociation(oldCollection, gridView);
            }

            var newCollection = args.NewValue as INotifyCollectionChanged;
            if (newCollection != null)
            {
                gridView.SelectionChanged += GridView_SelectionChanged;
                newCollection.CollectionChanged += SelectedItems_CollectionChanged;
                AddAssociation(newCollection, gridView);
                OnSelectedItemsChanged(newCollection, null, (IList)newCollection);
            }
        }

        private static void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            INotifyCollectionChanged collection = (INotifyCollectionChanged)sender;
            OnSelectedItemsChanged(collection, args.OldItems, args.NewItems);
        }

        private static void GridView_SelectionChanged(object sender, SelectionChangeEventArgs args)
        {
            if (isSyncingSelection)
            {
                return;
            }

            var collection = (IList)GetSelectedItems((GridViewDataControl)sender);
            foreach (object item in args.RemovedItems)
            {
                collection.Remove(item);
            }
            foreach (object item in args.AddedItems)
            {
                collection.Add(item);
            }
        }

        private static void OnSelectedItemsChanged(INotifyCollectionChanged collection, IList oldItems, IList newItems)
        {
            isSyncingSelection = true;

            var gridViews = GetOrCreateGridViews(collection);
            foreach (var gridView in gridViews)
            {
                SyncSelection(gridView, oldItems, newItems);
            }

            isSyncingSelection = false;
        }

        private static void SyncSelection(GridViewDataControl gridView, IList oldItems, IList newItems)
        {
            if (oldItems != null)
            {
                SetItemsSelection(gridView, oldItems, false);
            }

            if (newItems != null)
            {
                SetItemsSelection(gridView, newItems, true);
            }
        }

        private static void SetItemsSelection(GridViewDataControl gridView, IList items, bool shouldSelect)
        {
            foreach (var item in items)
            {
                bool contains = gridView.SelectedItems.Contains(item);
                if (shouldSelect && !contains)
                {
                    gridView.SelectedItems.Add(item);
                }
                else if (contains && !shouldSelect)
                {
                    gridView.SelectedItems.Remove(item);
                }
            }
        }

        private static void AddAssociation(INotifyCollectionChanged collection, GridViewDataControl gridView)
        {
            List<GridViewDataControl> gridViews = GetOrCreateGridViews(collection);
            gridViews.Add(gridView);
        }

        private static void RemoveAssociation(INotifyCollectionChanged collection, GridViewDataControl gridView)
        {
            List<GridViewDataControl> gridViews = GetOrCreateGridViews(collection);
            gridViews.Remove(gridView);

            if (gridViews.Count == 0)
            {
                CleanUp();
            }
        }

        private static List<GridViewDataControl> GetOrCreateGridViews(INotifyCollectionChanged collection)
        {
            for (int i = 0; i < collectionToGridViews.Count; i++)
            {
                var wr = collectionToGridViews[i].Item1;
                if (wr.Target == collection)
                {
                    return collectionToGridViews[i].Item2;
                }
            }

            collectionToGridViews.Add(new Tuple<WeakReference, List<GridViewDataControl>>(new WeakReference(collection), new List<GridViewDataControl>()));
            return collectionToGridViews[collectionToGridViews.Count - 1].Item2;
        }

        private static void CleanUp()
        {
            for (int i = collectionToGridViews.Count - 1; i >= 0; i--)
            {
                bool isAlive = collectionToGridViews[i].Item1.IsAlive;
                var behaviors = collectionToGridViews[i].Item2;
                if (behaviors.Count == 0 || !isAlive)
                {
                    collectionToGridViews.RemoveAt(i);
                }
            }
        }
    }
}
