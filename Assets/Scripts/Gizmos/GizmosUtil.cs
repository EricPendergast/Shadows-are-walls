using UnityEngine;

public class GizmosUtil {
    public static void DrawConstantWidthSphere(Vector2 point, float screenFraction) {
        Gizmos.DrawSphere(point, screenFraction*(Camera.current.orthographicSize));
    }
}
