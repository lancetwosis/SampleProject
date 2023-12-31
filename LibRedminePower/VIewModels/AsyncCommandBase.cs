﻿using LibRedminePower.Extentions;
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
    public class AsyncCommandBase : Bases.ViewModelBase, ICommandBase
    {
        public ReadOnlyReactivePropertySlim<string> TooltipMessage { get; }
        public ReadOnlyReactivePropertySlim<bool> IsVisibleTooltip { get; set; }

        public AsyncReactiveCommand Command { get; set; }
        public ICommand ICommand { get => Command; }
        public string Text { get; set; }
        public string MenuText { get; set; }
        public char Mnemonic { get; set; }
        public BitmapSource LargeImage { get; set; }
        public string GlyphKey { get; set; }

        // 非同期の場合、使用しない
        public ReadOnlyReactivePropertySlim<List<ChildCommand>> ChildCommands { get; set; }

        /// <summary>
        /// ワーニングの有無により、アイコン及びツールチップを切り替えたい場合に使用すること。
        /// </summary>
        public AsyncCommandBase(string text, Func<string, Bitmap> iconCreater, string normalTooltip, IObservable<string> warnTooltip, IObservable<string> errTooltip, Func<Task> asyncFunc)
        {
            TooltipMessage = errTooltip.CombineLatest(warnTooltip, (e, w) => (e != null ? e : (w != null) ? w : normalTooltip)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            IsVisibleTooltip = TooltipMessage.Select(a => !string.IsNullOrEmpty(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Command = errTooltip.Select(a => a == null).ToAsyncReactiveCommand().WithSubscribe(asyncFunc).AddTo(disposables);
            Text = text;
            warnTooltip.SubscribeWithErr(w => LargeImage = iconCreater.Invoke(w).ToBitmapSource());
        }

        private AsyncCommandBase(string text, IObservable<string> canExecute, Func<Task> asyncFunc)
        {
            TooltipMessage = canExecute.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            IsVisibleTooltip = TooltipMessage.Select(a => !string.IsNullOrEmpty(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Command = canExecute.Select(a => a == null).ToAsyncReactiveCommand().WithSubscribe(asyncFunc).AddTo(disposables);
            Text = text;
        }

        public AsyncCommandBase(string text, Bitmap icon, IObservable<string> canExecute,  Func<Task> asyncFunc)
            : this(text, canExecute, asyncFunc)
        {
            LargeImage = icon.ToBitmapSource();
        }

        /// <summary>
        /// ボタン名とアイコンだけをセットしたダミー
        /// </summary>
        public AsyncCommandBase(string text, Bitmap icon)
            : this(text, icon, Observable.Return(""), () => Task.CompletedTask)
        {
        }

        /// <summary>
        /// アイコンを RadGlyph キーで指定。キーは App.Current.Resources["GlyphPlus"] のように取得可能（FontResources.xaml に定義されている）
        /// https://docs.telerik.com/devtools/wpf/styling-and-appearance/glyphs/common-styles-appearance-glyphs-reference-sheet
        /// </summary>
        public AsyncCommandBase(string text, char mnemonic, string glyphKey, IObservable<string> canExecute, Func<Task> asyncFunc)
            : this(text, canExecute, asyncFunc)
        {
            GlyphKey = glyphKey;
            Mnemonic = mnemonic;
        }

        /// 以下は、非同期のコマンドが子供となるコマンドを保持することはないので未定義
        public ReadOnlyReactivePropertySlim<List<MenuItem>> GetChildMenus()
        {
            throw new NotImplementedException();
        }
        public ReadOnlyReactivePropertySlim<List<RadMenuItem>> GetChildRadMenus()
        {
            throw new NotImplementedException();
        }
    }
}
