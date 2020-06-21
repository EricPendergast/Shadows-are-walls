using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LightAngler))]
[CanEditMultipleObjects]
public class LightAnglerEditor : Editor {
    public override void OnInspectorGUI() {
        base.DrawDefaultInspector();
    }

    public void OnSceneGUI() {
        if (!Application.isPlaying) {
            (target as LightAngler).DoSnapping();
            (target as LightAngler).ApplySettings();
        }
    }
}
