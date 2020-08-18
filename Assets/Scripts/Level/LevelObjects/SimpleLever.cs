using UnityEngine;

public class SimpleLever : LevelObject, Interactable {
    [SerializeField]
    private GameObject controledGameObject;
    private Interactable controled;

    protected override void Awake() {
        base.Awake();
        gameObject.layer = PhysicsHelper.interactableLayer;
        collider.isTrigger = true;
        body.gravityScale = 0;
    }

    void Start() {
        controled = null;
        foreach (var controlable in controledGameObject.GetComponentsInChildren<Interactable>()) {
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
        controled.Interact(direction);
    }

    protected override void OnDrawGizmosSelected() {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.magenta;
        if (controledGameObject != null) {
            Gizmos.DrawLine(transform.position, controledGameObject.transform.position);
        }
    }
}
