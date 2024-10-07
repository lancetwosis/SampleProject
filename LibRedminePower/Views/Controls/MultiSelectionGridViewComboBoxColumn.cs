using Redmine.Net.Api.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace LibRedminePower.Views.Controls
{
    public class MultiSelectionGridViewComboBoxColumn : GridViewDataColumn
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource",
                typeof(IEnumerable),
                typeof(MultiSelectionGridViewComboBoxColumn),
                null);

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath",
                typeof(string),
                typeof(MultiSelectionGridViewComboBoxColumn),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath",
                typeof(string),
                typeof(MultiSelectionGridViewComboBoxColumn),
                new PropertyMetadata(string.Empty));

        public IEnumerable ItemsSource
        {
            get
            {
                return (IEnumerable)this.GetValue(ItemsSourceProperty);
            }
            set
            {
                this.SetValue(ItemsSourceProperty, value);
            }
        }

        public string DisplayMemberPath
        {
            get
            {
                return (string)this.GetValue(DisplayMemberPathProperty);
            }
            set
            {
                this.SetValue(DisplayMemberPathProperty, value);
            }
        }

        public string SelectedValuePath
        {
            get
            {
                return (string)this.GetValue(SelectedValuePathProperty);
            }
            set
            {
                this.SetValue(SelectedValuePathProperty, value);
            }
        }

        public override FrameworkElement CreateCellElement(GridViewCell cell, object dataItem)
        {
            var displayTextBlock = new TextBlock();
            displayTextBlock.Text = Properties.Resources.SettingsNotSpecified;
            var items = base.GetCellContent(dataItem) as ICollection;
            if (items == null || items.Count == 0 || ItemsSource == null)
            {
                return displayTextBlock;
            }

            var myType = ItemsSource.GetType().GetGenericArguments().First();
            var prop_V = myType.GetProperty(SelectedValuePath);
            var prop_D = myType.GetProperty(DisplayMemberPath);

            displayTextBlock.Text = null;
            foreach (var item in items)
            {
                if (prop_V != null)
                {
                    foreach (var sItem in ItemsSource)
                    {
                        if (prop_V.GetValue(sItem).ToString() == item.ToString())
                        {
                            if (string.IsNullOrEmpty(displayTextBlock.Text))
                            {
                                displayTextBlock.Text = prop_D.GetValue(sItem).ToString();
                            }
                            else
                            {
                                displayTextBlock.Text += ", " + prop_D.GetValue(sItem).ToString();
                            }
                            break;
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(displayTextBlock.Text))
                    {
                        displayTextBlock.Text = prop_D.GetValue(item).ToString();
                    }
                    else
                    {
                        displayTextBlock.Text += ", " + prop_D.GetValue(item).ToString();
                    }
                }
            }
            return displayTextBlock;
        }

        private object toSelectedValue(object value)
        {
            if(string.IsNullOrEmpty(SelectedValuePath))
            {
                return value;
            }
            else
            {
                var myType = ItemsSource.GetType().GetGenericArguments().First();
                var prop_V = myType.GetProperty(SelectedValuePath);
                return prop_V.GetValue(value);
            }
        }

        public override FrameworkElement CreateCellEditElement(GridViewCell cell, object dataItem)
        {
            var comboBox = new RadComboBox() { AllowMultipleSelection = true };
            this.InitializeEditorProperties(comboBox);

            if (this.DataMemberBinding != null)
            {
                var items = base.GetCellContent(dataItem) as IList;

                if (items != null)
                {
                    var selectionChanged = new SelectionChangedEventHandler((s, e) =>
                    {
                        if (e.AddedItems != null && e.AddedItems.Count != 0 && !items.Contains(toSelectedValue(e.AddedItems[0])))
                        {
                            items.Add(toSelectedValue(e.AddedItems[0]));
                        }
                        else if (e.RemovedItems != null && e.RemovedItems.Count != 0)
                        {
                            items.Remove(toSelectedValue(e.RemovedItems[0]));
                        }
                    });

                    comboBox.SelectionChanged += selectionChanged;

                    comboBox.Unloaded += (s, e) =>
                    {
                        comboBox.SelectionChanged -= selectionChanged;
                    };

                    if (items.Count > 0)
                    {
                        var itemsSource = ItemsSource.Cast<object>().Select(i => (SelectedValue: toSelectedValue(i), Item: i)).ToList();
                        foreach (var p in items.Cast<object>()
                                               .Select(i => itemsSource.FirstOrDefault(p => p.SelectedValue.Equals(i)))
                                               .Where(p => p.Item != null))
                        {
                            comboBox.SelectedItems.Add(p.Item);
                        }
                    }
                }
            }

            return comboBox;
        }

        private void InitializeEditorProperties(RadComboBox comboBox)
        {
            comboBox.DisplayMemberPath = this.DisplayMemberPath;
            comboBox.SelectedValuePath = this.SelectedValuePath;
            comboBox.ItemsSource = this.ItemsSource;
        }
    }
}
