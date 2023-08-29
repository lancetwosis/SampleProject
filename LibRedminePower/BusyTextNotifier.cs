using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace LibRedminePower
{
    /// <summary>
    /// Notify of busy.
    /// </summary>
    public class BusyTextNotifier : INotifyPropertyChanged, IObservable<bool>
    {

        private static readonly PropertyChangedEventArgs IsBusyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(IsBusy));
        private static readonly PropertyChangedEventArgs TextPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(Text));

        /// <summary>
        /// property changed event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private int ProcessCounter { get; set; }

        private Subject<bool> IsBusySubject { get; } = new Subject<bool>();
        private Subject<string> TextSubject { get; } = new Subject<string>();

        private object LockObject { get; } = new object();

        private bool isBusy;
        /// <summary>
        /// Is process running.
        /// </summary>
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy == value) { return; }
                isBusy = value;
                PropertyChanged?.Invoke(this, IsBusyPropertyChangedEventArgs);
                IsBusySubject.OnNext(isBusy);
            }
        }

        private string text;
        /// <summary>
        /// Is process running.
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                if (text == value) { return; }
                text = value;
                PropertyChanged?.Invoke(this, TextPropertyChangedEventArgs);
                TextSubject.OnNext(text);
            }
        }


        /// <summary>
        /// Process start.
        /// </summary>
        /// <returns>Call dispose method when process end.</returns>
        public IDisposable ProcessStart(string text)
        {
            lock (LockObject)
            {
                ProcessCounter++;
                IsBusy = ProcessCounter != 0;
                Text = text;

                return Disposable.Create(() =>
                {
                    lock (LockObject)
                    {
                        ProcessCounter--;
                        IsBusy = ProcessCounter != 0;
                        if (!IsBusy) Text = null;
                    }
                });
            }
        }

        /// <summary>
        /// Subscribe busy.
        /// </summary>
        /// <param name="observer">observer</param>
        /// <returns>disposable</returns>
        public IDisposable Subscribe(IObserver<bool> observer)
        {
            observer.OnNext(IsBusy);
            return IsBusySubject.Subscribe(observer);
        }
    }
}
