using UnityEngine;

public class SetInertia : MonoBehaviour {
    public int initialInertia = 1;


    void Start() {
        gameObject.GetComponent<Rigidbody2D>().inertia = initialInertia;
    }
}
