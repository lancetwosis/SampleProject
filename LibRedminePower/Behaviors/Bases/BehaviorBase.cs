using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace LibRedminePower.Behaviors.Bases
{
    public class BehaviorBase<T> : Behavior<T> where T : FrameworkElement
    {
        /// <summary>
        /// セットアップ状態
        /// </summary>
        private bool isSetup = false;

        /// <summary>
        /// Hook状態
        /// </summary>
        private bool isHookedUp = false;

        /// <summary>
        /// 対象オブジェクト
        /// </summary>
        private WeakReference weakTarget;

        /// <summary>
        /// Changedハンドラ
        /// </summary>
        protected override void OnChanged()
        {
            base.OnChanged();

            var target = AssociatedObject;
            if (target != null)
            {
                hookupBehavior(target);
            }
            else
            {
                unHookupBehavior();
            }
        }

        /// <summary>
        /// ビヘイビアをHookする。
        /// </summary>
        /// <param name="target"></param>
        private void hookupBehavior(T target)
        {
            if (isHookedUp)
                return;

            weakTarget = new WeakReference(target);
            target.Unloaded += onTargetUnloaded;
            target.Loaded += onTargetLoaded;
            isHookedUp = true;

            setupBehavior();
        }

        /// <summary>
        /// ビヘイビアをUnhookする。
        /// </summary>
        private void unHookupBehavior()
        {
            if (!isHookedUp)
                return;

            var target = AssociatedObject ?? (T)weakTarget.Target;
            if (target != null)
            {
                target.Unloaded -= onTargetUnloaded;
                target.Loaded -= onTargetLoaded;
            }
            isHookedUp = false;

            cleanupBehavior();
        }

        /// <summary>
        /// [関連オブジェクト] Loadedハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onTargetLoaded(object sender, RoutedEventArgs e)
        {
            setupBehavior();
        }

        /// <summary>
        /// [関連オブジェクト] Unloadedハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onTargetUnloaded(object sender, RoutedEventArgs e)
        {
            cleanupBehavior();
        }

        /// <summary>
        /// ビヘイビアのセットアップを行う。
        /// </summary>
        private void setupBehavior()
        {
            if (isSetup)
                return;

            isSetup = true;
            OnSetup();
        }

        /// <summary>
        /// ビヘイビアのクリーンアップを行う。
        /// </summary>
        private void cleanupBehavior()
        {
            if (!isSetup)
                return;

            isSetup = false;
            OnCleanup();
        }

        /// <summary>
        /// セットアップ時の処理を行う。
        /// </summary>
        protected virtual void OnSetup() { }

        /// <summary>
        /// クリーンアップ時の処理を行う。
        /// </summary>
        protected virtual void OnCleanup() { }

    }
}
