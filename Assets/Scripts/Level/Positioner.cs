using System.Collections;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Rigidbody2D))]
public partial class Positioner : MonoBehaviour, Interactable {
    [SerializeField]
    private Vector2 destination;
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

    public Vector2 GetPosition() {
        // This makes it so direct interaction is impossible. Theoretically,
        // this method should never even get called, since this object doesn't
        // have a collider.
        return Vector2.positiveInfinity;
    }

    private void MoveRaw(int direction) {
        movedThisFrame = true;
        Vector2 origin = (Vector2)transform.position;

        var deltaPosition = direction * speed / ((origin - destination).magnitude) * Time.deltaTime;
        position = Mathf.Clamp(position + deltaPosition, 0, 1);
        var newTarget = Vector2.Lerp(origin, destination, position);

        // TODO: Can pass in the spring velocity
        var accel = PhysicsHelper.GetSpringForce(controled.position, newTarget, controled.velocity, Vector2.zero, springConstant, dampingConstant);
        accel = Vector2.ClampMagnitude(accel, maxAccel);

        controled.AddForce(accel*controled.mass);
    }
}
