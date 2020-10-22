using System.Collections.Generic;
using UnityEngine;

public class ForceMeasurer : MonoBehaviour {
    [SerializeField] 
    private float lastUpdate = float.NegativeInfinity;

    [System.Serializable]
    private class FrameData {
        [SerializeField]
        public Vector2 sumForce = Vector2.zero;
        public float sumTorque = 0;
        public HashSet<Rigidbody2D> contacts = new HashSet<Rigidbody2D>();

        public void Reset() {
            sumForce = Vector2.zero;
            sumTorque = 0;
            contacts.Clear();
        }
    }

    [SerializeField]
    private FrameData thisFrame = new FrameData();
    [SerializeField]
    private FrameData lastFrame = new FrameData();

    public float GetTorqueLastFrame() {
        UpdateFrameData();
        return lastFrame.sumTorque;
    }

    public Vector2 GetForceLastFrame() {
        UpdateFrameData();
        return lastFrame.sumForce;
    }

    //void OnCollisionEnter2D(Collision2D collision) {
    //    RegisterCollision(collision);
    //}
    void OnCollisionStay2D(Collision2D collision) {
        RegisterCollision(collision);
    }

    //void OnCollisionExit2D(Collision2D collision) {
    //    RegisterCollision(collision);
    //}

    bool IsValid(float num) {
        return !float.IsNaN(num) && !float.IsPositiveInfinity(num) && !float.IsNegativeInfinity(num) && Mathf.Abs(num) < 100000;
    }

    void RegisterCollision(Collision2D collision) {
        foreach (var contact in collision.contacts) {
            thisFrame.contacts.Add(contact.rigidbody);

            if (lastFrame.contacts.Contains(contact.rigidbody)) {

                var impulse = (IsValid(contact.normalImpulse) ? contact.normal*contact.normalImpulse : Vector2.zero) +
                              (IsValid(contact.tangentImpulse) ? Math.Rotate(contact.normal, -90)*contact.tangentImpulse : Vector2.zero);

                var force = impulse/Time.fixedDeltaTime;
                thisFrame.sumForce += force;

                var torque = Vector3.Cross(force, contact.point - contact.otherRigidbody.worldCenterOfMass).z;
                thisFrame.sumTorque += torque;
            }
        }
    }

    void UpdateFrameData() {
        if (Time.fixedTime > lastUpdate) {
            lastUpdate = Time.fixedTime;

            var tmp = lastFrame;
            lastFrame = thisFrame;
            thisFrame = tmp;

            thisFrame.Reset();
        }
    }

    void FixedUpdate() {
        UpdateFrameData();
    }
}
