using UnityEngine;
using System.Collections.Generic;

public class Detector : MonoBehaviour {
    [SerializeField]
    private List<Collider2D> colliders = new List<Collider2D>();

    void OnTriggerEnter2D(Collider2D collider) {
        colliders.Add(collider);
    }

    void OnTriggerExit2D(Collider2D collider) {
        colliders.Remove(collider);
    }

    // TODO: Name this better
    public bool Overlaps() {
        return colliders.Count > 0;
    }
}
