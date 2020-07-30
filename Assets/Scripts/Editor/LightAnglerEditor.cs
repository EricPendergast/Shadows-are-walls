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
            var controled = (target as LightAngler).controled;
            if (controled != null) {
                Undo.RecordObject(controled, "Automatically set light aperture angle");
            }
            (target as LightAngler).ApplySettings();
        }
        base.OnSceneGUI();
    }
}
