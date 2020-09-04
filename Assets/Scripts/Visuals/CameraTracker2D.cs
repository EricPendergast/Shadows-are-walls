using UnityEngine;

public class CameraTracker2D : MonoBehaviour
{
    public static CameraTracker2D instance;
    public GameObject trackedObject;
    public float trackSpeed;
    public float maxDistance;
    //public float springConstant;
    //public float springDampingConstant;

    private Rigidbody2D rb;

    void Start() {
        if (instance != null) {
            Debug.LogError("Error: Multiple CameraTracker2D objects in scene");
        }
        instance = this;
    }

    // For some reason, having this be FixedUpdate makes it smoother.
    void FixedUpdate() {
        if (trackedObject == null) {
            return;
        }
        if (rb == null) {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }

        Vector2 relPos = rb.position - (Vector2)trackedObject.transform.position;
        
        if (relPos.magnitude > maxDistance) {
            relPos = relPos.normalized * maxDistance;
            rb.position = (Vector2)trackedObject.transform.position + relPos;
        }
        
        //rb.AddForce(-relPos*springConstant);
        //rb.velocity -= springDampingConstant*rb.velocity*Time.deltaTime;

        relPos = relPos - relPos*trackSpeed*Time.deltaTime;

        rb.position = relPos + (Vector2)trackedObject.transform.position;
    }
}
