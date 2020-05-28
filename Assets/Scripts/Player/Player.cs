using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    Rigidbody2D rb;
    [SerializeField]
    private float airAccel;
    [SerializeField]
    private float airFriction;
    [SerializeField]
    private float groundAccel;
    [SerializeField]
    private float groundFriction;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private Collider2D groundDetector;
    [SerializeField]
    private Collider2D interactableDetector;
    [SerializeField]
    private Collider2D mainCollider;
    [SerializeField]
    private float jumpSpeed;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {
        int lr = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        bool onGround = IsOnGround();
        var inputForce = (onGround ? groundAccel : airAccel) * Vector3.right * lr;

        if (Input.GetKey(KeyCode.Space) && onGround) {
            rb.AddForce(PhysicsHelper.GetNeededForce(rb, Vector2.up*jumpSpeed));
        }

        if (inputForce != Vector3.zero) {
            inputForce = PhysicsHelper.TruncateForce(rb, maxSpeed, inputForce);
            rb.AddForce(inputForce);
        }
        if (inputForce.x * rb.velocity.x <= 0) {
            var frictionForce = -(onGround ? groundFriction : airFriction) * Vector3.right * rb.mass * rb.velocity.x;
            rb.AddForce(frictionForce);
        }

        float interaction = (Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0);
        if (interaction != 0) {
            foreach (var lever in GetInteractable()) {
                lever.MovePosition(interaction * .01f);
            }
        }
    }

    IEnumerable<AngleLever> GetInteractable() {
        var colliders = new List<Collider2D>();
        interactableDetector.GetContacts(colliders);
    
        foreach (var col in colliders) {
            if (col.TryGetComponent(out AngleLever lever)) {
                yield return lever;
            }
        }
    }

    bool IsOnGround() {
        var colliders = new List<Collider2D>();
        groundDetector.GetContacts(colliders);
        foreach (var col in colliders) {
            if (col != mainCollider && col != interactableDetector) {
                return true;
            }
        }
        return false;
    }
}
