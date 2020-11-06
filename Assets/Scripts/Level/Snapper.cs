using UnityEngine;

public partial class Snapper : MonoBehaviour {
    [SerializeField]
    protected bool enable = false;
    [SerializeField]
    protected float gridSize = 1;
    [SerializeField]
    protected Vector2 snapPoint = new Vector2(-.5f, -.5f);

    private Vector2 ParentPosition() {
        return 
            transform.parent != null ?
            transform.parent.position :
            Vector3.zero;
    }

    public Vector2 SnapGlobalPoint(Vector2 point) {
        return SnapLocalPoint(point - ParentPosition()) + ParentPosition();
    }

    public Vector2 SnapLocalPoint(Vector2 point) {
        if (gridSize == 0) {
            return point;
        }
        return Util.Round(point/gridSize)*gridSize;
    }

    public void DoSnapping() {
        if (enable) {
            if (Application.isPlaying) {
                EditorHelper.RecordObjectUndo(transform, "Snap");
            }
            SnapScale();
            SnapPosition();
        }
    }

    void SnapPosition() {
        Vector2 currentPoint =
            (Vector2)transform.TransformPoint(snapPoint) -
            ParentPosition();

        var newPoint = SnapLocalPoint(currentPoint);

        // This prevents the scene from being dirtied occasionally due to float
        // precision issues
        if (!Mathf.Approximately(newPoint.x, currentPoint.x) || !Mathf.Approximately(newPoint.y, currentPoint.y)) {
            transform.localPosition += (Vector3)(newPoint - currentPoint);
        }
        if (gameObject.TryGetComponent<Rigidbody2D>(out var body)) {
            body.position = transform.position;
        }
    }

    void SnapScale() {
        if (gridSize == 0) {
            return;
        }
        transform.localScale = new Vector3(
            Mathf.Ceil(transform.localScale.x/gridSize),
            Mathf.Ceil(transform.localScale.y/gridSize),
            1/gridSize)*gridSize;
    }
}
