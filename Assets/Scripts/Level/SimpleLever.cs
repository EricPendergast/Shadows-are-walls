using UnityEngine;

public interface SimpleLeverControlable {
    // TODO: Is this a good name?
    void MovePosition(int direction);
}

public class SimpleLever : Lever {
    [SerializeField]
    private GameObject controledGameObject;
    private SimpleLeverControlable controled;

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
