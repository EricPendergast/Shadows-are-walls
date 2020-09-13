using System.Collections.Generic;

public class Algorithm {
    public struct Bounds {

        public Bounds(int lower, int upper, int sizeOfList) {
            this.lower = lower;
            this.upper = upper;
            this.sizeOfList = sizeOfList;
        }

        private int sizeOfList;
        private int lower;
        private int upper;

        public int Lower() {
            return lower;
        }

        public int Upper() {
            return upper;
        }

        public bool IsFull() {
            return upper == Math.Mod(lower - 1, sizeOfList);
        }

        public bool IsEmpty() {
            return upper == -1 && lower == -1;
        }
    }

    // Finds a contiguous region filled with the value 'true'. Bounds are
    // inclusive. 
    public static Bounds FindContiguousWrapAround(List<bool> l) {
        int? upper = null;
        int? lower = null;

        bool Get(int i) {
            return l[Math.Mod(i, l.Count)];
        }

        for (int i = 0; i < l.Count * 2; i++) {
            if (lower == null) {
                if (!Get(i - 1) && Get(i)) {
                    lower = Math.Mod(i, l.Count);
                }
            }
            if (lower != null && upper == null) {
                if (Get(i) && !Get(i + 1)) {
                    upper = Math.Mod(i, l.Count);
                    break;
                }
            }
        }

        if (lower != null && upper != null) {
            return new Bounds((int)lower, (int)upper, l.Count);
        } else {
            if (l.Count == 0 || !l[0]) {
                return new Bounds(-1, -1, l.Count);
            } else {
                return new Bounds(0, l.Count-1, l.Count);
            }
        }
    }
}
