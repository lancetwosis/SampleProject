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
        public CommandBase(string text, Bitmap largeImage, IObservable<string> canExecute, Action action)
            : base(text, largeImage, canExecute, (a) => action())
        {
        }

        /// <summary>
        /// ボタン名とアイコンだけをセットしたダミー
        /// </summary>
        public CommandBase(string text, Bitmap largeImage)
            : base(text, largeImage, new ReactivePropertySlim<string>(""), (a) => { })
        {
        }

        public CommandBase(string text, char mnemonic, Bitmap largeImage, IObservable<string> canExecute, Action action)
            : base(text, mnemonic, largeImage, canExecute, (a) => action())
        {
        }

        public CommandBase(string text, Bitmap largeImage, IObservable<string> toolTip, IObservable<string> canExecute, Action action)
            : base(text, largeImage, toolTip, canExecute, (a) => action())
        {
        }

        public CommandBase(string text, Bitmap largeImage, string toolTip, IObservable<string> canExecute, Action action)
            : base(text, largeImage, toolTip, canExecute, (a) => action())
        {
        }

        public CommandBase(string text, char mnemonic, Bitmap largeImage, string toolTip, IObservable<string> canExecute, Action action)
            : base(text, mnemonic, largeImage, toolTip, canExecute, (a) => action())
        {
        }

        /// <summary>
        /// アイコンは RadGlyph キーは App.Current.Resources["GlyphPlus"] のように取得可能（FontResources.xaml）
        /// https://docs.telerik.com/devtools/wpf/styling-and-appearance/glyphs/common-styles-appearance-glyphs-reference-sheet
        /// </summary>
        public CommandBase(string text, string glyphKey, IObservable<string> canExecute, Action action)
            : base(text, glyphKey, canExecute, (a) => action())
        {
        }

        /// <summary>
        /// アイコンなし
        /// </summary>
        public CommandBase(string text, char mnemonic, IObservable<string> canExecute, Action action)
            : base(text, mnemonic, canExecute, (a) => action())
        {
        }

        /// <summary>
        /// ドロップダウンで子供のコマンドが保持するコマンド
        /// </summary>
        public CommandBase(string text, Bitmap largeImage, IObservable<string> canExecute, ReadOnlyReactivePropertySlim<List<ChildCommand>> childCommands)
            : base(text, largeImage, canExecute, childCommands)
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

        private CommandBase(IObservable<string> canExecute, Action<T> action, string toolTip = null, IObservable<string> toolTip2 = null)
        {
            var errRp = canExecute.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            if (toolTip2 == null)
            {
                TooltipMessage = errRp.Select(e => e ?? toolTip).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            }
            else
            {
                var normalRp = toolTip2.ToReadOnlyReactivePropertySlim().AddTo(disposables);
                TooltipMessage = normalRp.CombineLatest(errRp, (n, e) => e ?? n).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            }

            IsVisibleTooltip = TooltipMessage.Select(a => !string.IsNullOrEmpty(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Command = errRp.Select(a => a == null).ToReactiveCommand<T>().WithSubscribe(action).AddTo(disposables);
        }

        private CommandBase(string text, Bitmap largeImage, IObservable<string> canExecute, Action<T> action, string toolTip, IObservable<string> toolTip2)
            : this(canExecute, action, toolTip, toolTip2)
        {
            Text = text;
            LargeImage = largeImage.ToBitmapSource();
        }

        protected CommandBase(string text, Bitmap largeImage, IObservable<string> toolTip, IObservable<string> canExecute, Action<T> action)
            : this(text, largeImage, canExecute, action, null, toolTip)
        {
        }

        protected CommandBase(string text, Bitmap largeImage, string toolTip, IObservable<string> canExecute, Action<T> action)
            : this(text, largeImage, canExecute, action, toolTip, null)
        {
        }

        protected CommandBase(string text, Bitmap largeImage, IObservable<string> canExecute, Action<T> action)
            : this(text, largeImage, canExecute, action, null, null)
        {
        }

        protected CommandBase(string text, char mnemonic, Bitmap largeImage, string toolTip, IObservable<string> canExecute, Action<T> action)
            : this(text, largeImage, canExecute, action, toolTip, null)
        {
            Mnemonic = mnemonic;
        }

        public CommandBase(string text, char mnemonic, Bitmap largeImage, IObservable<string> canExecute, Action<T> action)
            : this(text, largeImage, canExecute, action, null, null)
        {
            Mnemonic = mnemonic;
        }

        /// <summary>
        /// アイコンは RadGlyph キーは App.Current.Resources["GlyphPlus"] のように取得可能（FontResources.xaml）
        /// https://docs.telerik.com/devtools/wpf/styling-and-appearance/glyphs/common-styles-appearance-glyphs-reference-sheet
        /// </summary>
        public CommandBase(string text, string glyphKey, IObservable<string> canExecute, Action<T> action)
            : this(canExecute, action)
        {
            Text = text;
            GlyphKey = glyphKey;
        }

        /// <summary>
        /// アイコンなし。
        /// </summary>
        protected CommandBase(string text, char mnemonic, IObservable<string> canExecute, Action<T> action)
            : this(canExecute, action)
        {
            Text = text;
            Mnemonic = mnemonic;
        }

        /// <summary>
        /// ドロップダウンで子供のコマンドが保持するコマンド
        /// </summary>
        protected CommandBase(string text, Bitmap largeImage, IObservable<string> canExecute, ReadOnlyReactivePropertySlim<List<ChildCommand>> childCommands)
            : this(canExecute, (a) => { })
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
        private Func<Task> func;

        private ChildCommand(string text, IObservable<string> canExecute)
        {
            this.text = text;
            this.canExecute = canExecute.ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public ChildCommand(string text, IObservable<string> canExecute, Action action) : this(text, canExecute)
        {
            this.action = action;
        }

        public ChildCommand(string text, IObservable<string> canExecute, Func<Task> func) : this(text, canExecute)
        {
            this.func = func;
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

        public RadMenuItem ToAsyncRadMenuItem()
        {
            var menu = new RadMenuItem();
            menu.Header = text;
            menu.Command = canExecute.Select(s => string.IsNullOrEmpty(s)).ToAsyncReactiveCommand().WithSubscribe(async () => await func.Invoke()).AddTo(disposables);

            return menu;
        }
    }
}
