using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public partial class LightAngler : LevelObject, Interactable {

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

    [SerializeField]
    private float springConstant;
    [SerializeField]
    private float dampingConstant;

    [SerializeField]
    private float maxAccel;


    private RelativeJoint2D myJoint;

    public void Start() {
        if (!Application.isPlaying) {
            return;
        }
        if (!gameObject.TryGetComponent(out myJoint)) {
            myJoint = gameObject.AddComponent<RelativeJoint2D>();
        }
        myJoint.connectedBody = controled.GetComponent<Rigidbody2D>();
        //myJoint.maxForce = 80;
        //myJoint.maxTorque = 10;
        myJoint.maxForce = 0;
        myJoint.maxTorque = 0;
        myJoint.autoConfigureOffset = false;
        myJoint.angularOffset = currentAngle;
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
        controled.ApplyAngularAcceleration(direction * acceleration);
        
        controled.SetRotationConstraints(
            new RotatableLight.RotationConstraints{
                lower = body.rotation - angleConstraint,
                upper = body.rotation + angleConstraint,
                unconstrained = unconstrained
            }
        );
    }

    private void RotateOld(int direction) {
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

        float maxDiff = 2*speed*Time.deltaTime;
        if (direction == 0) {
            maxDiff /= 2;
        }
      
        var actualAngle = controled.GetRotation() - body.rotation;
        var difference = Math.AngleDifference(actualAngle, currentAngle);

        if (Mathf.Abs(difference) > maxDiff) {
            difference = Mathf.Clamp(difference, -maxDiff, maxDiff);
            currentAngle = actualAngle + difference;
        }
        currentAngle = ClampToConstraints(currentAngle);

        var rotAccel = -PhysicsHelper.GetSpringTorque(actualAngle, currentAngle, 0, 0, springConstant, dampingConstant);
        rotAccel = Mathf.Clamp(rotAccel, -maxAccel, maxAccel);
        //controled.ApplyAngularAcceleration(rotAccel);
        //Debug.Log("Applied " + rotAccel);
        //myJoint.angularOffset = actualAngle + difference;
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


    private float ClampToConstraints(float lightAngle) {
        if (!unconstrained) {
            lightAngle = Math.AngleDifference(0, lightAngle);
            return Mathf.Clamp(lightAngle, -(angleConstraint - apertureAngle/2) - .1f, angleConstraint - apertureAngle/2 + .1f);
        } else {
            return lightAngle;
        }
    }

}
