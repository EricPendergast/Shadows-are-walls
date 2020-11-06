#if UNITY_EDITOR

using UnityEngine;

public partial class Positioner : MonoBehaviour {

    public void EditorSetPosition(Vector2 position) {
        EditorHelper.RecordObjectUndo(this, "Set position");

        Vector2 toPosition = position - (Vector2)transform.position;
        Vector2 toDestination = destination - (Vector2)transform.position;

        this.position = toPosition.magnitude / toDestination.magnitude*Mathf.Sign(Vector2.Dot(toPosition, toDestination));
        ApplySettings();
    }

    public Vector2 EditorGetPosition() {
        return Vector2.Lerp(transform.position, destination, position);
    }

    public Vector2 EditorGetOrigin() {
        return transform.position;
    }
    public Vector2 EditorGetDestination() {
        return destination;
    }

    public void EditorSetDestination(Vector2 destination) {
        EditorHelper.RecordObjectUndo(this, "Set destination");
        this.destination = destination;
        ApplySettings();
    }

    void Update() {
        if (!Application.isPlaying) {
            ApplySettings();
        }
    }

    void ApplySettings() {
        EditorHelper.RecordObjectUndo(this, "Apply Settings");

        if (TryGetComponent<Snapper>(out var snapper)) {
            snapper.DoSnapping();
            localDestination = snapper.SnapLocalPoint(localDestination);
        }
        position = Mathf.Clamp01(position);

        if (controled != null) {
            EditorHelper.RecordObjectUndo(controled.transform, "Apply Settings");

            controled.transform.position = Vector2.Lerp(transform.position, destination, position);
            controled.transform.localRotation = Quaternion.Euler(0,0,rotation);
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, destination);
    }

    //void OnDrawGizmosSelected() {
    //    if (left != null && right != null) {
    //        left.Draw();
    //        right.Draw();
    //    }
    //    Draw();
    //}

    //public void Draw() {
    //    Gizmos.color = Color.cyan;
    //    if (left != null && right != null) {
    //        Gizmos.DrawLine(left, right);
    //    }
    //}
}

#endif
