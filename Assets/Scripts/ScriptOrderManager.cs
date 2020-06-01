using UnityEngine.Assertions;


// This inherits from AllTracker just for error checking
public class ScriptOrderManager : AllTracker<ScriptOrderManager> {

    void FixedUpdate() {
        // If there are multiple of these, very bad things may happen
        Assert.AreEqual(GetAllCount(), 1);

        foreach (var light in LightBase.GetAll()) {
            light.DoFixedUpdate();
        }

        foreach (var shadow in Shadow.GetAll()) {
            shadow.DoFixedUpdate();
        }

        foreach (var lightDivider in DividesLight.GetAll()) {
            lightDivider.DoFixedUpdate();
        }
    }
}
