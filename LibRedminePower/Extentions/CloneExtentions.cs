using System.Text.Encodings.Web;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Drawing;
using System.Text.Unicode;

namespace LibRedminePower.Extentions
{
    public static class CloneExtentions
    {
        private static JsonSerializerOptions createJsonSerializerOptions(bool writeIndented)
        {
            return new JsonSerializerOptions()
            {
                WriteIndented = writeIndented,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                Converters = { new ColorJsonConverter() },
            };
        }

        public static T Clone<T>(this T target) where T : class, new()
        {
            var str = ToJson(target);
            return ToObject<T>(str);
        }

        public static string ToJson<T>(this T target, bool writeIndented = true) where T : class
            => JsonSerializer.Serialize(target, target.GetType(), createJsonSerializerOptions(writeIndented));

        /// <summary>
        /// デシリアライズ
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <remarks>
        /// ・引数無しコンストラクタがあれば、そちらが優先される。
        /// ・他を呼びたい場合は、[JsonConstructor] 属性で指定可能。
        /// ・private setプロパティは、基本は復元されない。
        /// ・ただし、引数ありコンストラクタは、プロパティと引数の型/名前がマッチすれば、その中で展開される。
        /// ・また、[JsonInclude]属性指定でも、展開可能。
        /// 
        /// 結構、奥が深いので一読しておくと良い。
        /// https://devadjust.exblog.jp/28571843/
        /// private set, protected set
        /// </remarks>
        /// <returns></returns>
        public static T ToObject<T>(string target) where T : class, new ()
            => JsonSerializer.Deserialize<T>(target, createJsonSerializerOptions(true));

        public class ColorJsonConverter : JsonConverter<Color>
        {
            public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => ColorTranslator.FromHtml(reader.GetString());

            public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) => writer.WriteStringValue("#" + value.R.ToString("X2") + value.G.ToString("X2") + value.B.ToString("X2").ToLower());
        }
    }
}
