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
            (target as SnappableObject).DoSnapping();
        }
    }
}
