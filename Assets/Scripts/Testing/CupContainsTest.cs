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

    void OnDrawGizmosSelected() {
        Cup cup = new Cup(cup_p1, cup_p2, cup_p3);

        foreach (var side in cup.GetSides()) {
            Gizmos.DrawLine(side.p1, side.p2);
        }

        if (cup.Contains(point, epsilon)) {
            Gizmos.color = Color.green;
        } else {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawSphere(point, drawRadius);
    }
}
