using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class NeedsRestartSettingViewModel<T> : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<T> Value { get; set; }

        public bool NeedsRestart => !Value.Value.Equals(initialValue);

        private T initialValue;

        public NeedsRestartSettingViewModel(ReactivePropertySlim<T> value)
        {
            Value = value;
            Value.Skip(1).SubscribeWithErr(_ => MessageBoxHelper.ConfirmWarning(Properties.Resources.msgNeedRestart)).AddTo(disposables);

            initialValue = Value.Value;
        }
    }
}
