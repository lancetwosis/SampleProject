using FastEnumUtility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class EnumExtensions
    {
        public static string GetDescription<T>(this T enumValue) where T : struct
        {
            var cusAttrs = typeof(T).GetCustomAttributes(true);
            if (cusAttrs.Any())
            {
                var typeConvAttr = cusAttrs[0] as TypeConverterAttribute;
                var convType = Type.GetType(typeConvAttr.ConverterTypeName);
                var args = new[] { typeof(T) };
                var converter = (TypeConverter)Activator.CreateInstance(convType, args);

                return (string)converter.ConvertTo(enumValue, typeof(string));
            }
            else
            {
                string description = null;
                var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
                var attr = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
                if(attr != null)
                {
                    var descAttr = (DescriptionAttribute)attr;
                    description = descAttr.Description;
                }
                return description;
            }
        }

        public static T ToEnum<T>(this string str) where T : struct, Enum
        {
            return FastEnum.Parse<T>(str);
        }

        public static T ToEnum<T>(this int value) where T : struct, Enum
        {
            var values = FastEnum.GetValues<T>();
            return values.First(a => a.ToInt32() == value);
        }
    }
}
