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
    /// RadCalendar の SelectedDates をバインドするためのユーティリティ。
    /// RadGridView の SelectedItems から流用して、RadGridView を RadCalendar に置き換えた。
    /// https://github.com/telerik/xaml-sdk/blob/master/GridView/BindingSelectedItemsFromViewModel/GridViewSelectionUtilities.cs
    /// 
    /// SelectedDates が IList で追加に対応していないため、VM側から RadCalendar の SelectedDates を制御することはできない。
    /// VM側から追加や削除を行おうとすると必ず NotSupportedException になる。
    /// 
    /// より良い対応があるかもしれないが、現象が明らかなためこのままとする。
    /// </summary>
    public class CalendarSelectionUtilities
    {
        private static bool isSyncingSelection;
        private static List<Tuple<WeakReference, List<RadCalendar>>> collectionToCalendar = new List<Tuple<WeakReference, List<RadCalendar>>>();

        public static readonly DependencyProperty SelectedDatesProperty = DependencyProperty.RegisterAttached(
            "SelectedDates",
            typeof(INotifyCollectionChanged),
            typeof(CalendarSelectionUtilities),
            new PropertyMetadata(null, OnSelectedDatesChanged));

        public static INotifyCollectionChanged GetSelectedDates(DependencyObject obj)
        {
            return (INotifyCollectionChanged)obj.GetValue(SelectedDatesProperty);
        }

        public static void SetSelectedDates(DependencyObject obj, INotifyCollectionChanged value)
        {
            obj.SetValue(SelectedDatesProperty, value);
        }

        private static void OnSelectedDatesChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            var calendar = (RadCalendar)target;

            var oldCollection = args.OldValue as INotifyCollectionChanged;
            if (oldCollection != null)
            {
                calendar.SelectionChanged -= Calendar_SelectionChanged;
                oldCollection.CollectionChanged -= SelectedDates_CollectionChanged;
                RemoveAssociation(oldCollection, calendar);
            }

            var newCollection = args.NewValue as INotifyCollectionChanged;
            if (newCollection != null)
            {
                calendar.SelectionChanged += Calendar_SelectionChanged;

                newCollection.CollectionChanged += SelectedDates_CollectionChanged;
                AddAssociation(newCollection, calendar);
                OnSelectedDatesChanged(newCollection, null, (IList)newCollection);
            }
        }

        private static void SelectedDates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            INotifyCollectionChanged collection = (INotifyCollectionChanged)sender;
            OnSelectedDatesChanged(collection, args.OldItems, args.NewItems);
        }

        private static void Calendar_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs args)
        {
            if (isSyncingSelection)
            {
                return;
            }
            
            var collection = (IList)GetSelectedDates((RadCalendar)sender);
            foreach (object item in args.RemovedItems)
            {
                collection.Remove(item);
            }
            foreach (object item in args.AddedItems)
            {
                collection.Add(item);
            }
        }

        private static void OnSelectedDatesChanged(INotifyCollectionChanged collection, IList oldItems, IList newItems)
        {
            isSyncingSelection = true;

            var calendars = GetOrCreateCalendar(collection);
            foreach (var calendar in calendars)
            {
                SyncSelection(calendar, oldItems, newItems);
            }

            isSyncingSelection = false;
        }

        private static void SyncSelection(RadCalendar calendar, IList oldItems, IList newItems)
        {
            if (oldItems != null)
            {
                SetItemsSelection(calendar, oldItems, false);
            }

            if (newItems != null)
            {
                SetItemsSelection(calendar, newItems, true);
            }
        }

        private static void SetItemsSelection(RadCalendar calendar, IList items, bool shouldSelect)
        {
            foreach (var item in items)
            {
                bool contains = calendar.SelectedDates.Contains((DateTime)item);
                if (shouldSelect && !contains)
                {
                    calendar.SelectedDates.Add((DateTime)item);
                }
                else if (contains && !shouldSelect)
                {
                    calendar.SelectedDates.Remove((DateTime)item);
                }
            }
        }

        private static void AddAssociation(INotifyCollectionChanged collection, RadCalendar calendar)
        {
            List<RadCalendar> calendars = GetOrCreateCalendar(collection);
            calendars.Add(calendar);
        }

        private static void RemoveAssociation(INotifyCollectionChanged collection, RadCalendar calendar)
        {
            List<RadCalendar> calendars = GetOrCreateCalendar(collection);
            calendars.Remove(calendar);

            if (calendars.Count == 0)
            {
                CleanUp();
            }
        }

        private static List<RadCalendar> GetOrCreateCalendar(INotifyCollectionChanged collection)
        {
            for (int i = 0; i < collectionToCalendar.Count; i++)
            {
                var wr = collectionToCalendar[i].Item1;
                if (wr.Target == collection)
                {
                    return collectionToCalendar[i].Item2;
                }
            }

            collectionToCalendar.Add(new Tuple<WeakReference, List<RadCalendar>>(new WeakReference(collection), new List<RadCalendar>()));
            return collectionToCalendar[collectionToCalendar.Count - 1].Item2;
        }

        private static void CleanUp()
        {
            for (int i = collectionToCalendar.Count - 1; i >= 0; i--)
            {
                bool isAlive = collectionToCalendar[i].Item1.IsAlive;
                var behaviors = collectionToCalendar[i].Item2;
                if (behaviors.Count == 0 || !isAlive)
                {
                    collectionToCalendar.RemoveAt(i);
                }
            }
        }
    }
}
