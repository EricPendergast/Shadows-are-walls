using UnityEngine;

public interface SimpleButtonControlable {
    void SetState(SimpleButton.State state);
}

public class SimpleButton : LevelObject {
    public enum State {
        unpressed,
        pressed
    }
    [SerializeField]
    private GameObject controledGameObject;
    private SimpleButtonControlable controled;

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
        foreach (var controlable in controledGameObject.GetComponentsInChildren<SimpleButtonControlable>()) {
            if (controled == null) {
                controled = controlable;
            } else {
                Debug.Log("Warning: controledGameObject has multiple SimpleButtonControlable components");
            }
        }
        if (controled == null) {
            Debug.LogError("Warning: controledGameObject has no SimpleButtonControlable component");
        }

        controled.SetState(State.unpressed);
    }

    void Press() {
        controled.SetState(State.pressed);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*.5f, 1);
    }
    void Unpress() {
        controled.SetState(State.unpressed);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y*2, 1);
    }
    void OnTriggerEnter2D(Collider2D collider) {
        if (!collider.isTrigger) {
            numObjectsPressing++;
            if (numObjectsPressing == 1) {
                Press();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        if (!collider.isTrigger) {
            numObjectsPressing--;
            if (numObjectsPressing == 0) {
                Unpress();
            }
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, controledGameObject.transform.position);
    }
}
