using UnityEngine;

public class CupContainsTest : MonoBehaviour {
    public Vector2 point;
    //public Vector2 seg_p1;
    //public Vector2 seg_p2;
    public Vector2 cup_p1;
    public Vector2 cup_p2;
    public Vector2 cup_p3;

    public float epsilon = .0001f;
    public float drawRadius = .25f;

    void Update() {
        var _ = 
            TestHelpers.MouseMove(ref point, drawRadius) ||
            TestHelpers.MouseMove(ref cup_p1, drawRadius) ||
            TestHelpers.MouseMove(ref cup_p2, drawRadius) ||
            TestHelpers.MouseMove(ref cup_p3, drawRadius);
    }
    void OnDrawGizmosSelected() {
        Cup cup = new Cup(cup_p1, cup_p2, cup_p3);

        foreach (var side in cup.GetSides()) {
            Gizmos.DrawLine(side.p1, side.p2);
        }
        Gizmos.DrawSphere(cup_p3, drawRadius);

        if (cup.Contains(point, epsilon)) {
            Gizmos.color = Color.green;
        } else {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawSphere(point, drawRadius);
    }
}
