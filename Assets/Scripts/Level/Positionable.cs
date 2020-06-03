using UnityEngine;

public interface Positionable {
    void SetTargetPosition(Vector2 p);
    Vector2 GetTargetPosition();
    Vector2 GetActualPosition();
}
