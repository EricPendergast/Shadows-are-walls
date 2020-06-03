public class FreeObject : LevelObject {
    protected override void Awake() {
        base.Awake();

        gameObject.layer = PhysicsHelper.freeObjectLayer;
    }
}
