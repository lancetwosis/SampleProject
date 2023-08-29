using LibRedminePower.Extentions;
using LibRedminePower.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Views.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class TermViewModel : ColorSettingViewModelBase
    {
        public static TimeSpan NextStart { get; set; }
        public ReactivePropertySlim<TimeSpan> Start { get; set; }
        public ReactivePropertySlim<TimeSpan> End { get; set; }
        public ReactivePropertySlim<Enums.TermInputValidationType> ValidationType { get; private set; }
        public ReactivePropertySlim<bool> OnTime { get; set; }
        public ReactivePropertySlim<bool> IsCoreTime { get; set; }
        public TermModel Model { get; set; }

        /// <summary>
        /// RadGridView の「Click here to add new item」機能のために必要
        /// </summary>
        public TermViewModel() : base()
        {
            setModel(new TermModel(NextStart, getNextMajor(NextStart)));
        }

        public TermViewModel(TermModel model) : base()
        {
            setModel(model);
        }

        private void setModel(TermModel model)
        {
            this.Model = model;
            Start = model.ToReactivePropertySlimAsSynchronized(a => a.Start).AddTo(disposables);
            End = model.ToReactivePropertySlimAsSynchronized(a => a.End).AddTo(disposables);
            Start.Skip(1).SubscribeWithErr(a => End.Value = getNextMajor(a));
            Color = model.ToReactivePropertySlimAsSynchronized(a => a.Color, a => a.ToMediaColor(), a => a.ToDrawingColor()).AddTo(disposables);
            ValidationType = model.ToReactivePropertySlimAsSynchronized(a => a.ValidationType).AddTo(disposables);
            OnTime = model.ToReactivePropertySlimAsSynchronized(a => a.OnTime).AddTo(disposables);
            IsCoreTime = model.ToReactivePropertySlimAsSynchronized(a => a.IsCoreTime).AddTo(disposables);
        }

        private TimeSpan getNextMajor(TimeSpan timeSpan)
        {
            return TimeSpan.FromHours(timeSpan.Hours + 1);
        }

        public override string ToString() => Model.ToString();
    }
}
