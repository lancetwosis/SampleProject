using LibRedminePower.ViewModels;
using LibRedminePower.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Telerik.Windows.Controls;

namespace LibRedminePower.ViewModels.Bases
{
    public class ColorSettingViewModelBase : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ReactivePropertySlim<Color> Color { get;  set; }
        public ReactiveCommand<RadWindow> OpenEditColorsCommand { get; set; }

        public ColorSettingViewModelBase()
        {
            OpenEditColorsCommand = new ReactiveCommand<RadWindow>().WithSubscribe(win =>
            {
                using (var vm = new EditColorsViewModel() { EditedColor = Color.Value })
                {
                    var window = new EditColorsWindow()
                    {
                        DataContext = vm,
                        Owner = win,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    };

                    window.ShowDialog();
                    if (window.DialogResult == true)
                        this.Color.Value = vm.EditedColor;
                }
            }).AddTo(disposables);
        }
    }
}
