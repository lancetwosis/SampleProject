
using LibRedminePower.Applications;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.ViewModels
{
    class VersionDialogViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public string ProductName { get; set; }
        public string Version { get; set; }
        public string CopyRight{ get; set; }
        public ReactiveCommand<RadWindow> CloseCommand { get; set; }
        public VersionDialogViewModel()
        {
            ProductName = ApplicationInfo.Title;
            Version = ApplicationInfo.Version.ToString();
            CopyRight = ApplicationInfo.CopyRight;

            CloseCommand = new ReactiveCommand<RadWindow>().WithSubscribe(w => w.Close()).AddTo(disposables);
        }
    }
}
