using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Models.Bases
{
    [Serializable]
    public abstract class ModelBase : INotifyPropertyChanged, IDisposable
    {
#pragma warning disable 0067
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067

        [NonSerialized]
        protected CompositeDisposable disposables = new CompositeDisposable();

        public void RaisePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public virtual void Dispose()
        {
            disposables.Dispose();
        }
    }
}
