using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class ColorEx
    {
        private const int COLOR_MAX = 255;
        private const double CONTRAST_MIN = 4.5;

        public static System.Windows.Media.Color ToMediaColor(this Color color)
            => System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        public static System.Windows.Media.SolidColorBrush ToBrush(this Color color)
            => new System.Windows.Media.SolidColorBrush(color.ToMediaColor());
        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color mColor) =>
            System.Drawing.Color.FromArgb(mColor.A, mColor.R, mColor.G, mColor.B);

        public static Color GetRandColor(string name)
        {
            var random = new Random(name.GetHashCode() + 1);
            double backLumiance = relativeLuminance(Color.Black);
            double contrast = 0.0;
            Color resultColor;
            do
            {
                var color = Color.FromArgb(COLOR_MAX, random.Next(0, COLOR_MAX), random.Next(0, COLOR_MAX), random.Next(0, COLOR_MAX));
                resultColor = Color.FromArgb(COLOR_MAX, color.R, color.G, color.B);
                contrast = relativeLuminanceRatio(backLumiance, relativeLuminance(resultColor));
            }
            while (contrast < CONTRAST_MIN);

            return resultColor;
        }

        /// <summary>
        /// 背景色をもとにして「黒」か「白」かコントラスト比が大きい方を返す
        /// </summary>
        public static Color ToTextColor(this Color backgroundColor)
        {
            // 背景色の相対輝度
            double background = relativeLuminance(backgroundColor);

            const double white = 1.0D;  // 白の相対輝度
            const double black = 0.0D;  // 黒の相対輝度

            // 文字色と背景色のコントラスト比を計算
            double whiteContrast = relativeLuminanceRatio(white, background);   // 文字色：白との比
            double blackContrast = relativeLuminanceRatio(black, background);   // 文字色：黒との比

            // コントラスト比が大きい文字色を採用
            return whiteContrast < blackContrast ? Color.Black : Color.White;
        }

        private static double relativeLuminanceRatio(double relativeLuminance1, double relativeLuminance2)
        {
            return (Math.Max(relativeLuminance1, relativeLuminance2) + 0.05) / (Math.Min(relativeLuminance1, relativeLuminance2) + 0.05);
        }

        private static double relativeLuminance(Color color)
        {
            Func<byte, double> toRgb = (rgb) =>
            {
                var srgb = (double)rgb / COLOR_MAX;
                return srgb <= 0.03928 ? srgb / 12.92 : Math.Pow((srgb + 0.055) / 1.055, 2.4);
            };
            return 0.2126 * toRgb(color.R) + 0.7152 * toRgb(color.G) + 0.0722 * toRgb(color.B);
        }
    }
}
