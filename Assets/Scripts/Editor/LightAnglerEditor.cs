using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LightAngler))]
[CanEditMultipleObjects]
public class LightAnglerEditor : LevelObjectEditor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }

    public override void OnSceneGUI() {
        if (!Application.isPlaying) {
            Undo.RecordObject((target as LightAngler).controled, "Automatically set light aperture angle");
            (target as LightAngler).ApplySettings();
        }
        base.OnSceneGUI();
    }
}
