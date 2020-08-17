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
            // TODO: This is trying to make the editor save changes to the lampshade renderer, but it doesn't actually work.
            foreach (GameObject child in Util.AllChildrenIter((target as LightAngler).gameObject)) {
                Undo.RecordObject(child, "Automatically set light aperture angle");
            }
            (target as LightAngler).ApplySettings();
        }
        base.OnSceneGUI();
    }
}
