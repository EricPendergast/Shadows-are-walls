using UnityEngine;

public class PositionTracker : MonoBehaviour {
    public Vector2 position;
    void Awake() {
        position = transform.position;
    }
    void FixedUpdate() {
        position = transform.position;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        position = transform.position;
    }
}
