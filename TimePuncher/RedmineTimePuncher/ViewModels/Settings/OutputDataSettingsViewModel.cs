using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Settings;
using System.Reactive.Linq;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class OutputDataSettingsViewModel : Bases.SettingsViewModelBase<OutputDataSettingsModel>
    {
        public ReadOnlyReactivePropertySlim<OutputCsvExportSettingsViewModel> CsvExport { get; set; }
        public ReadOnlyReactivePropertySlim<OutputExtToolSettingsViewModel> ExtTool { get; set; }

        public OutputDataSettingsViewModel(OutputDataSettingsModel model) :base(model)
        {
            CsvExport = model.ObserveProperty(a => a.CsvExport).Select(a => new OutputCsvExportSettingsViewModel(a)).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
            ExtTool = model.ObserveProperty(a => a.ExtTool).Select(a => new OutputExtToolSettingsViewModel(a)).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }
}
