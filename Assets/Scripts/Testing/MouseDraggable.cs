using UnityEngine;

public class MouseDraggable : MonoBehaviour {
    private TargetJoint2D dragJoint = null;

    void OnMouseDown() {
        if (dragJoint != null) {
            Destroy(dragJoint);
        }

        dragJoint = gameObject.AddComponent<TargetJoint2D>();
    }

    void OnMouseUp() {
        Destroy(dragJoint);
    }

    void Update() {
        if (dragJoint != null) {
            dragJoint.target = Mouse.WorldPosition();
        }
    }
}
