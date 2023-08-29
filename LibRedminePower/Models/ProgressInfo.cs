using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Reactive.Bindings;
using System.Threading;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using Reactive.Bindings.Notifiers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using LibRedminePower.Extentions;

namespace LibRedminePower.Models
{
    public class ProgressInfo : Bases.ModelBase
	{
        public ReadOnlyReactivePropertySlim<TimeSpan> ElapsedTime { get; set; }
        public ReactivePropertySlim<TimeSpan> RemainTime { get; set; }
        public ReadOnlyReactivePropertySlim<TimeSpan> EstimateTime { get; set; }

        public ReadOnlyReactivePropertySlim<bool> IsBusy { get; }
        /// <summary>
        /// 進捗値
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// ヘッダーテキスト
        /// </summary>
        public string HeaderText { get; set; }
        /// <summary>
        /// テキスト
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// キャンセル
        /// </summary>
        public CancellationTokenSource Cancellation { get; set; } 
        /// <summary>
        /// 進捗更新時のアクション
        /// </summary>
        public Action<double> OnValueChangedAction { get; set; }
        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        public ReactiveCommand CancelCommand { get; set; }

        public Func<int, string> GetProgressText { get; set; }

        private BusyNotifier busyNotifier;

        public ProgressInfo()
        {
            var sw = new Stopwatch();
            busyNotifier = new BusyNotifier();
            busyNotifier.Where(a => a).SubscribeWithErr(a =>
            {
                Value = 0;
                sw.Start();
                var canCancel = new ReactiveProperty<bool>(true).AddTo(disposables);
                Cancellation = new CancellationTokenSource();
                CancelCommand = canCancel.ToReactiveCommand().WithSubscribe(() =>
                {
                    canCancel.Value = false;
                    Cancellation.Cancel();
                }).AddTo(disposables);
            });

            IsBusy = busyNotifier.ToReadOnlyReactivePropertySlim();

            this.ObserveProperty(a => a.Value).SubscribeWithErr(v =>
            {
                OnValueChangedAction?.Invoke(v);
                Text = string.Format(Properties.Resources.ProgressBarProgresText, (int)v);
            }).AddTo(disposables);

            ElapsedTime = Observable.Interval(TimeSpan.FromSeconds(1))
                .Select(a =>
                {
                    if(RemainTime.Value != null && RemainTime.Value.TotalMilliseconds > 0)
                        RemainTime.Value -= TimeSpan.FromSeconds(1);
                    return sw.Elapsed;
                }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            RemainTime = new ReactivePropertySlim<TimeSpan>().AddTo(disposables);
            EstimateTime = RemainTime.Where(a => a.TotalMilliseconds > 0).Select(a => (DateTime.Now + a).TimeOfDay).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public IDisposable Start()
        {
            return busyNotifier.ProcessStart();
        }

        public void SetValue(double value)
        {
            Cancellation.Token.ThrowIfCancellationRequested();
            if(!double.IsNaN(value))
                Value = value;
        }

        public void AddValue(double value)
        {
            Cancellation.Token.ThrowIfCancellationRequested();
            if (!double.IsNaN(value))
                Value += value;
        }
    }
}
