using LibRedminePower.ViewModels;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls;

namespace LibRedminePower.Interfaces
{
    public interface ICommandBase
    {
        ReadOnlyReactivePropertySlim<string> TooltipMessage { get; }
        ReadOnlyReactivePropertySlim<bool> IsVisibleTooltip { get; set; }
        ICommand ICommand { get; }
        string Text { get; set; }
        string MenuText { get; set; }
        char Mnemonic { get; set; }
        BitmapSource LargeImage { get; set; }
        string GlyphKey { get; set; }
        ReadOnlyReactivePropertySlim<List<ChildCommand>> ChildCommands { get; set; }
        ReadOnlyReactivePropertySlim<List<MenuItem>> GetChildMenus();
        ReadOnlyReactivePropertySlim<List<RadMenuItem>> GetChildRadMenus();
    }
}
