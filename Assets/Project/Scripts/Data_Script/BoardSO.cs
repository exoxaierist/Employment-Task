
using System.Collections.Generic;
using UnityEngine;

public class BoardSO : ScriptableObject
{
    public List<BoardBlockData> boardBlockDatas;
}

[System.Serializable]
public class BlockList
{
    public List<BoardBlockData> boardBlocks;
}