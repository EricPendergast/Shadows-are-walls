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
            var lightAngler = target as LightAngler;

            EditorGUI.BeginChangeCheck();
            var newRot = Handles.RotationHandle(Quaternion.Euler(0,0,lightAngler.EditorGetCurrentAngle()), lightAngler.transform.position);
            if (EditorGUI.EndChangeCheck()) {
                 lightAngler.EditorSetCurrentAngle(newRot.eulerAngles.z);
            } 
        }
        base.OnSceneGUI();
    }
}
