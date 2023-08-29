using LibRedminePower.Applications;
using LibRedminePower.Helpers;
using LibRedminePower.ViewModels;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Models;
using RedmineTableEditor.Models.Bases;
using RedmineTableEditor.Properties;
using RedmineTableEditor.ViewModels.FileSettings;
using RedmineTableEditor.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTableEditor.ViewModels
{
    public class TableEditorViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<RedmineManager> Redmine { get; set; }
        public CancellationTokenSource CTS { get; set; }

        public BusyNotifier IsBusy { get; set; }
        public ReactiveCommand CancelCommand { get; set; }
        public ReactivePropertySlim<string> ErrorMessage { get; set; }

        public ReadOnlyReactivePropertySlim<string> TitlePrefix { get; set; }

        public FileSettingsViewModel FileSettings { get; set; }
        public IssuesViewModel Issues { get; set; }

        public AsyncCommandBase OpenCommand { get; set; }
        public CommandBase NewCommand { get; set; }
        public CommandBase SaveCommand { get; set; }
        public CommandBase SaveAsCommand { get; set; }
        public AsyncCommandBase ApplyCommand { get; set; }
        public AsyncCommandBase UpdateContentCommand { get; set; }
        public AsyncCommandBase SaveToRedmineCommand { get; set; }

        public TableEditorViewModel(ReadOnlyReactivePropertySlim<(IRedmineManager Manager, IRedmineManager MasterManager)> redmine)
        {
            IsBusy = new BusyNotifier();
            var isCanceling = new ReactivePropertySlim<bool>().AddTo(disposables);
            CTS = new CancellationTokenSource();
            CancelCommand = new[]
            {
                IsBusy,
                isCanceling.Select(a => !a),
            }.CombineLatestValuesAreAllTrue().ToReactiveCommand().WithSubscribe(() =>
            {
                isCanceling.Value = true;
                CTS.Cancel();
                isCanceling.Value = false;
            }).AddTo(disposables);

            ErrorMessage = new ReactivePropertySlim<string>().AddTo(disposables);

            Redmine = new ReactivePropertySlim<RedmineManager>().AddTo(disposables);
            redmine.Select(r => new RedmineManager(r)).Subscribe(async r =>
            {
                var err = r.IsValid();
                if (err != null)
                {
                    ErrorMessage.Value = err;
                    Redmine.Value = r;
                    return;
                }

                ErrorMessage.Value = null;
                FileSettings?.Dispose();
                Issues?.Clear();
                Issues?.Dispose();

                await r.UpdateAsync();

                Redmine.Value = r;

                FileSettings = new FileSettingsViewModel(this).AddTo(disposables);
                Issues = new IssuesViewModel(this).AddTo(disposables);
            }).AddTo(disposables);

            TitlePrefix = this.ObserveProperty(a => a.FileSettings.FileName).CombineLatest(this.ObserveProperty(a => a.FileSettings.IsEdited.Value),
                (f, ie) =>
                {
                    if (string.IsNullOrEmpty(f))
                        return Resources.FileNew;
                    else
                        return ie ? $"{f}{Resources.FileUpdated}" : $"{f}";
                })
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);

            //----------------------------
            // 開くボタン
            //----------------------------
            OpenCommand = new AsyncCommandBase(
                Resources.RibbonCmdOpen, Resources.open_icon,
                new[] {
                    Redmine.Select(a => a != null && string.IsNullOrEmpty(a.IsValid())),
                    IsBusy.Select(a => !a),
                }.CombineLatestValuesAreAllTrue().Select(a => a ? null : ""),
                async () =>
                {
                    var r = FileSettings.SaveIfNeeded();
                    if (!r.HasValue)
                        return;

                    var dialog = new OpenFileDialog();
                    dialog.Filter = Resources.RedmineTableEditorFileFormat;
                    if (dialog.ShowDialog().Value)
                    {
                        await FileSettings.ReadAsync(dialog.FileName);
                        await Issues.ApplyAsync(FileSettings.Model.Value);
                    }
                }).AddTo(disposables);

            // 管理者のAPIキーが設定されていなかった場合、レイアウトが崩れるので先にダミーを作成する
            NewCommand = new CommandBase(Resources.RibbonCmdNew, Resources.new_icon).AddTo(disposables);
            SaveCommand = new CommandBase(Resources.RibbonCmdSave, Resources.save_icon).AddTo(disposables);
            SaveAsCommand = new CommandBase(Resources.RibbonCmdSaveAs, Resources.saveas_icon).AddTo(disposables);
            ApplyCommand = new AsyncCommandBase(Resources.RibbonCmdApply, Resources.apply_icon).AddTo(disposables);
            UpdateContentCommand = new AsyncCommandBase(Resources.RibbonCmdUpdate, Resources.reload_icon).AddTo(disposables);
            SaveToRedmineCommand = new AsyncCommandBase(Resources.RibbonCmdSaveRedmine, Resources.save_redmine_icon).AddTo(disposables);
        }

        private bool first = true;
        public void LoadFirstSettings(string fileName = null)
        {
            // 初回のファイルの読み込み（前回ファイルもしくは指定されたファイル）
            if (first)
            {
                first = false;
                this.ObserveProperty(a => a.FileSettings).CombineLatest(this.ObserveProperty(a => a.Issues), (f, i) => (f, i))
                    .Where(p => p.f != null && p.i != null).Take(1).ObserveOnUIDispatcher().Subscribe(async _ =>
                    {
                        if (await FileSettings.LoadFirstSettingAsync(fileName))
                        {
                            await Issues.ApplyAsync(FileSettings.Model.Value);
                        }
                    }).AddTo(disposables);
            }
        }

        public void OnWindowClosing(CancelEventArgs e)
        {
            if (FileSettings == null)
                return;

            var result = FileSettings.SaveIfNeeded();
            if (!result.HasValue)
            {
                e.Cancel = true;
            }

            Settings.Default.LastFileName = FileSettings.FileName;
            Settings.Default.Save();
        }
    }
}
