using LibRedminePower.Extentions;
using LibRedminePower.Interfaces;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls;

namespace LibRedminePower.ViewModels
{
    public class CommandBase : CommandBase<object>
    {
        public CommandBase(string text, Bitmap largeImage, IObservable<string> errTooltip, Action action)
            : base(text, largeImage, Observable.Return(""), errTooltip, (a) => action())
        {
        }

        /// <summary>
        /// ボタン名とアイコンだけをセットしたダミー
        /// </summary>
        public CommandBase(string text, Bitmap largeImage)
            : base(text, largeImage, Observable.Return(""), Observable.Return(""), (a) => { })
        {
        }

        public CommandBase(string text, char mnemonic, Bitmap largeImage, IObservable<string> errTooltip, Action action)
            : base(text, mnemonic, largeImage, Observable.Return(""), errTooltip, (a) => action())
        {
        }

        public CommandBase(string text, Bitmap largeImage, IObservable<string> normalTooltip, IObservable<string> errTooltip, Action action)
            : base(text, largeImage, normalTooltip, errTooltip, (a) => action())
        {
        }

        public CommandBase(string text, char mnemonic, Bitmap largeImage, IObservable<string> normalTooltip, IObservable<string> errTooltip, Action action)
            : base(text, mnemonic, largeImage, normalTooltip, errTooltip, (a) => action())
        {
        }

        /// <summary>
        /// アイコンは RadGlyph キーは App.Current.Resources["GlyphPlus"] のように取得可能（FontResources.xaml）
        /// https://docs.telerik.com/devtools/wpf/styling-and-appearance/glyphs/common-styles-appearance-glyphs-reference-sheet
        /// </summary>
        public CommandBase(string text, string glyphKey, IObservable<string> errTooltip, Action action)
            : base(text, glyphKey, errTooltip, (a) => action())
        {
        }

        /// <summary>
        /// アイコンなし
        /// </summary>
        public CommandBase(string text, IObservable<string> errTooltip, Action action)
            : base(text, errTooltip, (a) => action())
        {
        }

        /// <summary>
        /// アイコンなし
        /// </summary>
        public CommandBase(string text, char mnemonic, IObservable<string> errTooltip, Action action)
            : base(text, mnemonic, errTooltip, (a) => action())
        {
        }

        /// <summary>
        /// ドロップダウンで子供のコマンドが保持するコマンド
        /// </summary>
        public CommandBase(string text, Bitmap largeImage, IObservable<string> errTooltip, ReadOnlyReactivePropertySlim<List<ChildCommand>> childCommands)
            : base(text, largeImage, errTooltip, childCommands)
        {
        }
    }

    public class CommandBase<T> : Bases.ViewModelBase, ICommandBase
    {
        public ReadOnlyReactivePropertySlim<string> TooltipMessage { get; }
        public ReadOnlyReactivePropertySlim<bool> IsVisibleTooltip { get; set; }
        public ReactiveCommand<T> Command { get; set; }
        public ICommand ICommand { get => Command;  }

        public string Text { get; set; }
        public string MenuText { get; set; }
        public char Mnemonic { get; set; }

        public BitmapSource LargeImage { get; set; }
        public string GlyphKey { get; set; }

        public ReadOnlyReactivePropertySlim<List<ChildCommand>> ChildCommands { get; set; }

        public CommandBase(IObservable<string> normalMessage, IObservable<string> errorMessage, Action<T> action)
        {
            var normalRp = normalMessage.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var errRp = errorMessage.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            TooltipMessage = normalRp.CombineLatest(errRp, (n, e) => e != null ? e : n).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            IsVisibleTooltip = TooltipMessage.Select(a => !string.IsNullOrEmpty(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Command = errRp.Select(a => a == null).ToReactiveCommand<T>().WithSubscribe(action).AddTo(disposables);
        }

        public CommandBase(IObservable<string> normalMessage, IObservable<string> errorMessage, Action<T> action, Bitmap largeImage)
            : this(normalMessage, errorMessage, action)
        {
            LargeImage = largeImage.ToBitmapSource();
        }

        public CommandBase(string text, Bitmap largeImage, IObservable<string> normalTooltip, IObservable<string> errTooltip, Action<T> action)
            : this(normalTooltip, errTooltip, action)
        {
            Text = text;
            LargeImage = largeImage.ToBitmapSource();
        }

        public CommandBase(string text, char mnemonic, Bitmap largeImage, IObservable<string> normalTooltip, IObservable<string> errTooltip, Action<T> action)
            : this(text, largeImage, normalTooltip, errTooltip, action)
        {
            Mnemonic = mnemonic;
        }

        public CommandBase(string text, Bitmap largeImage, IObservable<string> errTooltip, Action<T> action)
            : this(Observable.Return(""), errTooltip, action)
        {
            Text = text;
            LargeImage = largeImage.ToBitmapSource();
        }

        public CommandBase(string text, char mnemonic, Bitmap largeImage, IObservable<string> errTooltip, Action<T> action)
            : this(text, largeImage, errTooltip, action)
        {
            Mnemonic = mnemonic;
        }

        /// <summary>
        /// アイコンは RadGlyph キーは App.Current.Resources["GlyphPlus"] のように取得可能（FontResources.xaml）
        /// https://docs.telerik.com/devtools/wpf/styling-and-appearance/glyphs/common-styles-appearance-glyphs-reference-sheet
        /// </summary>
        public CommandBase(string text, string glyphKey, IObservable<string> errTooltip, Action<T> action)
            : this(Observable.Return(""), errTooltip, action)
        {
            Text = text;
            GlyphKey = glyphKey;
        }

        /// <summary>
        /// アイコンなし
        /// </summary>
        public CommandBase(string text, IObservable<string> errTooltip, Action<T> action)
            : this(Observable.Return(""), errTooltip, action)
        {
            Text = text;
        }

        /// <summary>
        /// アイコンなし。
        /// </summary>
        public CommandBase(string text, char mnemonic, IObservable<string> errTooltip, Action<T> action)
            : this(text, errTooltip, action)
        {
            Mnemonic = mnemonic;
        }

        /// <summary>
        /// ドロップダウンで子供のコマンドが保持するコマンド
        /// </summary>
        public CommandBase(string text, Bitmap largeImage, IObservable<string> errTooltip, ReadOnlyReactivePropertySlim<List<ChildCommand>> childCommands)
            : this(Observable.Return(""), errTooltip, (a) => { })
        {
            Text = text;
            LargeImage = largeImage.ToBitmapSource();
            ChildCommands = childCommands;
        }

        public ReadOnlyReactivePropertySlim<List<MenuItem>> GetChildMenus()
        {
            return ChildCommands.Select(cmds => cmds.Select(c => c.ToMenuItem()).ToList()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public ReadOnlyReactivePropertySlim<List<RadMenuItem>> GetChildRadMenus()
        {
            return ChildCommands.Select(cmds => cmds.Select(c => c.ToRadMenuItem()).ToList()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }
    }

    public class ChildCommand : Bases.ViewModelBase
    {
        private string text;
        private ReadOnlyReactivePropertySlim<string> canExecute;
        private Action action;

        public ChildCommand(string text, IObservable<string> canExecute, Action action)
        {
            this.text = text;
            this.canExecute = canExecute.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            this.action = action;
        }

        public MenuItem ToMenuItem()
        {
            var menu = new MenuItem();
            menu.Header = text;
            menu.Command = canExecute.Select(s => string.IsNullOrEmpty(s)).ToReactiveCommand().WithSubscribe(() => action.Invoke()).AddTo(disposables);

            return menu;
        }

        public RadMenuItem ToRadMenuItem()
        {
            var menu = new RadMenuItem();
            menu.Header = text;
            menu.Command = canExecute.Select(s => string.IsNullOrEmpty(s)).ToReactiveCommand().WithSubscribe(() => action.Invoke()).AddTo(disposables);

            return menu;
        }
    }
}
