using UnityEngine;

public partial class PlayerLoader : MonoBehaviour {
    void OnDrawGizmos() {
        Gizmos.DrawSphere(transform.position, 1);
    }
}
