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
    public abstract class ModelBase : ModelBaseSlim, IDisposable
    {
        [NonSerialized]
        protected CompositeDisposable disposables = new CompositeDisposable();

        public virtual void Dispose()
        {
            disposables.Dispose();
        }
    }
}
