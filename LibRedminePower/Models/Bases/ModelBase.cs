using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LibRedminePower.Models.Bases
{
    [Serializable]
    public abstract class ModelBase : ModelBaseSlim, IDisposable
    {
        private CompositeDisposable _disposables;
        [JsonIgnore]
        protected CompositeDisposable disposables
        {
            get
            {
                if (_disposables == null)
                    _disposables = new CompositeDisposable();
                return _disposables;
            }
        }

        public virtual void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
