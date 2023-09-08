using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;

namespace TelerikEx.Helpers
{
    /// <summary>
    /// RadTreeMap の SelectedItems をバインドするためのユーティリティ。
    /// RadGridView の SelectedItems から流用して、RadGridView を RadTreeMap に置き換えた。
    /// https://github.com/telerik/xaml-sdk/blob/master/GridView/BindingSelectedItemsFromViewModel/GridViewSelectionUtilities.cs
    /// 
    /// SelectedItems が IList で追加に対応していないため、VM側から RadTreeMap の SelectedItems を制御することはできない。
    /// VM側から追加や削除を行おうとすると必ず NotSupportedException になる。
    /// 
    /// より良い対応があるかもしれないが、現象が明らかなためこのままとする。
    /// </summary>
    public class TreeMapSelectionUtilities
    {
        private static bool isSyncingSelection;
        private static List<Tuple<WeakReference, List<RadTreeMap>>> collectionToTreeMap = new List<Tuple<WeakReference, List<RadTreeMap>>>();

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(INotifyCollectionChanged),
            typeof(TreeMapSelectionUtilities),
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
            var treeMap = (RadTreeMap)target;

            var oldCollection = args.OldValue as INotifyCollectionChanged;
            if (oldCollection != null)
            {
                treeMap.SelectionChanged -= TreeMap_SelectionChanged;
                oldCollection.CollectionChanged -= SelectedItems_CollectionChanged;
                RemoveAssociation(oldCollection, treeMap);
            }

            var newCollection = args.NewValue as INotifyCollectionChanged;
            if (newCollection != null)
            {
                treeMap.SelectionChanged += TreeMap_SelectionChanged;

                newCollection.CollectionChanged += SelectedItems_CollectionChanged;
                AddAssociation(newCollection, treeMap);
                OnSelectedItemsChanged(newCollection, null, (IList)newCollection);
            }
        }

        private static void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            INotifyCollectionChanged collection = (INotifyCollectionChanged)sender;
            OnSelectedItemsChanged(collection, args.OldItems, args.NewItems);
        }

        private static void TreeMap_SelectionChanged(object sender, Telerik.Windows.Controls.TreeMap.SelectionChangedRoutedEventArgs args)
        {
            if (isSyncingSelection)
            {
                return;
            }
            
            var collection = (IList)GetSelectedItems((RadTreeMap)sender);
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

            var treeMaps = GetOrCreateTreeMap(collection);
            foreach (var treeMap in treeMaps)
            {
                SyncSelection(treeMap, oldItems, newItems);
            }

            isSyncingSelection = false;
        }

        private static void SyncSelection(RadTreeMap treeMap, IList oldItems, IList newItems)
        {
            if (oldItems != null)
            {
                SetItemsSelection(treeMap, oldItems, false);
            }

            if (newItems != null)
            {
                SetItemsSelection(treeMap, newItems, true);
            }
        }

        private static void SetItemsSelection(RadTreeMap treeMap, IList items, bool shouldSelect)
        {
            foreach (var item in items)
            {
                bool contains = treeMap.SelectedItems.Contains(item);
                if (shouldSelect && !contains)
                {
                    treeMap.SelectedItems.Add(item);
                }
                else if (contains && !shouldSelect)
                {
                    treeMap.SelectedItems.Remove(item);
                }
            }
        }

        private static void AddAssociation(INotifyCollectionChanged collection, RadTreeMap treeMap)
        {
            List<RadTreeMap> treeMaps = GetOrCreateTreeMap(collection);
            treeMaps.Add(treeMap);
        }

        private static void RemoveAssociation(INotifyCollectionChanged collection, RadTreeMap treeMap)
        {
            List<RadTreeMap> treeMaps = GetOrCreateTreeMap(collection);
            treeMaps.Remove(treeMap);

            if (treeMaps.Count == 0)
            {
                CleanUp();
            }
        }

        private static List<RadTreeMap> GetOrCreateTreeMap(INotifyCollectionChanged collection)
        {
            for (int i = 0; i < collectionToTreeMap.Count; i++)
            {
                var wr = collectionToTreeMap[i].Item1;
                if (wr.Target == collection)
                {
                    return collectionToTreeMap[i].Item2;
                }
            }

            collectionToTreeMap.Add(new Tuple<WeakReference, List<RadTreeMap>>(new WeakReference(collection), new List<RadTreeMap>()));
            return collectionToTreeMap[collectionToTreeMap.Count - 1].Item2;
        }

        private static void CleanUp()
        {
            for (int i = collectionToTreeMap.Count - 1; i >= 0; i--)
            {
                bool isAlive = collectionToTreeMap[i].Item1.IsAlive;
                var behaviors = collectionToTreeMap[i].Item2;
                if (behaviors.Count == 0 || !isAlive)
                {
                    collectionToTreeMap.RemoveAt(i);
                }
            }
        }
    }
}
