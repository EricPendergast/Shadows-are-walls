using System.Collections;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Rigidbody2D))]
public partial class Positioner : MonoBehaviour, Interactable {
    // This is local to the parent of this gameobject
    [SerializeField]
    private Vector2 localDestination;

    // This is in global coordinates
    private Vector2 destination {
        get => localDestination + ParentPosition();
        set => localDestination = value - ParentPosition();
    }
    // Represents where the object will be positioned. This is an interpolation
    // between transform.position and this.destination
    [SerializeField]
    private float position = 0;
    [SerializeField]
    private float speed = 3f;
    [SerializeField]
    private Rigidbody2D controled;
    [SerializeField]
    private float rotation;
    [SerializeField]
    private float maxAccel;
    [SerializeField]
    private float springConstant;
    [SerializeField]
    private float dampingConstant;
    [SerializeField]
    private float correctionConstant;
    [SerializeField]
    private float correctionMinDistance;
    [SerializeField]
    private float stationaryMass;
    [SerializeField]
    private float movingMass;
    [SerializeField]
    private bool movedThisFrame = false;

    void Start() {
        if (Application.isPlaying) {
            Debug.Assert(controled != null);
            MoveRaw(0);
        }
    }

    void FixedUpdate() {
        IEnumerator Do() {
            // This waits until all other calls to FixedUpdate to finish
            yield return new WaitForFixedUpdate();
            if (!movedThisFrame) {
                MoveRaw(0);
            }
            movedThisFrame = false;
        };
        StartCoroutine(Do());
    }

    public void Interact(Vector2 direction) {
        if (direction == Vector2.zero) {
            return;
        } 

        Vector2 origin = (Vector2)transform.position;

        if (Vector2.Dot(direction, destination - origin) > 0) {
            MoveRaw(1);
        } else {
            MoveRaw(-1);
        }
    }

    // This is for the Interactable interface.
    public Vector2 GetPosition() {
        // This makes it so direct interaction is impossible. Theoretically,
        // this method should never even get called, since this object doesn't
        // have a collider.
        return Vector2.positiveInfinity;
    }

    private float GetActualPosition() {
        Vector2 origin = (Vector2)transform.position;

        return Vector3.Project(controled.position - origin, destination - origin).magnitude /
               (origin - destination).magnitude;
    }

    private void MoveRaw(int direction) {
        controled.mass = direction == 0 ? stationaryMass : movingMass;
        movedThisFrame = true;
        Vector2 origin = (Vector2)transform.position;

        var deltaPosition = direction * speed / ((origin - destination).magnitude) * Time.deltaTime;

        var oldTarget = Vector2.Lerp(origin, destination, position);

        if ((oldTarget - controled.position).magnitude > correctionMinDistance) {
            position = Mathf.Lerp(position, GetActualPosition(), correctionConstant);
        }

        position = Mathf.Clamp(position + deltaPosition, 0, 1);

        var newTarget = Vector2.Lerp(origin, destination, position);

        var targetVelocity = (destination - origin).normalized*direction*speed;
        if (position == 1 || position == 0) {
            targetVelocity = Vector2.zero;
        }
        var accel = PhysicsHelper.GetSpringForce(controled.position, newTarget, controled.velocity, targetVelocity, springConstant, dampingConstant);
        accel = Vector2.ClampMagnitude(accel, maxAccel);

        controled.AddForce(accel*controled.mass);
    }

    private Vector2 ParentPosition() {
        return transform.parent == null ? Vector2.zero : (Vector2)transform.parent.position;
    }
}
