using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    Rigidbody2D rb;
    [SerializeField]
    private float accel;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private Detector groundDetector;
    [SerializeField]
    private Collider2D mainCollider;
    [SerializeField]
    private float jumpSpeed;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        Physics2D.IgnoreCollision(mainCollider, groundDetector.GetComponent<Collider2D>());
    }

    void FixedUpdate() {
        var force = Vector3.right*accel*Input.GetAxis("Horizontal");

        if (force != Vector3.zero) {
            force = PhysicsHelper.TruncateForce(rb, maxSpeed, force);
        }

        rb.AddForce(force);

        if (Input.GetKey(KeyCode.Space)) {
            if (groundDetector.Overlaps()) {
                rb.AddForce(PhysicsHelper.GetNeededForce(rb, Vector2.up*jumpSpeed));
            }
        }
    }
}
