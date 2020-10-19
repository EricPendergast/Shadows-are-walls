#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public partial class Snapper : MonoBehaviour {
    private void Update() {
        if (!Application.isPlaying) {
            Undo.RecordObject(transform, "Snap");
            PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
            DoSnapping();
        }
    }

    private void OnDrawGizmosSelected() {
        GizmosUtil.DrawConstantWidthSphere(transform.TransformPoint(snapPoint), .025f);
    }
}

#endif
