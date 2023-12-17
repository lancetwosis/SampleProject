using LibRedminePower.Interfaces;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.ViewModels.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RedmineTimePuncher.ViewModels.TableEditor
{
    public class TableEditorViewModel : FunctionViewModelBase
    {
        public RedmineTableEditor.ViewModels.TableEditorViewModel ViewModel { get; set; }

        public static string OPTION_KEY = "-eit";

        public TableEditorViewModel(MainWindowViewModel parent)
            : base(ApplicationMode.TableEditor, parent)
        {
            var redmine = parent.Redmine.Select(r => (r?.Manager, r?.MasterManager, (ICacheManager)CacheManager.Default)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            ViewModel = new RedmineTableEditor.ViewModels.TableEditorViewModel(redmine).AddTo(disposables);

            IsSelected.Subscribe(i =>
            {
                if (i)
                    ViewModel.LoadFirstSettings(fileName);
            }).AddTo(disposables);

            Title = CacheManager.Default.MyUser.CombineLatest(ViewModel.TitlePrefix, (u, p) => $"{p}  {getTitle(u)}").ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        private string fileName;
        public void SetFirstSettings(string fileName)
        {
            this.fileName = fileName;
        }

        public override void OnWindowClosing(CancelEventArgs e)
        {
            ViewModel.OnWindowClosing(e);
        }
    }
}
