using System.Collections.Generic;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using System.ComponentModel;
using System.Windows.Forms;

namespace TelerikEx.PersistenceProvider
{
    /// <summary>
    /// Telerikのサンプルをベースに変更した。
    /// Telerik.Windows.Examples.PersistenceFramework.GridViewCustomSerialization
    /// </summary>
    public class ColumnProxy
    {
        public string UniqueName { get; set; }
        public int DisplayIndex { get; set; }
        public object Header { get; set; }
        public GridViewLength Width { get; set; }
        public bool IsVisible { get; set; }
        public SortingState SortingState { get; set; } = SortingState.None;

        public ColumnProxy(GridViewColumn c)
        {
            if (c == null) return;
            if (c.UniqueName != null) UniqueName = c.UniqueName;
            DisplayIndex = c.DisplayIndex;
            Width = c.Width;
            if (c.Header != null) Header = c.Header;
            IsVisible = c.IsVisible;
            SortingState = c.SortingState;
        }
    }

    public class SortDescriptorProxy
    {
        public string ColumnUniqueName { get; set; }
        public ListSortDirection SortDirection { get; set; }
    }

    public class GroupDescriptorProxy
    {
        public string ColumnUniqueName { get; set; }
        public ListSortDirection? SortDirection { get; set; }
    }

    public class FilterDescriptorProxy
    {
        public FilterOperator Operator { get; set; }
        public object Value { get; set; }
        public bool IsCaseSensitive { get; set; }
    }

    public class FilterSetting
    {
        public string ColumnUniqueName { get; set; }

        private List<object> selectedDistinctValue;
        public List<object> SelectedDistinctValues
        {
            get
            {
                if (this.selectedDistinctValue == null)
                {
                    this.selectedDistinctValue = new List<object>();
                }

                return this.selectedDistinctValue;
            }
        }

        public FilterDescriptorProxy Filter1 { get; set; }
        public FilterCompositionLogicalOperator FieldFilterLogicalOperator { get; set; }
        public FilterDescriptorProxy Filter2 { get; set; }
    }

}
