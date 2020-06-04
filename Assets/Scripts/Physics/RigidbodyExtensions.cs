using UnityEngine;

namespace RigidbodyExtensions {
    public static class Extensions {
        // Transforms from local to global position
        public static Vector2 TransformPoint(this Rigidbody2D body, Vector2 point) {
            return (Vector2)(Quaternion.Euler(0,0,body.rotation)*point) + body.position;
        }

        public static Vector2 InverseTransformPoint(this Rigidbody2D body, Vector2 point) {
            return (Vector2)(Quaternion.Euler(0,0,-body.rotation)*point) - body.position;
        }
    }
}
