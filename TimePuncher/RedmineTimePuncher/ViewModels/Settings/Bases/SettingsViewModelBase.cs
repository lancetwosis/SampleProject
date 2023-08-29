using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Views.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings.Bases
{
    public abstract class SettingsViewModelBase<TModel> : LibRedminePower.ViewModels.Bases.ViewModelBase where TModel : ISettingsModel
    {
        public ReactiveCommand ImportCommand { get; set; }
        public ReactiveCommand ExportCommand { get; set; }

        public SettingsViewModelBase(TModel model)
        {
            var defaultFileName = typeof(TModel).Name.Replace("Model", "");
            ImportCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                var dialog = new OpenFileDialog();
                dialog.FileName = defaultFileName;
                dialog.Filter = "Text Files|*.txt" + "|All Files|*.*";
                dialog.FilterIndex = 0;
                if (dialog.ShowDialog().Value == true)
                {
                    try
                    {
                        model.Import(dialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(string.Format(Properties.Resources.errImport, ex.Message), ex);
                    }
                }
            }).AddTo(disposables);

            ExportCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                var dialog = new SaveFileDialog();
                dialog.FileName = defaultFileName;
                dialog.Filter = "Text Files|*.txt" + "|All Files|*.*";
                dialog.FilterIndex = 0;
                if (dialog.ShowDialog().Value == true)
                {
                    try
                    {
                        model.Export(dialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(string.Format(Properties.Resources.errExport, ex.Message), ex);
                    }
                }
            }).AddTo(disposables);
        }
    }
}
