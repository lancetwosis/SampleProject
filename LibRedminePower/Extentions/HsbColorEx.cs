using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LibRedminePower.Extentions
{
    // HSB カラーと RGB カラーをコンバートするためのクラス
    // https://qiita.com/soi/items/11a5c232d47585a3f83e
    public class HsbColor
    {
        private double _hue;
        /// <summary>
        /// 色相 0~360
        /// </summary>
        public double Hue
        {
            get => _hue;
            set => _hue = Math.Max(0, Math.Min(value, 360));
        }

        private double saturation;
        /// <summary>
        /// 彩度 0~1
        /// </summary>
        public double Saturation
        {
            get => saturation;
            set => saturation = Math.Max(0, Math.Min(value, 1));
        }

        private double brightness;
        /// <summary>
        /// 明度 0~1 (Value)
        /// </summary>
        public double Brightness
        {
            get => brightness;
            set => brightness = Math.Max(0, Math.Min(value, 1));
        }

        public static HsbColor FromHSB(double hue, double saturation, double brightness) =>
            new HsbColor() { Hue = hue, Saturation = saturation, Brightness = brightness };

        public override string ToString() => string.Format($"H:{Hue:F0}, S:{Saturation:F2}, B:{Brightness:F2}");
    }

    public static class HsbColorEx
    {
        public static double NormalizedRed(this Color color) => Normalize(color.R);
        public static double NormalizedGreen(this Color color) => Normalize(color.G);
        public static double NormalizedBlue(this Color color) => Normalize(color.B);
        private static double Normalize(byte value) => value / 255d;

        private static byte DeNormalize(double value) =>
            value > 1 ? (byte)0xFF
            : value < 0 ? (byte)0x00
            : (byte)(value * 255d);

        public static Color NormalizedRgbToColor(double nR, double nG, double nB) =>
            Color.FromRgb(r: DeNormalize(nR), g: DeNormalize(nG), b: DeNormalize(nB));

        public static HsbColor ToHsb(this Color source)
        {
            double nR = source.NormalizedRed();
            double nG = source.NormalizedGreen();
            double nB = source.NormalizedBlue();

            double[] nRGBs = new[] { nR, nG, nB };
            double max = nRGBs.Max();
            double min = nRGBs.Min();

            double diff = max - min;

            return new HsbColor()
            {
                Hue = max == min ? 0
                    : max == nR ? 60d * (nG - nB) / diff
                    : max == nG ? (60d * (nB - nR) / diff) + 120d
                    : (60d * (nR - nG) / diff) + 240d,
                Saturation = max == 0
                    ? 0
                    : diff / max,
                Brightness = max,
            };
        }

        public static Color ToRgb(this HsbColor source)
        {
            double max = source.Brightness;
            double min = max * (1 - source.Saturation);
            int hueZone = (int)(source.Hue / 60d);
            double f = source.Hue / 60d - hueZone;
            double x0 = max * (1 - f * source.Saturation);
            double x1 = max * (1 - (1 - f) * source.Saturation);

            switch (hueZone)
            {
                case 0:
                    return NormalizedRgbToColor(max, x1, min);
                case 1:
                    return NormalizedRgbToColor(x0, max, min);
                case 2:
                    return NormalizedRgbToColor(min, max, x1);
                case 3:
                    return NormalizedRgbToColor(min, x0, max);
                case 4:
                    return NormalizedRgbToColor(x1, min, max);
                default:
                    return NormalizedRgbToColor(max, min, x0);
            }
        }
    }
}
