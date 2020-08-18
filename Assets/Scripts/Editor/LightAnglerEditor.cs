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
            foreach (Component c in Util.AllChildrenComponentsIter((target as LightAngler).gameObject)) {
                Undo.RecordObject(c, "Automatically set light aperture angle");
            }
            (target as LightAngler).ApplySettings();
        }
        base.OnSceneGUI();
    }
}
