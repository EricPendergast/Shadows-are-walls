using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelObject), true)]
[CanEditMultipleObjects]
public class LevelObjectEditor : SnappableObjectEditor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }

    public override void OnSceneGUI() {
        base.OnSceneGUI();
    }
}
