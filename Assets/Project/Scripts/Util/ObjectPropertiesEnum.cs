public class ObjectPropertiesEnum
{
    public enum WallDirection
    {
        None = 0,
        Single_Up,
        Single_Down,
        Single_Left,
        Single_Right,
        Left_Up,
        Left_Down,
        Right_Up,
        Right_Down,
        Open_Up,
        Open_Down,
        Open_Left,
        Open_Right
    }

    public enum BlockGimmickType
    {
        None = 0,
        Constraint,
        Multiple,
        Frozen,
        Star,
        Key,
        Lock
    }

    public enum WallGimmickType
    {
        None = 0,
        Lock_OnOff
    }
}
