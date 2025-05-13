using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public List<BoardCondition> conditions = new();

    public void Set(BoardBlockData data)
    {
        transform.position = new(data.x * BoardGenerator.gridSize, 0, data.y * BoardGenerator.gridSize);
        conditions.AddRange(data.conditions);
    }
}

