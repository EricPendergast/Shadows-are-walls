using UnityEngine;

public class LevelObject : SnappableObject {
    protected Rigidbody2D body;
    new protected Collider2D collider;

    protected virtual void Awake() {
        body = GetComponent<Rigidbody2D>();

        if (body == null) {
            body = gameObject.AddComponent<Rigidbody2D>();
            // TODO: Maybe put some settings here
        }
        if (collider == null) {
            collider = GetComponent<Collider2D>();
            if (collider == null) {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }
        }
    }

    protected virtual void OnDrawGizmosSelected() {
        GizmosUtil.DrawConstantWidthSphere(transform.TransformPoint(snapPoint), .025f);
    }
}
