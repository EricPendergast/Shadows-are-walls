using UnityEngine;

public class TestHelpers {
    public static bool MouseMove(ref Vector2 point, float radius) {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(0)) {
            if ((point - worldPosition).magnitude < radius) {
                point = worldPosition;
                return true;
            }
        }
        return false;
    }
}
