using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTableEditor.Models.TicketFields.Bases
{
    public abstract class FieldBase : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public string Name { get; }
        public bool IsEdited { get; set; }

        public FieldBase(string name)
        {
            Name = name;
        }
    }

    public abstract class FieldBase<T>  : FieldBase
    {
        public T Value
        {
            get { return GetFunc(); }
            set 
            { 
                SetFunc(value);
                IsEdited = true;
                RaisePropertyChanged(nameof(Value));
            }
        }


        public Func<T> GetFunc { get; set; }
        public Action<T> SetFunc { get; set; }


        public FieldBase(string name, Func<T> getFunc, Action<T> setFunc) : base(name)
        {
            this.GetFunc = getFunc;
            this.SetFunc = setFunc;
        }

        public override string ToString()
        {
            return $"{Name}: {Value} (IsEdited={IsEdited})";
        }
    }
}
