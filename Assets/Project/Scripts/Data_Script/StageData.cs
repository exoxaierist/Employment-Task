using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Data_Script;
using UnityEngine;

public class StageData : ScriptableObject
{
    public int stageIndex;
    public List<BoardBlockData> boardBlocks;
    public List<PlayingBlockData> playingBlocks;
    public List<WallData> Walls;
}

[System.Serializable]
public class StageJsonData
{
    public int stageIndex;
    public List<BoardBlockData> boardBlocks;
    public List<PlayingBlockData> playingBlocks;
    public List<WallData> Walls;
}

[System.Serializable]
public class StageJsonWrapper
{
    public StageJsonData Stage;
}

