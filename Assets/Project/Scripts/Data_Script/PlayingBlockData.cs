
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayingBlockData : BlockBaseData
{
    public Vector2Int center;
    public int uniqueIndex;
    public ColorType colorType;
    public List<ShapeData> shapes;
    public List<GimmickData> gimmicks;
}

[System.Serializable]
public class ShapeData
{
    public Vector2Int offset;
    // public int offsetX;
    // public int offsetY;
}

// public enum BlockGimmickType
// {
//     None = 0,
//     Constraint,
//     Multiple,
//     Frozen,
//     Star,
//     Key,
//     Lock
// }

