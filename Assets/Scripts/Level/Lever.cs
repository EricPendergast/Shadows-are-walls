
public abstract class Lever : LevelObject {
    //[SerializeField]
    //private float position;
    //
    //public abstract float GetPosition();
    // TODO: Maybe this should be an int, so that the lever can control the
    // rate at which it is pulled
    public abstract void MovePosition(int direction);
    //
    //// TODO: This is overly simple; there should be some kind of feedback from
    //// the thing which this lever controls. Maybe this method shouldn't even
    //// exist, it should just be MovePosition. Or maybe this should be an
    //// abstract class (or interface?), and none of the methods should be
    //// implemented.
    //public virtual void SetPosition(float position) {
    //    this.position = position;
    //}
    //
    //void MovePosition(int direction) {
    //    // TODO
    //}
}
