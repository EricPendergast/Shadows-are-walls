using UnityEngine;
using System.Collections.Generic;

public class Detector : MonoBehaviour {
    [SerializeField]
    private HashSet<Collider2D> colliders = new HashSet<Collider2D>();
    public Collider2D coll;

    void OnTriggerEnter2D(Collider2D collider) {
        colliders.Add(collider);
        coll = collider;
    }

    void OnTriggerExit2D(Collider2D collider) {
        colliders.Remove(collider);
        coll = null;
    }

    // TODO: Name this better
    public bool Overlaps() {
        return colliders.Count > 0;
    }
}
