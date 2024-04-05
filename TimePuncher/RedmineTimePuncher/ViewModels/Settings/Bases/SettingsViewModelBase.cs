using LibRedminePower.Applications;
using LibRedminePower.Extentions;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Views.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings.Bases
{
    public abstract class SettingsViewModelBase<TModel> : LibRedminePower.ViewModels.Bases.ViewModelBase where TModel : ISettingsModel
    {
        public ReactiveCommand ImportCommand { get; set; }
        public ReactiveCommand ExportCommand { get; set; }

        public ReactivePropertySlim<bool> HasOnlineHelp { get; set; }
        public ReactiveCommand OpenOnlineHelpCommand { get; set; }

        public SettingsViewModelBase(TModel model)
        {
            var settingName = typeof(TModel).Name.Replace("Model", "").ToSnakeCase();
            ImportCommand = new ReactiveCommand().WithSubscribe(() =>
            {
                var dialog = new OpenFileDialog();
                dialog.FileName = settingName;
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
                dialog.FileName = settingName;
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

            HasOnlineHelp = new ReactivePropertySlim<bool>().AddTo(disposables);

            var helpUrl = $"{ApplicationInfo.AppBaseUrl}{settingName}/";
            OpenOnlineHelpCommand = HasOnlineHelp.ToReactiveCommand().WithSubscribe(() => Process.Start(helpUrl)).AddTo(disposables);
            Task.Run(async () =>
            {
                using (var client = new HttpClient())
                {
                    var r = await client.GetAsync(helpUrl);
                    HasOnlineHelp.Value = r.StatusCode == System.Net.HttpStatusCode.OK;
                }
            });
        }
    }
}
