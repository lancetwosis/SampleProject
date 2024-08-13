using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.ViewModels;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.FileSettings;
using RedmineTableEditor.Properties;

namespace RedmineTableEditor.ViewModels.FileSettings
{
    public class FileSettingsViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReadOnlyReactivePropertySlim<ParentIssueSettingsViewModel> ParentIssues { get; set; }
        public ReadOnlyReactivePropertySlim<SubIssueSettingsViewModel> SubIssues { get; set; }
        public ReadOnlyReactivePropertySlim<AutoBackColorViewModel> AutoBackColor { get; set; }

        public ReactiveProperty<bool> IsEdited { get; set; }
        public string FileName { get; set; }
        public ReactivePropertySlim<FileSettingsModel> Model { get; set; }

        private TableEditorViewModel parent;

        public FileSettingsViewModel(TableEditorViewModel parent)
        {
            this.parent = parent;

            Model = new ReactivePropertySlim<FileSettingsModel>().AddTo(disposables);

            FileName = Settings.Default.LastFileName;

            parent.Redmine.SubscribeWithErr(r =>
            {
                if (r.IsValid() != null) return;

                Model.Value = new FileSettingsModel();

                ParentIssues = Model.Select(a => new ParentIssueSettingsViewModel(a.ParentIssues, r)).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
                SubIssues = Model.Select(a => new SubIssueSettingsViewModel(a.SubIssues, r)).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);
                AutoBackColor = Model.Select(a => new AutoBackColorViewModel(a.AutoBackColor, r)).DisposePreviousValue().ToReadOnlyReactivePropertySlim().AddTo(disposables);

                IsEdited = new[]
                {
                    ParentIssues.ObserveProperty(a => a.Value.IsEdited.Value).Where(a => a),
                    SubIssues.ObserveProperty(a => a.Value.IsEdited.Value).Where(a => a),
                    AutoBackColor.ObserveProperty(a => a.Value.IsEdited.Value).Where(a => a),
                }.Merge().Select(_ => true).ToReactiveProperty().AddTo(disposables);

                IsEdited.SubscribeWithErr(a =>
                {
                    if (a)
                    {
                        System.Console.WriteLine($"ParentIssues = {ParentIssues.Value.IsEdited.Value}");
                        System.Console.WriteLine($"SubIssues = {SubIssues.Value.IsEdited.Value}");
                        System.Console.WriteLine($"AutoBackColor = {AutoBackColor.Value.IsEdited.Value}");
                    }
                    else
                    {
                        ParentIssues.Value.IsEdited.Value = false;
                        SubIssues.Value.IsEdited.Value = false;
                        AutoBackColor.Value.IsEdited.Value = false;
                    }
                }).AddTo(disposables);

            }).AddTo(disposables);

            parent.NewCommand = new CommandBase(
                Resources.RibbonCmdNew, Resources.new_icon,
                new[] {
                    parent.Redmine.Select(a => a != null && string.IsNullOrEmpty(a.IsValid())),
                    parent.IsBusy.Select(a => !a),
                }.CombineLatestValuesAreAllTrue().Select(a => a ? null : ""),
                () =>
                {
                    var r = SaveIfNeeded();
                    if (!r.HasValue)
                        return;

                    Model.Value = new FileSettingsModel();
                    FileName = null;
                }).AddTo(disposables);

            parent.SaveCommand = new CommandBase(
                Resources.RibbonCmdSave, Resources.save_icon,
                new[] {
                    Model.Select(m => m != null),
                    parent.Redmine.Select(a => a != null && string.IsNullOrEmpty(a.IsValid())),
                    parent.IsBusy.Select(a => !a),
                }.CombineLatestValuesAreAllTrue().Select(a => a ? null : ""),
                () => save()).AddTo(disposables);

            parent.SaveAsCommand = new CommandBase(
                Resources.RibbonCmdSaveAs, Resources.saveas_icon,
                new[] {
                    Model.Select(m => m != null),
                    parent.Redmine.Select(a => a != null && string.IsNullOrEmpty(a.IsValid())),
                    parent.IsBusy.Select(a => !a),
                }.CombineLatestValuesAreAllTrue().Select(a => a ? null : ""),
                () => saveAs()).AddTo(disposables);
        }

        public void save()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                saveAs();
            }
            else
            {
                System.IO.File.WriteAllText(FileName, Model.Value.ToJson());
                IsEdited.Value = false;
            }
        }

        private void saveAs()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = Resources.RedmineTableEditorFileFormat;
            var result2 = dialog.ShowDialog() ?? false;
            if (result2)
            {
                FileName = dialog.FileName;
                save();
            }
        }

        public async Task ReadAsync(string fileName)
        {
            if (!string.IsNullOrEmpty(parent.Redmine.Value.IsValid()))
                return;

            FileSettingsModel deserialized;
            try
            {
                deserialized = CloneExtentions.ToObject<FileSettingsModel>(System.IO.File.ReadAllText(fileName));

                if (deserialized.ParentIssues.Clone() != Model.Value.ParentIssues.Clone())
                {
                    if (deserialized.ParentIssues.UseQuery)
                    {
                        await parent.Redmine.Value.UpdateByQueryAsync(deserialized.ParentIssues.Query);
                    }
                    else
                    {
                        var issue = parent.Redmine.Value.GetIssue(int.Parse(deserialized.ParentIssues.IssueId));
                        await parent.Redmine.Value.UpdateByParentIssueAsync(issue);
                    }
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format(Resources.ErrMsgFailedOpenSettingsFile, fileName), e);
            }

            Model.Value = deserialized;
            FileName = fileName;
        }

        public async Task<bool> LoadFirstSettingAsync(string selectedFileName = null)
        {
            var fileName = selectedFileName != null ? selectedFileName : Settings.Default.LastFileName;
            if (string.IsNullOrEmpty(fileName))
                return false;

            if (System.IO.File.Exists(fileName))
            {
                await ReadAsync(fileName);
                return true;
            }
            else
            {
                FileName = null;
                throw new ApplicationException(string.Format(Resources.ErrMsgNotFoundPreviousFile, fileName));
            }
        }

        public bool? SaveIfNeeded()
        {
            if (!IsEdited.Value)
                return false;

            var needsSave = MessageBoxHelper.ConfirmQuestion(Resources.MsgConfirmToSave, MessageBoxHelper.ButtonType.YesNoCancel);
            if (!needsSave.HasValue)
                return null;

            if (needsSave.Value)
            {
                try
                {
                    save();
                    return true;
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
