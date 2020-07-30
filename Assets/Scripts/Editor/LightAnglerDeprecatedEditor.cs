using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LightAnglerDeprecated))]
[CanEditMultipleObjects]
public class LightAnglerDeprecatedEditor : LevelObjectEditor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }

    public override void OnSceneGUI() {
        if (!Application.isPlaying) {
            Undo.RecordObject((target as LightAnglerDeprecated).controled, "Automatically set light aperture angle");
            (target as LightAnglerDeprecated).ApplySettings();
        }
        base.OnSceneGUI();
    }
}
