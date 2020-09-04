using UnityEngine;

public class SimpleButton : LevelObject {
    [SerializeField]
    private GameObject controledGameObject;
    private Interactable controled;
    [SerializeField]
    private Vector2 interactionDirection;

    [SerializeField]
    private int numObjectsPressing;

    protected override void Awake() {
        base.Awake();
        gameObject.layer = PhysicsHelper.interactableLayer;
        collider.isTrigger = true;
        body.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void Start() {
        controled = null;
        foreach (var controlable in controledGameObject.GetComponentsInChildren<Interactable>()) {
            if (controled == null) {
                controled = controlable;
            } else {
                Debug.Log("Warning: controledGameObject has multiple Interactable components");
            }
        }
        if (controled == null) {
            Debug.LogError("Warning: controledGameObject has no Interactable component");
        }
        numObjectsPressing = 0;
    }

    // NOTE: It is important that this happens in FixedUpdate, because it needs
    // to be in sync with LightAngler.
    void FixedUpdate() {
        if (numObjectsPressing > 0) {
            controled.Interact(interactionDirection);
        } else {
            controled.Interact(-interactionDirection);
        }
    }

    void Press() {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*.5f, 1);
    }

    void Unpress() {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*2, 1);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (!ShouldIgnore(collider)) {
            numObjectsPressing++;
            if (numObjectsPressing == 1) {
                Press();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (!ShouldIgnore(collider)) {
            numObjectsPressing--;
            if (numObjectsPressing == 0) {
                Unpress();
            }
        }
    }

    bool ShouldIgnore(Collider2D collider) {
        return collider.isTrigger || collider.attachedRigidbody.constraints == RigidbodyConstraints2D.FreezeAll || collider.attachedRigidbody.bodyType == RigidbodyType2D.Static;
    }

    protected override void OnDrawGizmosSelected() {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.magenta;
        if (controledGameObject != null) {
            Gizmos.DrawLine(transform.position, controledGameObject.transform.position);
        }
    }
}
