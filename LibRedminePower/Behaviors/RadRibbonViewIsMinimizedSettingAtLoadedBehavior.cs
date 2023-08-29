using LibRedminePower.Behaviors.Bases;
using System;
using System.Xaml;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using System.Windows;
using Telerik.Windows.Controls;
using LibRedminePower.Extentions;
using LibRedminePower;
using System.Windows.Data;
using AngleSharp.Dom;

namespace LibRedminePower.Behaviors
{
    /// <summary>
    /// #486の回避策
    /// RadRibbonView.IsMinimized="true"　かつ RadRibbonTab.IsSelected="true"の状態で起動を行うと、エラーが発生するというtelerikのバグがある。
    /// RadRibbonView の Loaded 後に IsMinimized を true に設定すれば回避できる
    /// </summary>
    public class RadRibbonViewIsMinimizedSettingAtLoadedBehavior : BehaviorBase<RadRibbonView>
    {
        private bool isMinimized;
        protected override void OnSetup()
        {
            base.OnSetup();
            isMinimized = AssociatedObject.IsMinimized;
            // エラー回避のため、一度 false に設定する
            AssociatedObject.IsMinimized = false;
            AssociatedObject.Loaded += onLoadWithIsMinimizedBinding;
        }

        protected override void OnCleanup()
        {
            AssociatedObject.Unloaded -= onLoadWithIsMinimizedBinding;
            base.OnCleanup();
        }

        private void onLoadWithIsMinimizedBinding(object sender, EventArgs e)
        {
            var ribbonView = (sender as RadRibbonView);
            ribbonView.IsMinimized = isMinimized;    
        }
    }
}
