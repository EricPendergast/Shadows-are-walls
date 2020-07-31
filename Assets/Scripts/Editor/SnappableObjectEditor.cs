using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SnappableObject), true)]
[CanEditMultipleObjects]
public class SnappableObjectEditor : Editor {

    public override void OnInspectorGUI() {
        base.DrawDefaultInspector();
    }

    public virtual void OnSceneGUI() {
        if (!Application.isPlaying) {
            Undo.RecordObject(target as SnappableObject, "Snap object");
            Undo.RecordObject((target as SnappableObject).transform, "Snap object");
            (target as SnappableObject).DoSnapping();
        }
    }
}
