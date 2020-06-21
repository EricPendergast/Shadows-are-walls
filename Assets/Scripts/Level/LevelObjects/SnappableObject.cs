using UnityEngine;

public class SnappableObject : MonoBehaviour {
    [SerializeField]
    protected bool snapToGrid = false;
    [SerializeField]
    protected float gridSize = 1;
    [SerializeField]
    protected Vector2 snapPoint = new Vector2(-.5f, -.5f);

    protected Vector2 Snap(Vector2 point) {
        if (gridSize == 0) {
            return point;
        }
        return Util.Round(point/gridSize)*gridSize;
    }

    protected void SnapPosition() {
        Vector2 point = transform.TransformPoint(snapPoint);
        var newPoint = Snap(point);
        transform.position += (Vector3)(newPoint - point);
    }

    protected void SnapScale() {
        if (gridSize == 0) {
            return;
        }
        transform.localScale = new Vector3(Mathf.Ceil(transform.localScale.x/gridSize), Mathf.Ceil(transform.localScale.y/gridSize), 1/gridSize)*gridSize;
    }

    public virtual void DoSnapping() {
        if (snapToGrid) {
            SnapPosition();
        }
    }
}
