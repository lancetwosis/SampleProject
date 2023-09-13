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
    public class ExpandableTwinListBoxViewModel<T> : TwinListBoxViewModel<T>
    {
        public bool Expanded { get; set; }

        public ReactiveCommand ExpandCommand { get; set; }

        public ReactiveCommand<T> RemoveCommand { get; set; }

        public ExpandableTwinListBoxViewModel(IEnumerable<T> allItems, ObservableCollection<T> selectedItems)
            : base(allItems, selectedItems)
        {
            ExpandCommand = new ReactiveCommand().WithSubscribe(() => Expanded = !Expanded).AddTo(disposables);
            RemoveCommand = new ReactiveCommand<T>().WithSubscribe(t => ToItems.Remove(t)).AddTo(disposables);
        }
    }
}
