using UnityEngine;

public interface SimpleLeverControlable {
    // TODO: Is this a good name?
    void MovePosition(int direction);
}

public class SimpleLever : LevelObject, Lever {
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
