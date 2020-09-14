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
            foreach (Component c in Util.AllChildrenComponentsIter(lightAngler.gameObject)) {
                Undo.RecordObject(c, "Automatically set light aperture angle");
            }

            EditorGUI.BeginChangeCheck();
            var newRot = Handles.RotationHandle(Quaternion.Euler(0,0,lightAngler.GetCurrentAngle()), lightAngler.transform.position);
            if (EditorGUI.EndChangeCheck()) {
                 lightAngler.SetCurrentAngle(newRot.eulerAngles.z);
            } 


            lightAngler.ApplySettings();
        }
        base.OnSceneGUI();
    }
}
