using LibRedminePower.Extentions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using RedmineTimePuncher.ViewModels.Input.Report;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.ViewModels.Input
{
    public class ReportsViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public bool IsEnabled { get; set; }
        public ReactivePropertySlim<bool> IsBusy { get; set; }
        public ObservableCollection<PersonHourReportViewModel> Items { get; set; }
        public ReactiveCommand UpdateCommand { get; set; }
        public bool IsExpanded { get; set; }

        public ReportsViewModel(InputViewModel input)
        {
            IsBusy = new ReactivePropertySlim<bool>().AddTo(disposables);
            Items = new ObservableCollection<PersonHourReportViewModel>();

            SettingsModel.Default.ObserveProperty(p => p.PersonHourReport).SubscribeWithErr(s =>
            {
                if (s == null)
                    return;

                Items.Clear();

                IsEnabled = s.Items.Any();
                foreach (var setting in s.Items)
                {
                    Items.Add(new PersonHourReportViewModel(input, setting));
                }
            });

            UpdateCommand = Items.CollectionChangedAsObservable().Select(_ => Items.Any()).CombineLatest(IsBusy, (any, ib) => any && !ib)
                .ToReactiveCommand().WithSubscribe(async () => await UpdateAsync()).AddTo(disposables);
        }

        public async Task UpdateAsync()
        {
            try
            {
                IsBusy.Value = true;
                await Task.WhenAll(Items.Select(i => i.UpdateAsync()).ToArray());
            }
            finally
            {
                IsBusy.Value = false;
            }
        }
    }
}
