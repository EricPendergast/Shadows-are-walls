using UnityEngine;
using System.Collections.Generic;

public abstract class AllTracker<T> : MonoBehaviour where T : AllTracker<T> {
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

    private bool destroyedDirectly = false;

    public void Destroy() {
        all.Remove((T)this);
        destroyedDirectly = true;
        Destroy(this);
    }

    // Prevents the assert failing when the game ends
    void OnApplicationQuit() {
        all.Remove((T)this);
        destroyedDirectly = true;
    }

    protected virtual void OnDestroy() {
        if (!destroyedDirectly) {
            all.Remove((T)this);
        }
        Debug.Assert(destroyedDirectly, "Error: Object was not destroyed directly");
    }
}
