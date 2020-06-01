using UnityEngine;
using System.Collections.Generic;

public class AllTracker<T> : MonoBehaviour where T : AllTracker<T> {
    private static HashSet<T> all = new HashSet<T>();

    public static IEnumerable<T> GetAll() {
        return all;
    }

    public static int GetAllCount() {
        return all.Count;
    }

    protected virtual void Awake() {
        all.Add((T)this);
    }

    protected virtual void OnDestroy() {
        all.Remove((T)this);
    }
}
