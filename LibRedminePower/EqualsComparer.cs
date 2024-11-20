using LibRedminePower.Extentions;
using System.Collections.Generic;

namespace LibRedminePower
{
    public static class EqualsComparer
    {
        /// <summary>
        /// nullと空リストを同一とみなして List<T> を比較する
        /// </summary>
        public static bool AreListsEqual<T>(IList<T> list1, IList<T> list2)
        {
            if ((list1 == null || list1.Count == 0) && (list2 == null || list2.Count == 0))
                return true;

            if (list1 == null || list2 == null)
                return false;

            if (list1.Count != list2.Count)
                return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (!list1[i].Equals(list2[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 型の一致と JSON 変換した文字列の一致を判定する
        /// </summary>
        public static bool JsonEquals<T>(this T t1, object obj) where T : class
        {
            return obj is T t2 && t1.ToJson() == t2.ToJson();
        }

        /// <summary>
        /// JSON 変換した文字列のハッシュ値を返す
        /// </summary>
        public static int GetJsonHashcode<T>(this T t) where T : class
        {
            return EqualityComparer<string>.Default.GetHashCode(t.ToJson());
        }
    }
}
