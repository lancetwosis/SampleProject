using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.Logging;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Settings.Bases;
using RedmineTimePuncher.ViewModels.Input.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTimePuncher.ViewModels.Input.Resources.Bases
{
    public class ResourceUpdater
    {
        public AsyncReactiveCommand UpdateCommand { get; private set; }
        public AsyncReactiveCommand CancelCommand { get; }
        public MyTimeIndicator Indicator { get; protected set; }
        public BusyNotifier IsBusy { get; } = new BusyNotifier();

        private MyResourceBase res;
        private CancellationTokenSource cts;

        public ResourceUpdater(MyResourceBase res, Brush brush)
        {
            this.res = res;
            Indicator = new MyTimeIndicator(brush);

            CancelCommand = IsBusy.ToAsyncReactiveCommand().WithSubscribe(async () =>
            {
                cts.Cancel();
                if (IsBusy.IsBusy)
                    await IsBusy.ToReadOnlyReactivePropertySlim();
            });
        }

        public void SetUpdateCommand(IObservable<bool> canExecute, Func<CancellationToken, Task> func)
        {
            UpdateCommand = canExecute.ToAsyncReactiveCommand().WithSubscribe(async () =>
            {
                cts = new CancellationTokenSource();
                using (IsBusy.ProcessStart())
                {
                    await func(cts.Token);
                    Indicator.DateTime = DateTime.Now;
                }
            });
        }

        public ReactiveTimer CreateAutoReloadTimer(IAutoUpdateSetting setting, AsyncReactiveCommand saveCommand = null)
        {
            return CreateAutoReloadTimer(setting, () =>
            {
                if (saveCommand != null && saveCommand.CanExecute())
                    return saveCommand.ExecuteAsync();
                else if (UpdateCommand.CanExecute())
                    return UpdateCommand.ExecuteAsync();
                else
                    return Task.CompletedTask;
            });
        }

        public ReactiveTimer CreateAutoReloadTimer(IAutoUpdateSetting setting, Func<Task> func)
        {
            if (setting.IsAutoUpdate)
            {
                var t = new ReactiveTimer(TimeSpan.FromMinutes(setting.AutoUpdateMinutes));
                var executing = new BusyNotifier();
                t.ObserveOnUIDispatcher().Skip(1).Where(_ => !executing.IsBusy).SubscribeWithErr(async _ =>
                {
                    if (res.ResourceSetting.IsEnabled)
                    {
                        using (executing.ProcessStart())
                        {
                            try
                            {
                                await func.Invoke();
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"Auto Update Exception occured. {ex.ToString()}");
                                MessageBoxHelper.ConfirmError(ex.Message);
                            }
                        }
                    }
                });
                t.Start();
                return t;
            }
            else
                return null;
        }
    }
}
