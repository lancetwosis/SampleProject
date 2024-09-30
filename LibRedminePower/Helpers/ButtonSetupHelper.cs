using LibRedminePower.Converters;
using LibRedminePower.Extentions;
using LibRedminePower.Interfaces;
using LibRedminePower.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;

namespace LibRedminePower.Helpers
{
    public class ButtonSetupHelper
    {
        public static DependencyProperty DataProperty = DependencyProperty.RegisterAttached(
            "Data", 
            typeof(ICommandBase), 
            typeof(ButtonSetupHelper), 
            new PropertyMetadata(null, dataChanged));

        // プログラムからアクセスするための添付プロパティのラッパー
        public static ICommandBase GetData(DependencyObject obj)
        {
            return (ICommandBase)obj.GetValue(DataProperty);
        }

        public static void SetData(DependencyObject obj, CommandBase value)
        {
            obj.SetValue(DataProperty, value);
        }

        private static RibbonButtonTextConverter TEXT_CONV = new RibbonButtonTextConverter();
        private static EnabledToOpacityConverter ENABLED_CONV = new EnabledToOpacityConverter();

        private static void dataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;

            var data =(ICommandBase)e.NewValue;
            if (d is Button button)
            {
                button.SetBinding(Button.CommandProperty, new Binding(nameof(data.ICommand)) { Source = data });
                button.SetBinding(Button.ToolTipProperty, new Binding(nameof(data.TooltipMessage) + ".Value") { Source = data });
                button.SetBinding(ToolTipService.IsEnabledProperty, new Binding(nameof(data.IsVisibleTooltip) + ".Value") { Source = data });
                button.SetValue(ToolTipService.ShowOnDisabledProperty, true);
                if (button is RadRibbonButton rButton)
                {
                    rButton.SetBinding(RadRibbonButton.LargeImageProperty, new Binding(nameof(data.LargeImage)) { Source = data});

                    if (data.Text != null)
                    {
                        rButton.SetBinding(RadRibbonButton.TextProperty, new Binding(nameof(rButton.CurrentSize))
                        {
                            Converter = TEXT_CONV,
                            ConverterParameter = data.Text,
                            Source = rButton,
                        });
                    }
                }
                else if (button is RadRibbonDropDownButton ddButton)
                {
                    ddButton.SetBinding(RadRibbonDropDownButton.LargeImageProperty, new Binding(nameof(data.LargeImage)) { Source = data });

                    if (data.Text != null)
                    {
                        ddButton.SetBinding(RadRibbonDropDownButton.TextProperty, new Binding(nameof(rButton.CurrentSize))
                        {
                            Converter = TEXT_CONV,
                            ConverterParameter = data.Text,
                            Source = ddButton,
                        });
                    }

                    var ddContent = new RadContextMenu();
                    var menus = data.GetChildRadMenus();
                    ddContent.SetBinding(RadContextMenu.ItemsSourceProperty, new Binding("Value") { Source = menus });
                    ddButton.DropDownContent = ddContent;
                }
            }
            else if (d is RadRibbonSplitButton sButton)
            {
                sButton.SetBinding(RadRibbonSplitButton.CommandProperty, new Binding(nameof(data.ICommand)) { Source = data });
                sButton.SetBinding(RadRibbonSplitButton.ToolTipProperty, new Binding(nameof(data.TooltipMessage) + ".Value") { Source = data });
                sButton.SetBinding(ToolTipService.IsEnabledProperty, new Binding(nameof(data.IsVisibleTooltip) + ".Value") { Source = data });
                sButton.SetValue(ToolTipService.ShowOnDisabledProperty, true);
                sButton.SetBinding(RadRibbonSplitButton.LargeImageProperty, new Binding(nameof(data.LargeImage)) { Source = data });

                if (data.Text != null)
                {
                    sButton.SetBinding(RadRibbonSplitButton.TextProperty, new Binding(nameof(sButton.CurrentSize))
                    {
                        Converter = TEXT_CONV,
                        ConverterParameter = data.Text,
                        Source = sButton,
                    });
                }

                // 画面のレイアウトのためダミーを表示する場合があるためチェックを行う
                if (data.ChildCommands != null)
                {
                    var ddContent = new RadContextMenu();
                    var menus = data.GetChildRadMenus();
                    ddContent.SetBinding(RadContextMenu.ItemsSourceProperty, new Binding("Value") { Source = menus });
                    sButton.DropDownContent = ddContent;
                }
            }
            else if (d is MenuItem menuItem)
            {
                menuItem.SetBinding(MenuItem.CommandProperty, new Binding(nameof(data.ICommand)) { Source = data });
                menuItem.SetBinding(MenuItem.ToolTipProperty, new Binding(nameof(data.TooltipMessage) + ".Value") { Source = data });
                menuItem.SetBinding(ToolTipService.IsEnabledProperty, new Binding(nameof(data.IsVisibleTooltip) + ".Value") { Source = data });
                menuItem.SetValue(ToolTipService.ShowOnDisabledProperty, true);
                if (data.LargeImage != null)
                {
                    var image = new Image() { Source = data.LargeImage };
                    image.SetBinding(Image.OpacityProperty, new Binding(nameof(menuItem.IsEnabled)) { Source = menuItem, Converter = ENABLED_CONV });
                    menuItem.SetValue(MenuItem.IconProperty, image);
                }
                if (data.GlyphKey != null)
                {
                    var glyph = new RadGlyph() { Glyph = data.GlyphKey };
                    menuItem.SetValue(MenuItem.IconProperty, glyph);
                }
                if (data.Text != null)
                {
                    var text = data.MenuText != null ? data.MenuText : data.Text.Replace(Environment.NewLine, "");
                    if (data.Mnemonic != default)
                        text = $"{text}(_{data.Mnemonic})";

                    menuItem.SetValue(MenuItem.HeaderProperty, text);
                }
                if (data.ChildCommands != null)
                {
                    var menus = data.GetChildMenus();
                    menuItem.SetBinding(MenuItem.ItemsSourceProperty, new Binding("Value") { Source = menus });
                }
            }
            else
            {
                throw new NotSupportedException($"{d.GetType()} is not supported by ButtonSetupHelper");
            }
        }
    }
}
