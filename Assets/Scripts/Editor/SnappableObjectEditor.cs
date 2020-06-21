using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SnappableObject), true)]
[CanEditMultipleObjects]
public class SnappableEditor : Editor {

    public override void OnInspectorGUI() {
        base.DrawDefaultInspector();
    }

    public void OnSceneGUI() {
        if (!Application.isPlaying) {
            (target as SnappableObject).DoSnapping();
        }
    }
}
