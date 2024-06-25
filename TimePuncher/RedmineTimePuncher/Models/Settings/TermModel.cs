using LibRedminePower.Extentions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Models.Settings
{
    public class TermModel : LibRedminePower.Models.Bases.ModelBaseSlim
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public System.Drawing.Color Color { get; set; }
        public Enums.TermInputValidationType ValidationType { get; set; }
        public bool OnTime { get; set; }
        public bool IsCoreTime { get; set; }

        public TermModel(TimeSpan start, TimeSpan end)
        {
            Start = start;
            End = end;
            Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
        }

        internal IEnumerable<TermModel> Split(int minLength)
        {
            var startTime = Start;
            var endTime = End;
            while (startTime < endTime)
            {
                yield return new TermModel(startTime, startTime.Add(TimeSpan.FromMinutes(minLength)))
                {
                    Color = this.Color,
                    ValidationType = this.ValidationType ,
                };
                startTime = startTime.Add(TimeSpan.FromMinutes(minLength));
            }
        }

        public bool Contains(DateTime target)
        {
            return (Start <= target.TimeOfDay && target.TimeOfDay < End);
        }


        public override string ToString()
        {
            return $"{Start.ToString(@"hh\:mm")} - {End.ToString(@"hh\:mm")} : {ValidationType.GetDescription()}" ;
        }

        public string GetMessage()
        {
            return ValidationType.GetMessage(Start, End);
        }

        private static Dictionary<System.Drawing.Color, Brush> majorBrushDic = new Dictionary<System.Drawing.Color, Brush>();
        private static Dictionary<System.Drawing.Color, Brush> minorBrushDic = new Dictionary<System.Drawing.Color, Brush>();
        public Brush GetTickBackground(bool isMajor)
        {
            if (isMajor)
            {
                if (!majorBrushDic.ContainsKey(Color))
                    majorBrushDic[Color] = Color.ToBrush();

                return majorBrushDic[Color];
            }
            else
            {
                if (!minorBrushDic.ContainsKey(Color))
                    minorBrushDic[Color] = new LinearGradientBrush(Color.ToMediaColor(), Colors.White, 20);

                return minorBrushDic[Color];
            }
        }
    }

    public static class TermModelEx
    {
        /// <summary>
        /// 隣接した期間を持つ Term（前者の End と 後者の Start が等しいもの）を一つの Term にまとめた Term のリストを返す
        /// </summary>
        public static IEnumerable<TermModel> Merge(this IEnumerable<TermModel> targetTerms)
        {
            var result = new List<TermModel>();
            foreach (var target in targetTerms.GroupBy(a => a.ValidationType))
            {
                var myResult = target.OrderBy(a => a.Start).ToList();
                while (true)
                {
                    // ペアを作り、一つ目の End と二つ目の Start が等しいものを抽出する
                    var temps = myResult.Pairs().Where(a => a.First().End == a.Last().Start);
                    if (!temps.Any())
                    {
                        break;
                    }
                    else
                    {
                        // 条件に当てはまった最初のペアを取り除く
                        myResult.RemoveAll(a => temps.First().Contains(a));
                        // 取り除いたペアの替わりに二つをつなげた Term を追加する
                        myResult.Add(new TermModel(temps.First().First().Start, temps.First().Last().End) { ValidationType = temps.First().First().ValidationType });
                        myResult = myResult.OrderBy(a => a.Start).ToList();
                    }
                }
                result.AddRange(myResult);
            }
            return result;
        }
    }
}
