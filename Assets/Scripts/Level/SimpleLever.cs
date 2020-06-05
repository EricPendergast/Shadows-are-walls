using UnityEngine;

public interface SimpleLeverControlable {
    // TODO: Is this a good name?
    void MovePosition(int direction);
}

public class SimpleLever : Lever {
    [SerializeField]
    private GameObject controledGameObject;
    private SimpleLeverControlable controled;

    protected override void Awake() {
        base.Awake();
        gameObject.layer = PhysicsHelper.interactableLayer;
        collider.isTrigger = true;
        body.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void Start() {
        controled = controledGameObject.GetComponent<SimpleLeverControlable>();
        if (controled == null) {
            Debug.LogError("Warning: controledGameObject has no SimpleLeverControlable component");
        }
    }

    public override void MovePosition(int direction) {
        if (controled != null) {
            controled.MovePosition(direction);
        }
    }
}
