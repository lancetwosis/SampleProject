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

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class ScheduleSettingsViewModel : Bases.SettingsViewModelBase<ScheduleSettingsModel>
    {
        public ReactivePropertySlim<TickLengthType> TickLength { get; set; }
        public ReadOnlyReactivePropertySlim<int> TickLengthValue { get; set; }
        public ReactivePropertySlim<TimeSpan> DayStartTime { get; set; }
        public ReactivePropertySlim<TimeSpan> WorkStartTime { get; set; }
        public ReactivePropertySlim<bool> UseFlexTime { get; set; }
        public EditableGridViewModel<TermViewModel, TermModel> SpecialTerms { get; set; }

        private CompositeDisposable myDisposables;

        public ScheduleSettingsViewModel(ScheduleSettingsModel model) :base(model)
        {
            TickLength = model.ToReactivePropertySlimAsSynchronized(a => a.TickLength).AddTo(disposables);
            TickLengthValue = TickLength.Select(a => (int)a).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            DayStartTime = model.ToReactivePropertySlimAsSynchronized(a => a.DayStartTime).AddTo(disposables);
            WorkStartTime = model.ToReactivePropertySlimAsSynchronized(a => a.WorkStartTime).AddTo(disposables);

            UseFlexTime = model.ToReactivePropertySlimAsSynchronized(a => a.UseFlexTime).AddTo(disposables);

            setUpSpecialTerms(model);

            TickLength.SubscribeWithErr(a =>
            {
                // TODO: 一旦 TickLength で割って余りが出たらデフォルトを設定するようにする
                //       集約例外の処理を対応後に再度仕様を検討すること
                if (WorkStartTime.Value.Minutes % (int)a != 0) 
                    WorkStartTime.Value = ScheduleSettingsModel.DEFAULT_WORK_START;
                if (DayStartTime.Value.Minutes % (int)a != 0) 
                    DayStartTime.Value = ScheduleSettingsModel.DEFAULT_DAY_START;

                if (SpecialTerms != null)
                    foreach (var term in SpecialTerms)
                    {
                        if (term.Start.Value.Minutes % (int)a != 0 || term.End.Value.Minutes % (int)a != 0)
                        {
                            term.Start.Value = ScheduleSettingsModel.DEFAULT_WORK_START;
                            term.End.Value = ScheduleSettingsModel.DEFAULT_WORK_START;
                        }
                    }
            });

            SpecialTerms.CollectionChangedAsObservable().StartWithDefault().CombineLatest(
                WorkStartTime,
                SpecialTerms.MaxAsObservable(a => a.End.Value, a => a.End.Value), (_, start, max) => SpecialTerms.Any() ? max : start)
                .SubscribeWithErr(a => TermViewModel.NextStart = a);

            ImportCommand = ImportCommand.WithSubscribe(() => setUpSpecialTerms(model)).AddTo(disposables);
        }

        private void setUpSpecialTerms(ScheduleSettingsModel model)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            SpecialTerms = new EditableGridViewModel<TermViewModel, TermModel>(model.SpecialTerms, 
                a => new TermViewModel(a), a => a.Model).AddTo(myDisposables);
        }
    }
}
