using ObservableCollectionSync;
using LibRedminePower.Extentions;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Views.Settings;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using LibRedminePower.ViewModels;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class ScheduleSettingsViewModel : Bases.SettingsViewModelBase<ScheduleSettingsModel>
    {
        public ReactivePropertySlim<TickLengthType> TickLength { get; set; }
        public ReadOnlyReactivePropertySlim<int> TickLengthValue { get; set; }
        public ReactivePropertySlim<TimeSpan> DayStartTime { get; set; }
        public ReactivePropertySlim<TimeSpan> WorkStartTime { get; set; }
        public ReactivePropertySlim<bool> UseFlexTime { get; set; }
        public ReadOnlyReactivePropertySlim<EditableGridViewModel<TermViewModel, TermModel>> SpecialTerms { get; set; }

        private CompositeDisposable myDisposables;

        public ScheduleSettingsViewModel(ScheduleSettingsModel model) : base(model)
        {
            TickLength = model.ToReactivePropertySlimAsSynchronized(a => a.TickLength).AddTo(disposables);
            TickLengthValue = TickLength.Select(a => (int)a).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            DayStartTime = model.ToReactivePropertySlimAsSynchronized(a => a.DayStartTime).AddTo(disposables);
            WorkStartTime = model.ToReactivePropertySlimAsSynchronized(a => a.WorkStartTime).AddTo(disposables);
            UseFlexTime = model.ToReactivePropertySlimAsSynchronized(a => a.UseFlexTime).AddTo(disposables);
            SpecialTerms = model.ToReadOnlyViewModel(a => a.SpecialTerms, 
                stm => new EditableGridViewModel<TermViewModel, TermModel>(stm, a => new TermViewModel(a), a => a.Model)).AddTo(disposables);
            var maxValue =
                SpecialTerms.Select(col => col.MaxAsObservable(a => a.End.Value, a => a.End.Value)).Switch();
            SpecialTerms.Select(a => a.AnyAsObservable()).Switch().CombineLatest(maxValue, WorkStartTime, (any, max, start) => any ? max : start)
                .SubscribeWithErr(a => TermViewModel.NextStart = a).AddTo(disposables);

            TickLength.SubscribeWithErr(a =>
            {
                // TODO: 一旦 TickLength で割って余りが出たらデフォルトを設定するようにする
                //       集約例外の処理を対応後に再度仕様を検討すること
                if (WorkStartTime.Value.Minutes % (int)a != 0)
                    WorkStartTime.Value = ScheduleSettingsModel.DEFAULT_WORK_START;
                if (DayStartTime.Value.Minutes % (int)a != 0)
                    DayStartTime.Value = ScheduleSettingsModel.DEFAULT_DAY_START;

                if (SpecialTerms != null)
                    foreach (var term in SpecialTerms.Value)
                    {
                        if (term.Start.Value.Minutes % (int)a != 0 || term.End.Value.Minutes % (int)a != 0)
                        {
                            term.Start.Value = ScheduleSettingsModel.DEFAULT_WORK_START;
                            term.End.Value = ScheduleSettingsModel.DEFAULT_WORK_START;
                        }
                    }
            });
        }
    }
}
