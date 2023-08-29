using LibRedminePower.Applications;
using System;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace LibRedminePower.Helpers
{
    public static class MessageBoxHelper
    {
        public enum ButtonType
        {
            Ok,
            OkCancel,
            YesNoCancel,
        }

        public enum IconType
        {
            Information,
            Warning,
            Error,
            Question,
        }

        public static bool? ConfirmInformation(string message, ButtonType button = ButtonType.Ok)
        => Show(message, IconType.Warning, button);

        public static bool? ConfirmWarning(string message, ButtonType button = ButtonType.Ok)
            => Show(message, IconType.Warning, button);

        public static bool? ConfirmError(string message, ButtonType button = ButtonType.Ok)
            => Show(message, IconType.Error, button);

        public static bool? ConfirmQuestion(string message, ButtonType button = ButtonType.OkCancel)
            => Show(message, IconType.Question, button);

        public static bool? Show(object message, IconType icon, ButtonType button)
        {
            var parameter = new DialogParameters();

            parameter.WindowStyle = (Style)Application.Current.Resources["ConfirmWindowStyle"];
            parameter.Header = ApplicationInfo.Title;
            parameter.Content = message;
            parameter.DefaultFocusedButton = ResponseButton.None;
            parameter.IconTemplate = createIcon(icon);
            parameter.ContentStyle = createStyle(button);

            return show(parameter);
        }

        private static DataTemplate createIcon(IconType type)
        {

            switch (type)
            {
                case IconType.Information:
                    return (DataTemplate)Application.Current.Resources["ConfirmInformationIconTemplate"];
                case IconType.Warning:
                    return (DataTemplate)Application.Current.Resources["ConfirmWarningIconTemplate"];
                case IconType.Error:
                    return (DataTemplate)Application.Current.Resources["ConfirmErrorIconTemplate"];
                case IconType.Question:
                    return (DataTemplate)Application.Current.Resources["ConfirmQuestionIconTemplate"];
                default:
                    throw new InvalidProgramException();
            }
        }

        private static Style createStyle(ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Ok:
                    return (Style)Application.Current.Resources["ConfirmOkContentStyle"];
                case ButtonType.OkCancel:
                    return (Style)Application.Current.Resources["ConfirmOkCancelContentStyle"];
                case ButtonType.YesNoCancel:
                    return (Style)Application.Current.Resources["ConfirmYesNoCancelContentStyle"];
                default:
                    throw new InvalidProgramException();
            }
        }

        private static bool? show(DialogParameters parameter)
        {
            bool? exeFlag = null;

            var win = getMainWindow();
            if (win != null)
            {
                parameter.Owner = win;
                parameter.DialogStartupLocation = WindowStartupLocation.CenterOwner;
                parameter.Closed = (s, a) => exeFlag = a.DialogResult;
                RadWindow.Confirm(parameter);
            }
            else
            {
                parameter.Owner = null;
                parameter.DialogStartupLocation = WindowStartupLocation.CenterScreen;
                parameter.Closed = (s, a) => exeFlag = a.DialogResult;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RadWindow.Confirm(parameter);
                });
            }
            return exeFlag;
        }

        private static Window getMainWindow()
        {
            try
            {
                if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
                    return Application.Current.MainWindow;
            }
            catch (Exception) { }
            return null;
        }

        public static string Input(string message, string defautValue = null, bool isReadOnly = false)
        {
            var parameter = new DialogParameters();

            parameter.WindowStyle = (Style)Application.Current.Resources["ConfirmWindowStyle"];
            parameter.Header = ApplicationInfo.Title;
            parameter.DefaultFocusedButton = ResponseButton.None;
            parameter.IconTemplate = createIcon(isReadOnly ? IconType.Information : IconType.Question);
            parameter.ContentStyle = createStyle(isReadOnly ? ButtonType.Ok : ButtonType.OkCancel);

            var panel = new StackPanel() { Orientation = Orientation.Vertical };
            panel.Children.Add(new TextBlock() { Text = message, Margin = new Thickness(0, 5, 0, 0) });
            var textBox = new TextBox()
            {
                Margin = new Thickness(5, 5, 0, 0),
                TextWrapping = TextWrapping.WrapWithOverflow,
                MaxHeight = 300,
                IsReadOnly = isReadOnly,
                Text = defautValue,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            panel.Children.Add(textBox);
            parameter.Content = panel;

            var result = show(parameter);
            if (result.HasValue && result.Value)
                return textBox.Text;
            else
                return null;
        }

        public static int? Input(string message, int initial, int min, int max)
        {
            var parameter = new DialogParameters();

            parameter.WindowStyle = (Style)Application.Current.Resources["ConfirmWindowStyle"];
            parameter.Header = ApplicationInfo.Title;
            parameter.DefaultFocusedButton = ResponseButton.None;
            parameter.IconTemplate = createIcon(IconType.Question);
            parameter.ContentStyle = createStyle(ButtonType.OkCancel);

            var panel = new StackPanel() { Orientation = Orientation.Vertical };
            panel.Children.Add(new TextBlock() { Text = message, Margin = new Thickness(0, 5, 0, 0) });
            var updown = new RadNumericUpDown()
            {
                Margin = new Thickness(5, 5, 0, 0),
                Value = initial,
                Minimum = min,
                Maximum = max,
                IsInteger = true,
            };
            panel.Children.Add(updown);
            parameter.Content = panel;

            var result = show(parameter);
            if (result.HasValue && result.Value)
                return (int)updown.Value;
            else
                return null;
        }
    }
}
