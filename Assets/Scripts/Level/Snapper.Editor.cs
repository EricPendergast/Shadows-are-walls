#if UNITY_EDITOR

using UnityEngine;

[ExecuteAlways]
public partial class Snapper : MonoBehaviour {
    private void Update() {
        if (!Application.isPlaying) {
            EditorHelper.RecordObjectUndo(transform, "Snap");
            DoSnapping();
        }
    }

    private void OnDrawGizmosSelected() {
        GizmosUtil.DrawConstantWidthSphere(transform.TransformPoint(snapPoint), .025f);
    }
}

#endif
