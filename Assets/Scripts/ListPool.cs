using System.Collections.Generic;

// This is intended to be used only with 'using' statements. If a ListPoolEntry
// is returned from a method, bad things will happen.
public class Pool<T> where T : Poolable, new() {
    private Stack<T> pool = new Stack<T>();
    private HashSet<T> poolSet = new HashSet<T>();

    public readonly struct ListPoolEntry : System.IDisposable {
        private readonly T entry;
        private readonly Pool<T> owner;

        public T val {
            get => entry;
        }

        public ListPoolEntry(T list, Pool<T> owner) {
            this.entry = list;
            this.owner = owner;
        }

        public void Dispose() {
            if (entry != null) {
                entry.Clear();
                if (!owner.poolSet.Contains(entry)) {
                    owner.poolSet.Add(entry);
                    owner.pool.Push(entry);
                }
            }
        }
    }

    public ListPoolEntry TakeTemporary() {
        if (pool.Count == 0) {
            return new ListPoolEntry(new T(), this);
        } else {
            var e = new ListPoolEntry(pool.Pop(), this);
            poolSet.Remove(e.val);
            return e;
        }
    }
}


public interface Poolable {
    void Clear();
}

public class PoolableList<T> : List<T>, Poolable {
    new void Clear() {
        base.Clear();
    }
}

public class PoolableQueue<T> : Queue<T>, Poolable {
    new void Clear() {
        base.Clear();
    }
}

public class ListPool<T> : Pool<PoolableList<T>> {}
public class QueuePool<T> : Pool<PoolableQueue<T>> {}
