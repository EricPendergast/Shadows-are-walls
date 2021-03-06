using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LightAngler))]
[CanEditMultipleObjects]
public class LightAnglerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }

    public void OnSceneGUI() {
        if (!Application.isPlaying) {
            var lightAngler = target as LightAngler;

            EditorGUI.BeginChangeCheck();
            var newRot = DiscHandle(lightAngler.EditorGetCurrentAngle(), 1f);

            if (EditorGUI.EndChangeCheck()) {
                 lightAngler.EditorSetCurrentAngle(newRot);
            } 

            EditorGUI.BeginChangeCheck();
            var newApertureAngle = DiscHandle(lightAngler.EditorGetApertureAngle(), -.6f);

            if (EditorGUI.EndChangeCheck()) {
                lightAngler.EditorSetApertureAngle(newApertureAngle);
            }
        }
    }

    private float DiscHandle(float currentAngle, float scale) {
        var lightAngler = target as LightAngler;
        float newAngle = Handles.Disc(
            Quaternion.Euler(0,0,currentAngle),
            (Vector2)lightAngler.transform.position,
            Vector3.back,
            HandleUtility.GetHandleSize(lightAngler.transform.position)*scale,
            false,
            1
        ).eulerAngles.z;

        return currentAngle + Math.AngleDifference(currentAngle, newAngle);
    }
}
