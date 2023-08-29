using LibRedminePower.Extentions;
using LibRedminePower.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class OutputCsvExportSettingsViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public TwinListBoxViewModel<Enums.ExportItems> TwinListBoxViewModel { get; set; }
        public ReactivePropertySlim<string> ExportDir { get; set; }
        public ReactivePropertySlim<bool> IsOepn { get; set; } 
        public ReactivePropertySlim<int> ExportNum { get; set; }

        public ReactiveCommand SelectCsvExportDirCommand { get; set; }

        public OutputCsvExportSettingsViewModel(OutputCsvExportSettingsModel model)
        {
            var selectedItems = new ObservableCollection<Enums.ExportItems>(model.ExportItems);
            TwinListBoxViewModel = new TwinListBoxViewModel<Enums.ExportItems>(FastEnumUtility.FastEnum.GetValues<Enums.ExportItems>(), selectedItems);
            selectedItems.ObserveAddChanged().SubscribeWithErr(a => model.ExportItems.Add(a)).AddTo(disposables);
            selectedItems.ObserveRemoveChanged().SubscribeWithErr(a => model.ExportItems.Remove(a)).AddTo(disposables);
            selectedItems.CollectionChangedAsObservable().Where(a => a.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move).SubscribeWithErr(a =>
            {
                model.ExportItems.Move(a.OldStartingIndex, a.NewStartingIndex);
            }).AddTo(disposables);

            ExportDir = model.ToReactivePropertySlimAsSynchronized(a => a.ExportDir).AddTo(disposables);
            IsOepn = model.ToReactivePropertySlimAsSynchronized(a => a.IsOepn).AddTo(disposables);
            ExportNum = model.ToReactivePropertySlimAsSynchronized(a => a.ExportNum).AddTo(disposables);

            SelectCsvExportDirCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                using (var cofd = new CommonOpenFileDialog()
                {
                    // フォルダ選択モードにする
                    IsFolderPicker = true,
                })
                {
                    if (cofd.ShowDialog() != CommonFileDialogResult.Ok) return;
                    ExportDir.Value = cofd.FileName;
                }
            }).AddTo(disposables);
        }
    }
}
