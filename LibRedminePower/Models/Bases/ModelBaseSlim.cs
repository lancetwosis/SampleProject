using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Models.Bases
{
    [Serializable]
    public abstract class ModelBaseSlim : INotifyPropertyChanged
    {
#pragma warning disable 0067
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067

        public void RaisePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}