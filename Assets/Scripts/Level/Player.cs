using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public enum State {
        initial,
        inAir,
        onGround,
        jumping
    }
    [SerializeField]
    private State current;
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
    [SerializeField]
    private float jumpTime;

    private float timeOfLastJump;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        Refs.instance.cameraTracker.trackedObject = gameObject;
        timeOfLastJump = -1000;
        current = State.initial;
    }

    void FixedUpdate() {
        DoStateMachine();

        int lr = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        var inputForce = (current == State.onGround ? groundAccel : airAccel) * Vector3.right * lr;

        if (inputForce != Vector3.zero) {
            inputForce = PhysicsHelper.TruncateForce(rb, maxSpeed, inputForce);
            rb.AddForce(inputForce);
            var standingOn = GetStandingOn();
            foreach (var col in standingOn) {
                col.attachedRigidbody.AddForce(-inputForce/standingOn.Count);
            }
        }

        if (current == State.jumping) {
            rb.AddForce(PhysicsHelper.GetNeededForce(rb, new Vector2(0,rb.velocity.y), Vector2.up*jumpSpeed));
        }

        if (inputForce.x * rb.velocity.x <= 0) {
            var frictionForce = -(current == State.onGround ? groundFriction : airFriction) * Vector3.right * rb.mass * rb.velocity.x;
            rb.AddForce(frictionForce);
        }

        int interaction = (Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0);
        if (interaction != 0) {
            foreach (var lever in GetInteractable()) {
                lever.MovePosition(interaction);
            }
        }
    }

    IEnumerable<Lever> GetInteractable() {
        var colliders = new List<Collider2D>();
        interactableDetector.GetContacts(colliders);
    
        foreach (var col in colliders) {
            if (col.TryGetComponent(out Lever lever)) {
                yield return lever;
            }
        }
    }

    void DoStateMachine() {
        if (current == State.onGround) {
            if (Input.GetKey(KeyCode.Space)) {
                current = State.jumping;
                timeOfLastJump = Time.time;
                return;
            }
        } else if (current == State.jumping) {
            // If the player is still holding space, and it is within the
            // specified time limit, stay jumping. This allows the player to
            // alter the height of their jump
            if (Time.time - timeOfLastJump < jumpTime && Input.GetKey(KeyCode.Space)) {
                return;
            }
        }

        current = IsOnGround() ? State.onGround : State.inAir;
    }

    List<Collider2D> GetStandingOn() {
        var colliders = new List<Collider2D>();
        groundDetector.GetContacts(colliders);

        colliders.RemoveAll(col => col == mainCollider || col == interactableDetector || col.isTrigger);

        return colliders;
    }

    bool IsOnGround() {
        return GetStandingOn().Count > 0;
    }
}
