using UnityEngine;

// This class shouldn't be used in new levels. Use LightAngler. Really this
// should be removed soon.
[RequireComponent(typeof(Rigidbody2D))]
public class LightAnglerDeprecated : LevelObject, Interactable {

    [SerializeField]
    private bool useConstraints = true;
    [SerializeField]
    [Range(-180,180)]
    private float angleLeft = 90;
    [SerializeField]
    [Range(-180,180)]
    private float angleRight = 90;
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
    public RotatableLight controled;
    //[SerializeField]
    //private float gizmoLength = 1;

    private RelativeJoint2D myJoint;

    public void Start() {
    }

    public void Interact(Vector2 direction) {
    }

    // This can't be interacted with directly
    public Vector2 GetPosition() {
        return Vector2.positiveInfinity;
    }

    public void MovePosition(int direction) {
    }

    public void ApplySettings() {
    }
}
