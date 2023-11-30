using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using LibRedminePower.Extentions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls.ChartView;

namespace RedmineTimePuncher.Models.Visualize.Factors
{
    public class FactorType
    {
        public FactorValueType ValueType { get; set; }
        public string Name { get; set; }

        public FactorType() { }

        public FactorType(FactorValueType type)
        {
            ValueType = type;
            Name = type.GetDescription();
        }

        /// <summary>
        /// ValueType が IssueCustomField の時のみ使用。
        /// </summary>
        public MyCustomField CustomField { get; set; }
        public FactorType(CustomField cf, ResultModel model) : base()
        {
            ValueType = FactorValueType.IssueCustomField;
            Name = cf.Name;

            if (cf.IsListFormat())
                CustomField = new MyCustomField(cf);
            else if (cf.IsUserFormat())
                CustomField = new MyCustomField(cf, model.Users);
            else if (cf.IsVersionFormat())
                CustomField = new MyCustomField(cf, model.Projects);
            else
                throw new NotSupportedException($"FieldFormat={cf.FieldFormat} は対応していません。");
        }


        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            var type = obj as FactorType;
            if (type == null)
                return false;

            if (ValueType != type.ValueType)
                return false;

            if (ValueType == FactorValueType.IssueCustomField)
                // カスタムフィールドの場合、Name が一意に決まらないため、名前も含めて判定する
                return Name == type.Name;
            else
                return true;
        }

        public override int GetHashCode()
        {
            int hashCode = -1979447941;
            hashCode = hashCode * -1521134295 + ValueType.GetHashCode();
            if (ValueType == FactorValueType.IssueCustomField)
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }

    public static class FactorTypeEx
    {
        private static Dictionary<(FactorType, string), Brush> colorDic = new Dictionary<(FactorType, string), Brush>();

        private static List<Brush> issueColors = getColors(ChartPalettes.Flower);
        private static List<Brush> userColors = getColors(ChartPalettes.Spring);
        private static List<Brush> defaultColors = getColors(ChartPalettes.Office2019);

        private static List<Brush> getColors(params ChartPalette[] palettes)
        {
            return palettes.SelectMany(p => p.GlobalEntries).Select(a => a.Fill).ToList();
        }

        private static int index;
        public static Brush GetColor(this FactorType type, string title, int? id = null)
        {
            if (colorDic.TryGetValue((type, title), out var color))
            {
                return color;
            }

            switch (type.ValueType)
            {
                case FactorValueType.Issue:
                    colorDic[(type, title)] = issueColors.GetUniqueColorById(id.Value);
                    return colorDic[(type, title)];
                case FactorValueType.User:
                    colorDic[(type, title)] = userColors.GetUniqueColorById(id.Value);
                    return colorDic[(type, title)];
                case FactorValueType.Project:
                    colorDic[(type, title)] = MyProject.COLORS.GetUniqueColorById(id.Value);
                    return colorDic[(type, title)];
                default:
                    colorDic[(type, title)] = defaultColors[index % defaultColors.Count];
                    index++;
                    return colorDic[(type, title)];
            }
        }
    }
}
