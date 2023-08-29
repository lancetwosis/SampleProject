using LibRedminePower.Extentions;
using LibRedminePower.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Telerik.Windows.Controls.RibbonView;

namespace LibRedminePower.Converters
{
    public class RibbonButtonTextConverter : IValueConverter
    {
        // 英語の場合、ボタン名が長い場合には「 」（半角スペース）で自動で改行してくれる。しかし、以下のような問題がある
        // ・日本語の場合、改行してくれない。
        // ・リソースに改行を入れると一行で表示されたとき、それは半角スペースに変換される。
        // 上記の問題を解決するため、以下の対応を行う
        // ・日本語の場合、改行したければリソースに改行を入れる
        // ・日本語の場合、半角スペースではなく空文字で結合されるように本コンバーターを追加する
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = (ButtonSize)value;
            var text = parameter as string;
            if (size == ButtonSize.Large)
            {
                return text;
            }
            else
            {
                if (CultureInfo.CurrentCulture.IsJp())
                {
                    return text.Replace(Environment.NewLine, "");
                }
                else
                {
                    return text.Replace(Environment.NewLine, " ");
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
