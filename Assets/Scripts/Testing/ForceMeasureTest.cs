using UnityEngine;

public class ForceMeasureTest : MonoBehaviour {
    void OnCollisionEnter2D(Collision2D collision) {
        PrintCollision(collision);
    }
    void OnCollisionStay2D(Collision2D collision) {
        PrintCollision(collision);
    }

    void PrintCollision(Collision2D collision) {
        Debug.Log("Collision");
        //Debug.Log(collision.relativeVelocity);
        foreach (var contact in collision.contacts) {
            //Debug.Log(new Vector2(contact.normalImpulse, contact.tangentImpulse).magnitude);
            Debug.Log(contact.separation);
        }
    }
}
