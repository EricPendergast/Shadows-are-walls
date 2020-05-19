using UnityEngine;

public class Mouse {
    public static Vector2 WorldPosition() {
       return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public static bool LeftMouseDown() {
        return Input.GetMouseButtonDown(0);
    }

    public static bool RightMouseDown() {
        return Input.GetMouseButtonDown(1);
    }

    public static bool LeftMouseUp() {
        return Input.GetMouseButtonUp(0);
    }

    public static bool RightMouseUp() {
        return Input.GetMouseButtonUp(1);
    }

    public static bool LeftMouse() {
        return Input.GetMouseButton(0);
    }

    public static bool RightMouse() {
        return Input.GetMouseButton(1);
    }
}
