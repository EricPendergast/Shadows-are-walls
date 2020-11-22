using UnityEngine.Assertions;
using UnityEngine.Profiling;


// This inherits from AllTracker just for error checking
public class ScriptOrderManager : AllTracker<ScriptOrderManager> {

    void FixedUpdate() {
        // If there are multiple of these, very bad things may happen
        Assert.AreEqual(GetAllCount(), 1);

        Profiler.BeginSample("RotatableLights");
        foreach (var light in LightBase.GetAll()) {
            light.DoFixedUpdate();
        }
        Profiler.EndSample();

        Profiler.BeginSample("Shadows");
        foreach (var shadow in Shadow.GetAll()) {
            shadow.DoFixedUpdate();
        }
        Profiler.EndSample();

        Profiler.BeginSample("Shadow and Light Edges");
        foreach (var lightDivider in ShadowEdgeBase.GetAll()) {
            lightDivider.DoFixedUpdate();
        }
        Profiler.EndSample();
    }
}
