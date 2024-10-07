using LibRedminePower.Extentions;
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
            CsvExport = model.ToReadOnlyViewModel(a => a.CsvExport, a => new OutputCsvExportSettingsViewModel(a)).AddTo(disposables);
            ExtTool = model.ToReadOnlyViewModel(a => a.ExtTool, a => new OutputExtToolSettingsViewModel(a)).AddTo(disposables);
        }
    }
}
