using UnityEngine;

public class LightEdge : DividesLight {
    [SerializeField]
    private bool DEBUG = false;

    public void SetTargetLength(float newLength) {
        target = target.WithLength(newLength);
    }

    public override LineSegment GetDivider() {
        return GetActual();
        //return target;
    }

    public override void DoFixedUpdate() {
        if (DEBUG) {
            Debug.Log("Break point");
        }
        UpdateColliders();
        AddSimpleForces();
    }

}
