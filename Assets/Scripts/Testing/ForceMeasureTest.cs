using System.Collections.Generic;
using UnityEngine;

public class ForceMeasureTest : MonoBehaviour {
    public Vector2 totalImpulse = Vector2.zero;
    public Vector2 impulseThisFrame = Vector2.zero;
    public Vector2 forceThisFrame = Vector2.zero;
    public Rigidbody2D relayTo;
    public float relayScale = 1;

    //public Dictionary<Rigidbody2D, int> multiFrameContacts = new Dictionary<Rigidbody2D, int>();
    public HashSet<Rigidbody2D> contactsLastFrame = new HashSet<Rigidbody2D>();
    public HashSet<Rigidbody2D> contactsThisFrame = new HashSet<Rigidbody2D>();

    //void OnCollisionEnter2D(Collision2D collision) {
    //    RegisterCollision(collision);
    //}
    void OnCollisionStay2D(Collision2D collision) {
        RegisterCollision(collision);
    }

    //void OnCollisionExit2D(Collision2D collision) {
    //    RegisterCollision(collision);
    //}

    bool isValid(float num) {
        return !float.IsNaN(num) && !float.IsPositiveInfinity(num) && !float.IsNegativeInfinity(num) && Mathf.Abs(num) < 100000;
    }

    void RegisterCollision(Collision2D collision) {
        foreach (var contact in collision.contacts) {
            contactsThisFrame.Add(contact.rigidbody);

            if (contactsLastFrame.Contains(contact.rigidbody)) {

                var impulse = (isValid(contact.normalImpulse) ? contact.normal*contact.normalImpulse : Vector2.zero) +
                              (isValid(contact.tangentImpulse) ? Math.Rotate(contact.normal, -90)*contact.tangentImpulse : Vector2.zero);

                impulseThisFrame += impulse;
                forceThisFrame += impulse/Time.fixedDeltaTime;
                totalImpulse += impulse;
            }
        }
    }

    void FixedUpdate() {
        impulseThisFrame = Vector2.zero;
        forceThisFrame = Vector2.zero;
        {
            var tmp = contactsLastFrame;
            contactsLastFrame = contactsThisFrame;
            contactsThisFrame = tmp;
        }
    }
}
