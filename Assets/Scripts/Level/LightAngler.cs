using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public partial class LightAngler : LevelObject, Interactable {
    [SerializeField]
    private bool DEBUG = false;

    [SerializeField]
    private RotatableLight.RotationConstraints constraints =
        new RotatableLight.RotationConstraints {
            unconstrained = true,
            lower= 0,
            upper = 0
        };
    new private Rigidbody2D body {
        get => base.body == null ? GetComponent<Rigidbody2D>() : base.body;
    }
    [SerializeField]
    private float acceleration = 20;

    [SerializeField]
    private bool rotatedThisFrame = false;

    [SerializeField]
    public RotatableLight controled;

    public void Interact(Vector2 direction) {
        var lightDirection = controled.transform.right;

        if (direction == Vector2.zero || Vector2.Angle(direction, lightDirection) < 1) {
            return;
        }
        var angle = Vector2.SignedAngle(lightDirection, direction);
        if (Vector2.SignedAngle(lightDirection, direction) < 0) {
            Rotate(-1);
        } else {
            Rotate(1);
        }
    }

    public Vector2 GetPosition() {
        return transform.position;
    }

    private void Rotate(int direction) {
        rotatedThisFrame = true;
        controled.ApplyAngularAcceleration(direction * acceleration);
        
        controled.SetRotationConstraints(constraints);
    }

    private void FixedUpdate() {
        IEnumerator Do() {
            // This waits until all other calls to FixedUpdate to finish
            yield return new WaitForFixedUpdate();
            // TODO: Can make this Update/FixedUpdate independent if we save all
            // calls to Interact, and then only execute Rotate here, based on the
            // calls during hte previous frame.
            if (!rotatedThisFrame) {
                Rotate(0);
            }
            rotatedThisFrame = false;
        };
        StartCoroutine(Do());
    }
}
