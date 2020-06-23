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
            Undo.RecordObject((target as LightAngler).controled, "Automatically set light aperture angle");
            (target as LightAngler).DoSnapping();
            (target as LightAngler).ApplySettings();
        }
    }
}
