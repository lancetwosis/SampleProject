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

namespace LibRedminePower.ViewModels
{
    public class EditColorsViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public Color EditedColor { get; set; }
        public ReactiveCommand<RadWindow> OkCommand { get; set; }
        public ReactiveCommand<RadWindow> CancelCommand { get; set; }

        public EditColorsViewModel()
        {
            OkCommand = new ReactiveCommand<RadWindow>().WithSubscribe(win => {
                win.DialogResult = true;
                win.Close();
            }).AddTo(disposables);

            CancelCommand = new ReactiveCommand<RadWindow>().WithSubscribe(win => {
                win.DialogResult = false;
                win.Close();
            }).AddTo(disposables);
        }
    }
}
