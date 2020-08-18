using UnityEngine;

public interface SimpleLeverControlable {
    // TODO: Decide whether this should work with the old or new interaction
    // interface. This is not an obvious decision because levers have only one
    // axis of movement, so this interface still may be intuitive.
    void MovePosition(int direction);
}

public class SimpleLever : LevelObject, Lever, Interactable {
    [SerializeField]
    private GameObject controledGameObject;
    private SimpleLeverControlable controled;

    protected override void Awake() {
        base.Awake();
        gameObject.layer = PhysicsHelper.interactableLayer;
        collider.isTrigger = true;
        body.gravityScale = 0;
    }

    void Start() {
        controled = null;
        foreach (var controlable in controledGameObject.GetComponentsInChildren<SimpleLeverControlable>()) {
            if (controled == null) {
                controled = controlable;
            } else {
                Debug.Log("Warning: controledGameObject has multiple SimpleLeverControlable components");
            }
        }
        if (controled == null) {
            Debug.LogError("Warning: controledGameObject has no SimpleLeverControlable component");
        }
    }

    public Vector2 GetPosition() {
        return transform.position;
    }

    public void Interact(Vector2 direction) {
        if (direction.x < 0) {
            MovePosition(-1);
        } else if (direction.x > 0) {
            MovePosition(1);
        }
    }

    public void MovePosition(int direction) {
        if (controled != null) {
            controled.MovePosition(direction);
        }
    }

    protected override void OnDrawGizmosSelected() {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.magenta;
        if (controledGameObject != null) {
            Gizmos.DrawLine(transform.position, controledGameObject.transform.position);
        }
    }
}
