using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.Models.TicketFields.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using System.Windows.Data;

namespace RedmineTableEditor.Models.TicketFields.Standard.Bases
{
    public abstract class FieldList<T> : FieldBase<int?>
    {
        public T SelectedItem => getSelectedItemFunc(Value);
        public string DisplayValue => getDisplayValueFunc(SelectedItem);

        private Func<int?, T> getSelectedItemFunc { get; }
        private Func<T, string> getDisplayValueFunc { get; }

        public FieldList(string name, Issue issue, Func<int?> getFunc, Action<int?> setFunc, Func<int?, T> getSelectedItemFunc, Func<T, string> getDisplayValueFunc)
            : base(name, getFunc, setFunc)
        {
            this.getSelectedItemFunc = getSelectedItemFunc;
            this.getDisplayValueFunc = getDisplayValueFunc;
        }

        public override string ToString()
        {
            return $"{Name}: {Value}({DisplayValue}) (IsEdited={IsEdited})";
        }
    }
}
