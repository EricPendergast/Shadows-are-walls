using UnityEngine;

public class ForceTest : MonoBehaviour {
    public Vector2 position;
    public Vector2 delta = new Vector2(-.01f, 0);
    public Vector2 projectedPosition = Vector2.zero;

    void OnDrawGizmosSelected() {
        Gizmos.DrawSphere(position, .1f);
        Gizmos.DrawLine(GetComponent<Rigidbody2D>().position, projectedPosition);
    }

    void FixedUpdate() {
        var body = GetComponent<Rigidbody2D>();
        var force = PhysicsHelper.GetMoveToForce(body, position);
        Debug.Log(force);
        body.AddForce(force);

        projectedPosition = body.position + body.velocity*Time.deltaTime + .5f*force/body.mass*Time.deltaTime*Time.deltaTime;
        position += delta;
    }

}
