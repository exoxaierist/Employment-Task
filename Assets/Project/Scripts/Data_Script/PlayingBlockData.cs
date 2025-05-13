using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayingBlockData : BlockBaseData
{
    public Vector2Int center;
    public ColorType colorType;
    public List<ShapeData> shapes;
}

[System.Serializable]
public class ShapeData
{
    public Vector2Int offset;
}

