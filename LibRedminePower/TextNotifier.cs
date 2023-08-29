using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower
{
    public class TextNotifier : INotifyPropertyChanged, IObservable<string>, IDisposable
    {
        private static readonly PropertyChangedEventArgs ValuePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(Value));
        private static readonly PropertyChangedEventArgs IsNullOrEmptyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(IsNullOrEmpty));
        private static readonly PropertyChangedEventArgs IsNullPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(IsNull));

        /// <summary>
        /// property changed event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private Subject<string> TextSubject { get; } = new Subject<string>();
        private Subject<bool> IsNullOrEmptySubject { get; } = new Subject<bool>();

        private string value;
        /// <summary>
        /// Is process running.
        /// </summary>
        public string Value
        {
            get { return value; }
            set
            {
                if (this.value == value) { return; }
                this.value = value;
                PropertyChanged?.Invoke(this, ValuePropertyChangedEventArgs);
                TextSubject.OnNext(this.value);
            }
        }

        private bool isNullOrEmpty = true;
        public bool IsNullOrEmpty
        {
            get { return isNullOrEmpty; }
            set
            {
                if (isNullOrEmpty == value) return;
                isNullOrEmpty = value;
                PropertyChanged?.Invoke(this, IsNullOrEmptyPropertyChangedEventArgs);
                IsNullOrEmptySubject.OnNext(isNullOrEmpty);
            }
        }

        private bool isNull = true;
        public bool IsNull
        {
            get { return isNull; }
            set
            {
                if (isNull == value) return;
                isNull = value;
                PropertyChanged?.Invoke(this, IsNullPropertyChangedEventArgs);
                IsNullOrEmptySubject.OnNext(isNull);
            }
        }

        private CompositeDisposable disposables = new CompositeDisposable();

        public TextNotifier()
        {
            TextSubject.SubscribeWithErr(a => IsNullOrEmpty = string.IsNullOrEmpty(a)).AddTo(disposables);
            TextSubject.SubscribeWithErr(a => IsNull = a == null).AddTo(disposables);
        }

        public TextNotifier(IObservable<string> target) : this()
        {
            target.SubscribeWithErr(a => Value = a).AddTo(disposables);
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            observer.OnNext(Value);
            return TextSubject.Subscribe(observer);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
