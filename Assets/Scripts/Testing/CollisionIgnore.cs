using UnityEngine;

public class CollisionIgnore : MonoBehaviour {

    void FixedUpdate() {

    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.GetComponent<PositionTracker>()) {
            Vector2 counterImpulse = Vector2.zero;
            foreach (ContactPoint2D point in collision.contacts) {
                counterImpulse -= point.normal*point.normalImpulse;
            }
            collision.rigidbody.AddForce(counterImpulse, ForceMode2D.Impulse);
            Debug.Log("Collision");
        }
    }
}
