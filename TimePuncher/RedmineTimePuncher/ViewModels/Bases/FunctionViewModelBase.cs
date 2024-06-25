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
        [JsonIgnore]
        public BitmapSource Icon { get; set; }

        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<bool> IsSelected { get; set; }
        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        [JsonIgnore]
        public ReadOnlyReactivePropertySlim<string> Title { get; set; }

        public FunctionViewModelBase() { }

        public FunctionViewModelBase(ApplicationMode mode, MainWindowViewModel parent)
        {
            Mode = mode;
            Icon = mode.GetIcon();
            IsSelected = parent.Mode.Select(m => m == mode).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            Title = parent.Redmine.Select(r => getTitle(r)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        protected string getTitle(RedmineManager r, string prefix = null)
        {
            var title = $"{CacheManager.Default.MyUser.Value?.Name}  -  {ApplicationInfo.Title}";
            return string.IsNullOrEmpty(prefix) ? title : $"{prefix}  {title}";
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
