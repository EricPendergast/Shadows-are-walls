#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

public partial class SceneTriggerLoader : MonoBehaviour {
    partial void EditorUpdate() {
        if (!Application.isPlaying) {
            EditorHelper.RecordObjectUndo(this, "Collect adjacent scene trigger loaders");
            if (adjacent == null) {
                adjacent = new List<SceneTriggerLoader>();
            }
            adjacent.Clear();
            foreach (var collider in GetComponents<Collider2D>()) {
                List<Collider2D> results = new List<Collider2D>();
                collider.OverlapCollider(
                    new ContactFilter2D {
                        useLayerMask = true,
                        layerMask = PhysicsHelper.triggerLoaderLayerMask,
                        useTriggers = true
                    }, results
                );
                foreach (var result in results) {
                    adjacent.AddRange(result.GetComponents<SceneTriggerLoader>());
                }
            }
        }
    }
}

#endif
