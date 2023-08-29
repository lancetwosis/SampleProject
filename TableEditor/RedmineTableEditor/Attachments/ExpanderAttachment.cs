using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace RedmineTableEditor.Attachments
{
    public static class ExpanderAttachment
    {
        public enum GridSnapMode
        {
            None,
            Auto,
            Explicit,
        }

        private static BooleanToVisibilityConverter BoolToVisible { get; }
            = new BooleanToVisibilityConverter();

        public static GridSnapMode GetGridSnap(Expander obj)
        {
            return (GridSnapMode)obj.GetValue(GridSnapProperty);
        }

        public static void SetGridSnap(Expander obj, GridSnapMode value)
        {
            obj.SetValue(GridSnapProperty, value);
        }

        // Using a DependencyProperty as the backing store for GridSnap.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridSnapProperty =
            DependencyProperty.RegisterAttached("GridSnap",
                typeof(GridSnapMode),
                typeof(ExpanderAttachment),
                new PropertyMetadata(GridSnapMode.None,
                (d, e) =>
                {
                    if (!(e.NewValue is GridSnapMode mode)) { return; }
                    if (!(d is Expander expander)) { return; }

                    // コントロールを取得したいため少し待ってから処理開始
                    expander.Dispatcher.BeginInvoke((Action)(async () =>
                    {
                        await Task.Delay(500);

                        // 横開きモードか縦開きモードかで分岐
                        switch (expander.ExpandDirection)
                        {
                            case ExpandDirection.Down:
                            case ExpandDirection.Up:
                                expander.AttachMode_Vertical(mode);
                                break;
                            case ExpandDirection.Left:
                            case ExpandDirection.Right:
                                expander.AttachMode_Horizontal(mode);
                                break;
                            default:
                                break;
                        }
                    }));
                }));

        public static GridSplitter GetTargetGridSplitter(DependencyObject obj)
        {
            return (GridSplitter)obj.GetValue(TargetGridSplitterProperty);
        }

        public static void SetTargetGridSplitter(DependencyObject obj, GridSplitter value)
        {
            obj.SetValue(TargetGridSplitterProperty, value);
        }

        // Using a DependencyProperty as the backing store for TargetGridSplitter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetGridSplitterProperty =
            DependencyProperty.RegisterAttached("TargetGridSplitter", typeof(GridSplitter), typeof(ExpanderAttachment), new PropertyMetadata(null));

        private static void AttachMode_Vertical(this Expander expander, GridSnapMode mode)
        {
            var targetGrid = expander.FindAncestor<Grid>();
            if (targetGrid == null) { return; }

            expander.Expanded -= Expanded_Vertical;
            expander.Collapsed -= Collapsed_Vertical;

            if (mode == GridSnapMode.None)
            {
                return;
            }

            // ExpanderのGrid.Rowから
            // 高さを変更したいRowDefinitionを取得
            var gridRow = Grid.GetRow(expander);
            var targetRowDefinition = targetGrid.RowDefinitions[gridRow];
            SetTargetRowDefinition(expander, targetRowDefinition);
            SetLastGridLength(expander, targetRowDefinition.Height);

            // GridSplitterを取得
            var gridSplitter = mode == GridSnapMode.Auto
                ? targetGrid.FindDescendant<GridSplitter>()
                : GetTargetGridSplitter(expander);
            if (mode == GridSnapMode.Explicit && gridSplitter == null)
            {
                throw new ArgumentException("Mode 'Explicit' requires 'TargetGridSplitter'", "TargetGridSplitter");
            }
            // 表示切替はBindingで行う
            gridSplitter.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(Expander.IsExpanded))
            {
                Mode = BindingMode.OneWay,
                Converter = BoolToVisible,
                Source = expander,
            });

            expander.Expanded += Expanded_Vertical;
            expander.Collapsed += Collapsed_Vertical;
        }

        private static void Collapsed_Vertical(object sender, RoutedEventArgs e)
        {
            if (!(sender is Expander expander)) { return; }
            // 閉じる前の高さを保存し
            // 高さをAutoに戻す
            var targetRowDefinition = GetTargetRowDefinition(expander);
            SetLastGridLength(expander, targetRowDefinition.Height);
            targetRowDefinition.Height = GridLength.Auto;
        }

        private static void Expanded_Vertical(object sender, RoutedEventArgs e)
        {
            if (!(sender is Expander expander)) { return; }
            // 前に閉じたときの高さ値が残っていたらそれを復元
            GetTargetRowDefinition(expander).Height = GetLastGridLength(expander);
        }

        private static void AttachMode_Horizontal(this Expander expander, GridSnapMode mode)
        {
            var targetGrid = expander.FindAncestor<Grid>();
            if (targetGrid == null) { return; }

            expander.Expanded -= Expanded_Horizontal;
            expander.Collapsed -= Collapsed_Horizontal;

            if (mode == GridSnapMode.None)
            {
                return;
            }

            // ExpanderのGrid.Columnから
            // 幅を変更したいColumnDefinitionを取得
            var gridColumn = Grid.GetColumn(expander);
            var targetColumnDefinition = targetGrid.ColumnDefinitions[gridColumn];
            SetTargetColumnDefinition(expander, targetColumnDefinition);
            SetLastGridLength(expander, targetColumnDefinition.Width);

            // GridSplitterを取得
            var gridSplitter = mode == GridSnapMode.Auto
                ? targetGrid.FindDescendant<GridSplitter>()
                : GetTargetGridSplitter(expander);
            if (mode == GridSnapMode.Explicit && gridSplitter == null)
            {
                throw new ArgumentException("Mode 'Explicit' requires 'TargetGridSplitter'", "TargetGridSplitter");
            }
            // 表示切替はBindingで行う
            gridSplitter.SetBinding(UIElement.VisibilityProperty, new Binding(nameof(Expander.IsExpanded))
            {
                Mode = BindingMode.OneWay,
                Converter = BoolToVisible,
                Source = expander,
            });

            expander.Expanded += Expanded_Horizontal;
            expander.Collapsed += Collapsed_Horizontal;
        }

        private static void Collapsed_Horizontal(object sender, RoutedEventArgs e)
        {
            if (!(sender is Expander expander)) { return; }
            // 閉じる前の幅を保存し
            // 幅をAutoに戻す
            var targetColumnDefinition = GetTargetColumnDefinition(expander);
            SetLastGridLength(expander, targetColumnDefinition.Width);
            targetColumnDefinition.Width = GridLength.Auto;
        }

        private static void Expanded_Horizontal(object sender, RoutedEventArgs e)
        {
            if (!(sender is Expander expander)) { return; }
            // 前に閉じたときの高さ値が残っていたらそれを復元
            GetTargetColumnDefinition(expander).Width = GetLastGridLength(expander);
        }

        private static GridLength GetLastGridLength(Expander obj)
        {
            return (GridLength)obj.GetValue(LastGridLengthProperty);
        }

        private static void SetLastGridLength(Expander obj, GridLength value)
        {
            obj.SetValue(LastGridLengthProperty, value);
        }

        // Using a DependencyProperty as the backing store for LastGridLength.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty LastGridLengthProperty =
            DependencyProperty.RegisterAttached("LastGridLength", typeof(GridLength), typeof(ExpanderAttachment), new PropertyMetadata(GridLength.Auto));



        private static RowDefinition GetTargetRowDefinition(Expander obj)
        {
            return (RowDefinition)obj.GetValue(TargetRowDefinitionProperty);
        }

        private static void SetTargetRowDefinition(Expander obj, RowDefinition value)
        {
            obj.SetValue(TargetRowDefinitionProperty, value);
        }

        // Using a DependencyProperty as the backing store for TargetRowDefinition.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty TargetRowDefinitionProperty =
            DependencyProperty.RegisterAttached("TargetRowDefinition", typeof(RowDefinition), typeof(ExpanderAttachment), new PropertyMetadata(null));



        private static ColumnDefinition GetTargetColumnDefinition(Expander obj)
        {
            return (ColumnDefinition)obj.GetValue(TargetColumnDefinitionProperty);
        }

        private static void SetTargetColumnDefinition(Expander obj, ColumnDefinition value)
        {
            obj.SetValue(TargetColumnDefinitionProperty, value);
        }

        // Using a DependencyProperty as the backing store for TargetColumnDefinition.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty TargetColumnDefinitionProperty =
            DependencyProperty.RegisterAttached("TargetColumnDefinition", typeof(ColumnDefinition), typeof(ExpanderAttachment), new PropertyMetadata(null));

        public static T FindAncestor<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            while (depObj != null)
            {
                if (depObj is T target)
                {
                    return target;
                }
                depObj = VisualTreeHelper.GetParent(depObj);
            }
            return null;
        }

        public static T FindDescendant<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) { return null; }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? FindDescendant<T>(child);
                if (result != null) { return result; }
            }
            return null;
        }
    }
}