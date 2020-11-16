using System.Collections.Generic;

 namespace ListExtensions {

    public static class Extensions {
        public static T Min<T>(this List<T> list) where T : System.IComparable<T> {
            T min = list[0];

            for (int i = 1; i < list.Count; i++) {
                if (min.CompareTo(list[i]) > 0) {
                    min = list[i];
                }
            }

            return min;
        }

        public static T Max<T>(this List<T> list) where T : System.IComparable<T> {
            T max = list[0];

            for (int i = 1; i < list.Count; i++) {
                if (max.CompareTo(list[i]) < 0) {
                    max = list[i];
                }
            }

            return max;
        }

        public static void InsertSorted<T>(this List<T> list, T item) where T : System.IComparable<T> {
            var i = list.BinarySearch(item);
            if (i < 0) {
                i = ~i;
            }
            list.Insert(i, item);
        }

        //public static void RemoveAll
            
    }
}
