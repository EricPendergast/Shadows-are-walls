using System.Collections.Generic;
using UnityEngine;

namespace Player {
    public enum State {
        initial,
        inAir,
        onGround,
        jumping,
        interacting
    }

    public class StateMachine {

        private State current = State.initial;

        public void TransitionState(Player player) {
            if (current == State.onGround) {
                if (player.JumpKeyPressed()) {
                    current = State.jumping;
                    player.OnFirstFrameOfJump();
                    return;
                }
            } else if (current == State.jumping) {
                if (player.CanStillJump()) {
                    return;
                }
            }
            current = player.IsOnGround() ? State.onGround : State.inAir;
        }

        public State Current() {
            return current;
        }
    }

    public class Player : MonoBehaviour {

        private Rigidbody2D rb;
        [SerializeField]
        private StateMachine sm;
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
        [SerializeField]
        private float forceTransferRatio;

        private float timeOfLastJump;

        private void Start() {
            rb = GetComponent<Rigidbody2D>();
            Refs.instance.cameraTracker.trackedObject = gameObject;
            timeOfLastJump = -1000;
            sm = new StateMachine();
        }

        private void FixedUpdate() {
            sm.TransitionState(this);

            var playerForce = GetHorizontalInputForce();
            rb.AddForce(playerForce);

            ApplyForceToStandingOn(-playerForce*forceTransferRatio);

            if (sm.Current() == State.jumping) {
                Jump();
            }

            rb.AddForce(GetFrictionForce(playerForce));

            int interaction = (Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0);
            if (interaction != 0) {
                foreach (var lever in GetInteractable()) {
                    lever.MovePosition(interaction);
                }
            }
        }

        private Vector2 GetHorizontalInputForce() {
            int lr = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
            Vector2 inputForce = (sm.Current() == State.onGround ? groundAccel : airAccel) * Vector3.right * lr;
            return PhysicsHelper.TruncateForce(rb, maxSpeed, inputForce);
        }

        private void ApplyForceToStandingOn(Vector2 force) {
            if (force!= Vector2.zero) {
                var standingOn = GetStandingOn();
                foreach (var col in standingOn) {
                    col.attachedRigidbody.AddForce(force/standingOn.Count);
                }
            }
        }

        private List<Collider2D> GetStandingOn() {
            var colliders = new List<Collider2D>();
            groundDetector.GetContacts(colliders);

            colliders.RemoveAll(col => col == mainCollider || col == interactableDetector || col.isTrigger);

            return colliders;
        }


        public void Jump() {
            rb.AddForce(PhysicsHelper.GetNeededForce(rb, new Vector2(0,rb.velocity.y), Vector2.up*jumpSpeed));
        }

        private Vector2 GetFrictionForce(Vector2 forceAppliedToPlayer) {
            if (forceAppliedToPlayer.x * rb.velocity.x <= 0) {
                return -(sm.Current() == State.onGround ? groundFriction : airFriction) * Vector3.right * rb.mass * rb.velocity.x;
            }
            return Vector2.zero;

        }

        private IEnumerable<Lever> GetInteractable() {
            var colliders = new List<Collider2D>();
            interactableDetector.GetContacts(colliders);
        
            foreach (var col in colliders) {
                if (col.TryGetComponent(out Lever lever)) {
                    yield return lever;
                }
            }
        }

        public bool IsOnGround() {
            return GetStandingOn().Count > 0;
        }

        public bool JumpKeyPressed() {
            return Input.GetKey(KeyCode.Space);
        }

        public void OnFirstFrameOfJump() {
            timeOfLastJump = Time.time;
        }

        public bool CanStillJump() {
            return Time.time - timeOfLastJump < jumpTime && JumpKeyPressed();
        }
    }
}
