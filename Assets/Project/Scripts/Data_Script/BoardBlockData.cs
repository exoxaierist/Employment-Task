using System.Collections.Generic;

[System.Serializable]
public class BoardBlockData : BlockBaseData
{
    public List<BoardCondition> conditions;
}

[System.Serializable]
public struct BoardCondition
{
    public ColorType color;
    public Direction direction;
    public float offsetFromCenter;
    public int totalLength;
}
