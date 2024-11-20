using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower
{
    public class ReactiveCommandSlimToolTip : ReactiveCommandSlim, IDisposable
    {
        public ReadOnlyReactivePropertySlim<string> ToolTip { get; set; }

        private CompositeDisposable disposables = new CompositeDisposable();

        public ReactiveCommandSlimToolTip(IObservable<string> canExecute)
                : base(canExecute.Select(a => a == null))
        {
            ToolTip = canExecute.ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public new void Dispose()
        {
            base.Dispose();
            disposables.Dispose();
        }
    }

    public class ReactiveCommandSlimToolTip<T> : ReactiveCommandSlim<T>, IDisposable
    {
        public ReadOnlyReactivePropertySlim<string> ToolTip { get; set; }

        private CompositeDisposable disposables = new CompositeDisposable();

        public ReactiveCommandSlimToolTip(IObservable<string> canExecute)
                : base(canExecute.Select(a => a == null))
        {
            ToolTip = canExecute.ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public new void Dispose()
        {
            base.Dispose();
            disposables.Dispose();
        }
    }

    public static class ReactiveCommandSlimToolTipExtensions
    {
        public static ReactiveCommandSlimToolTip WithSubscribe(this ReactiveCommandSlimToolTip self, Action onNext)
        {
            self.SubscribeWithErr(_ => onNext());
            return self;
        }

        public static ReactiveCommandSlimToolTip ToReactiveCommandToolTipSlim(this IObservable<string> canExecute)
        {
            return new ReactiveCommandSlimToolTip(canExecute);
        }

        public static ReactiveCommandSlimToolTip<T> WithSubscribe<T>(this ReactiveCommandSlimToolTip<T> self, Action<T> onNext)
        {
            self.SubscribeWithErr(onNext);
            return self;
        }

        public static ReactiveCommandSlimToolTip<T> ToReactiveCommandToolTipSlim<T>(this IObservable<string> canExecute)
        {
            return new ReactiveCommandSlimToolTip<T>(canExecute);
        }

    }
}