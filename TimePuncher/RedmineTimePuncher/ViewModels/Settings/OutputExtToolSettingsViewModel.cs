using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class OutputExtToolSettingsViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<string> FileName { get; set; }
        public ReactivePropertySlim<string> Argument { get; set; }

        public ReactiveCommand SelectExtToolFileNameCommand { get; set; }

        public string ArgumentHelpMsg { get; set; }

        public OutputExtToolSettingsViewModel(OutputExtToolSettingsModel model)
        {
            FileName = model.ToReactivePropertySlimAsSynchronized(a => a.FileName).AddTo(disposables);
            Argument = model.ToReactivePropertySlimAsSynchronized(a => a.Argument).AddTo(disposables);

            SelectExtToolFileNameCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                var dialog = new OpenFileDialog();
                dialog.FileName = FileName.Value;
                dialog.Filter =
                "Executable Files|*.exe;*.bat;*.ps1" +
                "|All Files|*.*";
                dialog.FilterIndex = 0;
                if (dialog.ShowDialog().Value == true)
                {
                    FileName.Value = dialog.FileName;
                }
            }).AddTo(disposables);

            ArgumentHelpMsg = string.Format(Resources.SettingsExpoMsgExternalToolArgHelp, OutputDataSettingsModel.FILE_NAME);
        }
    }
}
