using LibRedminePower.Applications;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Models.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RedmineTimePuncher.ViewModels.Bases
{
    public abstract class FunctionViewModelBase : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public ApplicationMode Mode { get; set; }
        public BitmapSource Icon { get; set; }

        public ReactivePropertySlim<bool> IsSelected { get; set; }
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        public ReadOnlyReactivePropertySlim<string> Title { get; set; }

        public FunctionViewModelBase(ApplicationMode mode)
        {
            Mode = mode;
            Icon = mode.GetIcon();
            IsSelected = new ReactivePropertySlim<bool>(false);
            Title = CacheManager.Default.Updated.Select(_ => getTitle()).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        protected string getTitle(string prefix = null)
        {
            var pre = !string.IsNullOrEmpty(prefix) ? $"{prefix}  " : "";
            var user = CacheManager.Default.MyUser != null ? $"{CacheManager.Default.MyUser.Name}  -  " : "";
            return $"{pre}{user}{ApplicationInfo.Title}";
        }

        /// <summary>
        /// Loaded の時の処理。基底クラスでは処理を行っていないので base(e) の実行は不要。
        /// </summary>
        public virtual void OnWindowLoaded(RoutedEventArgs e)
        {
        }

        /// <summary>
        /// WindowClosing の時の処理。基底クラスでは処理を行っていないので base(e) の実行は不要。
        /// </summary>
        public virtual void OnWindowClosing(CancelEventArgs e)
        {
        }

        /// <summary>
        /// WindowClosed の時の処理。基底クラスでは処理を行っていないので base(e) の実行は不要。
        /// </summary>
        public virtual void OnWindowClosed()
        {
        }
    }
}
