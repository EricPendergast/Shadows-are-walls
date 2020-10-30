[System.Serializable]
public class LightSettings {
    public float mult = 1;
    public float maxAccel = 1;
    public float minAccel = .1f;
    public float resolveConstant = .1f;
    public float maxResolve = 1;
    public float inertia = 1000;
    public float centralAccelerationConstant = 0;
    public float minVelocity = .01f;
}
