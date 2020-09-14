using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LightAngler : LevelObject, Interactable {

    [SerializeField]
    private bool DEBUG = false;
    [SerializeField]
    private bool unconstrained = false;
    [SerializeField]
    [Range(0,180)]
    private float angleConstraint = 90;
    [SerializeField]
    private float currentAngle = 0;
    [SerializeField]
    private float apertureAngle = 35;
    new private Rigidbody2D body {
        get => base.body == null ? GetComponent<Rigidbody2D>() : base.body;
    }
    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private float currentVelocity = 0;
    [SerializeField]
    private float acceleration = 20;
    [SerializeField]
    private float decceleration = 20;

    [SerializeField]
    private bool rotatedThisFrame = false;

    [SerializeField]
    public RotatableLight controled;

    private RelativeJoint2D myJoint;

    public void Start() {
        ApplySettings();
        myJoint = gameObject.AddComponent<RelativeJoint2D>();
        myJoint.connectedBody = controled.GetComponent<Rigidbody2D>();
        myJoint.maxForce = 80;
        myJoint.maxTorque = 10;
        myJoint.autoConfigureOffset = false;
        myJoint.angularOffset = currentAngle;

        Rotate(0);
    }

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

        float currentAcceleration;
        if (direction == 0 && currentVelocity == 0) {
            currentAcceleration = 0;
        } else if (direction == 0) {
            currentAcceleration = -decceleration * Mathf.Sign(currentVelocity);
        } else if (currentVelocity == 0) {
            currentAcceleration = acceleration * Mathf.Sign(direction);
        } else {
            currentAcceleration = (direction * currentVelocity >= 0 ? acceleration : decceleration) * Mathf.Sign(direction);
        }

        var newVelocity = currentVelocity + currentAcceleration*Time.deltaTime;

        if (newVelocity * currentVelocity < 0) {
            currentVelocity = 0;
        } else {
            currentVelocity = Mathf.Clamp(newVelocity, -speed, speed);
        }

        currentAngle += currentVelocity*Time.deltaTime;
        currentAngle = ClampToConstraints(currentAngle);

        float maxDiff = 2*speed*Time.deltaTime;
        if (direction == 0) {
            maxDiff /= 2;
        }
      
        var actualAngle = myJoint.connectedBody.rotation - body.rotation;

        currentAngle = Mathf.Clamp(currentAngle, actualAngle - maxDiff, actualAngle + maxDiff);

        myJoint.angularOffset = currentAngle;
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

    private void OnDrawGizmos() {
        DrawGizmos(1);
    }

    protected override void OnDrawGizmosSelected() {
        DrawGizmos(20);
    }

    void DrawGizmos(float gizmoLength) {
        if (unconstrained) {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - angleConstraint)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - (angleConstraint - apertureAngle))*Vector2.right*gizmoLength);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + angleConstraint)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + (angleConstraint - apertureAngle))*Vector2.right*gizmoLength);
    }

    private float ClampToConstraints(float lightAngle) {
        if (!unconstrained) {
            lightAngle = Math.AngleDifference(0, lightAngle);
            return Mathf.Clamp(lightAngle, -(angleConstraint - apertureAngle/2) - .1f, angleConstraint - apertureAngle/2 + .1f);
        } else {
            return lightAngle;
        }
    }

    public void SetCurrentAngle(float angle) {
        currentAngle = angle;
    }

    public float GetCurrentAngle() {
        return currentAngle;
    }

    public void ApplySettings() {
        DoSnapping();
        DetectConstraints();

        angleConstraint = Mathf.Max(angleConstraint, apertureAngle/2);
        currentAngle = ClampToConstraints(currentAngle);
        if (controled != null) {
            SetControledAngle(currentAngle);
            controled.SetTargetApertureAngle(apertureAngle);
            controled.transform.localPosition = Vector3.zero;
        }
    }

    public override void DoSnapping() {
        if (!Application.isPlaying) {
            if (base.snapToGrid) {
                SnapPosition();
                body.position = transform.position;
            }
        }
    }

    private float GetControledAngle() {
        return controled.GetComponent<Rigidbody2D>().rotation;
    }

    private void SetControledAngle(float angle) {
        controled.GetComponent<Rigidbody2D>().rotation = body.rotation + angle;
        // When this is called in the editor, the transform doesn't update
        // automatically, so we need to change it manually.
        controled.transform.localRotation = Quaternion.Euler(0,0,angle);
    }


    private bool IsAngleUnoccupied(float angle) {
        foreach (RaycastHit2D hit in Physics2D.LinecastAll(body.position + Math.Rotate(Vector2.right, angle)*.1f, body.position + Math.Rotate(Vector2.right, angle)*.5f)) {
            if (PhysicsHelper.IsStatic(hit.rigidbody) && hit.rigidbody != body) {
                return false;
            }
        }

        return true;
    }


    // Uses raycasting to detect angular constraints on this light. For
    // example, if it is on a wall, this function will set body.rotation and
    // angleConstraint so that the light won't intersect the wall.
    private void DetectConstraints() {
        float inc = 45;

        var unoccupiedAngles = new List<bool>();

        for (int i = 0; i < 360/inc; i++) {
            unoccupiedAngles.Add(IsAngleUnoccupied((i+.5f)*inc));
        }
        Algorithm.Bounds bounds = Algorithm.FindContiguousWrapAround(unoccupiedAngles);

        if (bounds.IsEmpty() || bounds.IsFull()) {
            unconstrained = true;
            return;
        } else {
            unconstrained = false;
        }

        float lowerBound = (bounds.Lower()) * inc;
        float upperBound = (bounds.Upper()+1) * inc;
        if (upperBound < lowerBound) {
            lowerBound -= 360;
        }

        float prevRotation = body.rotation;
        body.rotation = (upperBound + lowerBound)/2;
        transform.rotation = Quaternion.Euler(0,0,body.rotation);
        currentAngle += prevRotation - body.rotation;
        angleConstraint = Math.CounterClockwiseAngleDifference(lowerBound, upperBound)/2;
    }
}
