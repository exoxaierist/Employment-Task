namespace Project.Scripts.Data_Script
{
    [System.Serializable]
    public class WallData
    {
        public int x;
        public int y;
        public ObjectPropertiesEnum.WallDirection WallDirection;
        public int length;
        public ColorType wallColor;
        public WallGimmickType wallGimmickType;
    }
}

public enum WallGimmickType
{
    None = 0,
    Star
}