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

    [System.Serializable]
    public class StateMachine {
        [SerializeField]
        private State current = State.initial;

        public void TransitionState(Player player) {
            // Interaction state changes have precendence over other state
            // changes, so they are checked before everything else.
            if (current == State.interacting) {
                if (player.InteractKeyPressed() && !player.TooFarFromInteractable()) {
                    return;
                }
                player.EndInteract();
                current = State.initial;
            } else {
                if (player.InteractKeyPressed()) {
                    var startedInteracting = player.TryBeginInteract();
                    if (startedInteracting) {
                        current = State.interacting;
                        return;
                    }
                }
            }

            if (current == State.onGround) {
                if (player.JumpKeyPressed()) {
                    current = State.jumping;
                    player.OnFirstFrameOfJump();
                    return;
                }
            } else if (current == State.jumping) {
                if (player.JumpTimeUp()) {
                    if (player.JumpKeyPressed()) {
                        current = State.initial;
                        player.OnLastFrameOfJump();
                    }
                } else {
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
        private Collider2D mainCollider;
        [SerializeField]
        private float initialJumpSpeed;
        [SerializeField]
        private float secondaryJumpSpeed;
        [SerializeField]
        private float jumpTime;
        [SerializeField]
        private float forceTransferRatio;

        private float timeOfLastJump;

        [SerializeField]
        private StateMachine sm;
        [SerializeField]
        private InteractionHandler interactionHandler;

        private void Start() {
            rb = GetComponent<Rigidbody2D>();
            Refs.instance.cameraTracker.trackedObject = gameObject;
            timeOfLastJump = -1000;
            sm = new StateMachine();
        }

        private void FixedUpdate() {
            sm.TransitionState(this);

            if (sm.Current() == State.interacting) {
                interactionHandler.ApplyForce(rb);
                interactionHandler.Interact(GetDirectionalInput());
            } else {
                var playerForce = GetHorizontalInputForce();
                rb.AddForce(playerForce);

                ApplyForceToStandingOn(-playerForce*forceTransferRatio);

                rb.AddForce(GetFrictionForce(playerForce));
            }
        }

        private Vector2 GetDirectionalInput() {
            return Vector2.up * (Input.GetKey(KeyCode.W) ? 1 : 0) +
                   Vector2.left * (Input.GetKey(KeyCode.A) ? 1 : 0) +
                   Vector2.down * (Input.GetKey(KeyCode.S) ? 1 : 0) +
                   Vector2.right * (Input.GetKey(KeyCode.D) ? 1 : 0);
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

            colliders.RemoveAll(col => col == mainCollider || col.isTrigger);

            return colliders;
        }

        //public void Jump() {
        //    rb.AddForce(PhysicsHelper.GetNeededForce(rb, new Vector2(0,rb.velocity.y), Vector2.up*jumpSpeed));
        //}

        private Vector2 GetFrictionForce(Vector2 forceAppliedToPlayer) {
            if (forceAppliedToPlayer.x * rb.velocity.x <= 0) {
                return -(sm.Current() == State.onGround ? groundFriction : airFriction) * Vector3.right * rb.mass * rb.velocity.x;
            }
            return Vector2.zero;

        }

        public bool InteractKeyPressed() {
            return Input.GetKey(KeyCode.LeftShift);
        }

        public bool TooFarFromInteractable() {
            return interactionHandler.TooFarFromInteractable();
        }

        public bool TryBeginInteract() {
            return interactionHandler.TryBeginInteract();
        }

        public void EndInteract() {
            interactionHandler.EndInteract();
        }

        public bool IsOnGround() {
            return GetStandingOn().Count > 0;
        }

        public bool JumpKeyPressed() {
            return Input.GetKey(KeyCode.Space);
        }

        public void OnFirstFrameOfJump() {
            rb.velocity = new Vector2(rb.velocity.x, initialJumpSpeed);
            timeOfLastJump = Time.time;
        }

        public void OnLastFrameOfJump() {
            rb.velocity = new Vector2(rb.velocity.x, secondaryJumpSpeed);
        }

        public bool JumpTimeUp() {
            return Time.time - timeOfLastJump > jumpTime;
        }
    }


    [System.Serializable]
    public class InteractionHandler {
        private Interactable interactingObject = null;
        [SerializeField]
        private float interactionBreakDistance;
        [SerializeField]
        private Collider2D interactableDetector;
        [SerializeField]
        private float springConstant;
        [SerializeField]
        private float springDampingConstant;

        private Vector2 GetPosition() {
            return interactableDetector.transform.position;
        }

        private IEnumerable<Interactable> GetInteractables() {
            var colliders = new List<Collider2D>();
            interactableDetector.GetContacts(colliders);

            foreach (var col in colliders) {
                if (col.TryGetComponent(out Interactable interactable)) {
                    yield return interactable;
                }
            }
        }

        private Interactable GetClosestInteractable() {
            Interactable closest = null;
            float minDistance = Mathf.Infinity;

            foreach (Interactable interactable in GetInteractables()) {
                float distance = (interactable.GetPosition() - this.GetPosition()).sqrMagnitude;
                if (distance < minDistance) {
                    closest = interactable;
                    minDistance = distance;
                }
            }

            return closest;
        }

        public bool TooFarFromInteractable() {
            if (interactingObject == null) {
                return true;
            }

            // TODO: Maybe this should use the collider of the interacting
            // object. Using just its position may be more problematic as its
            // collider gets larger.
            Vector2 difference = interactingObject.GetPosition() - interactableDetector.ClosestPoint(interactingObject.GetPosition());

            return difference.magnitude > interactionBreakDistance;
        }

        public bool TryBeginInteract() {
            Debug.Assert(interactingObject == null, "ERROR: Tried to begin interacting while already interacting!");
            interactingObject = GetClosestInteractable();

            return interactingObject != null;
        }

        public void EndInteract() {
            interactingObject = null;
        }

        public void ApplyForce(Rigidbody2D rb) {
            if (interactingObject == null) {
                return;
            }

            Vector2 difference = interactingObject.GetPosition() - rb.position;

            rb.AddForce(difference*springConstant);
            rb.velocity -= springDampingConstant*rb.velocity*Time.deltaTime;
        }

        public void Interact(Vector2 direction) {
            if (interactingObject != null) {
                interactingObject.Interact(direction);
            }
        }
    }
}
