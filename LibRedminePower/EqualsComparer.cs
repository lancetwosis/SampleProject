using System.Collections.Generic;

namespace LibRedminePower
{
    public static class EqualsComparer
    {
        public static bool AreListsEqual<T>(IList<T> list1, IList<T> list2)
        {
            // nullと空リストを同一とみなす
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
    }
}
