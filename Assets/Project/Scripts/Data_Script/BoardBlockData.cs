
using System.Collections.Generic;

[System.Serializable]
public class BoardBlockData : BlockBaseData
{
    public List<ColorType> colorType;
    public List<int> dataType;
}

public enum DestroyWallDirection
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4
}
