#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public partial class Positioner : MonoBehaviour {
    void Update() {
        if (TryGetComponent<Snapper>(out var snapper)) {
            left = snapper.SnapPoint(left);
            right = snapper.SnapPoint(right);
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(left, right);
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

    public void EditorUpdate() {
        position = Mathf.Clamp01(position);
        if (controled != null) {
            controled.transform.position = Vector2.Lerp(left, right, position);
            controled.transform.localRotation = Quaternion.Euler(0,0,rotation);
        }
    }
}

#endif
