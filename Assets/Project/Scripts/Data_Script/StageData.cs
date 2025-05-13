using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileContainer
{
    [Flags]
    public enum TileFlags
    {
        None = 0,
        Board = 1 << 0,
        Block = 1 << 1,
        //Goal = 1 << 2,
        Gimmick = 1 << 3,
    }

    public enum GimmickType
    {
        None,
        Star,
        //...
    }

    [System.Serializable]
    public struct GoalInfo
    {
        public Direction direction;
        public ColorType colorType;
    }

    public int x;
    public int y;

    public TileFlags flags;
    public ColorType colorType;
    public GimmickType gimmickType;

    public List<GoalInfo> goals = new();

    public bool HasFlag(TileFlags flag) => (flags & flag) != 0;
    public void SetFlag(TileFlags flag, bool state)
    {
        if (state) flags |= flag;
        else flags &= ~flag;

        //block can't exist without board
        if (HasFlag(TileFlags.Block) && !HasFlag(TileFlags.Board))
        {
            SetFlag(TileFlags.Block, false);
        }

        //can't have goals on boards
        if (HasFlag(TileFlags.Board)) goals.Clear();

        /*//goal and block can't coexist
        if (HasFlag(TileFlags.Goal) && HasFlag(TileFlags.Block))
        {
            SetFlag(TileFlags.Goal, false);
            SetFlag(TileFlags.Block, false);
            SetFlag(flag, state);
        }

        //remove goals if flag is set to false
        if (flag == TileFlags.Goal && !state) goals.Clear();*/
    }

    public GoalInfo GetGoalInfo(ColorType colorType)
    {
        foreach (var goal in goals)
        {
            if (goal.colorType == colorType) return goal;
        }
        return new();
    }
    public GoalInfo GetGoalInfo(Direction direction)
    {
        foreach (var goal in goals)
        {
            if (goal.direction == direction) return goal;
        }
        return new();
    }
    public bool TryGetGoalInfo(Direction direction, out GoalInfo outGoalInfo)
    {
        foreach (var goal in goals)
        {
            if (goal.direction == direction)
            {
                outGoalInfo = goal;
                return true;
            }
        }
        outGoalInfo = new();
        return false;
    }
    public void SetGoalInfo(GoalInfo goalInfo)
    {
        //clear goal
        if (goalInfo.colorType == ColorType.None)
        {
            goals.Clear();
            return;
        }

        //override existing goal if direction overlap
        for (var i = 0; i < goals.Count; i++)
        {
            if (goals[i].direction != goalInfo.direction) continue;
            goals[i] = goalInfo;
            return;
        }
        //add new if none exist
        goals.Add(goalInfo);
    }
    public void RemoveGoalInfo(Direction inDirection)
    {
        for (var i = 0; i < goals.Count; i++)
        {
            if (goals[i].direction != inDirection) continue;
            goals.RemoveAt(i);
            return;
        }
    }
}

[CreateAssetMenu(fileName = "StageData", menuName = "Stage Data")]
public class StageData : ScriptableObject
{
    public List<TileContainer> tiles = new();

    [NonSerialized] private Dictionary<Vector2Int, TileContainer> cachedMap;

    //clear tiles with no flags, no goals
    public void ClearEmpty()
    {
        HashSet<TileContainer> tilesToRemove = new();
        foreach (var tile in tiles)
        {
            if (tile.flags == 0 && tile.goals.Count == 0) tilesToRemove.Add(tile);
        }

        foreach (var tile in tilesToRemove)
        {
            tiles.Remove(tile);
        }
    }

    //cache list into dict for faster lookups
    public void CacheTiles()
    {
        cachedMap = new();
        foreach (var tile in tiles)
        {
            cachedMap.Add(new(tile.x, tile.y), tile);
        }
    }

    public bool TryGetTile(Vector2Int pos, out TileContainer outTile)
    {
        if (cachedMap.ContainsKey(pos))
        {
            outTile = cachedMap[pos];
            return true;
        }
        outTile = null;
        return false;
    }

    public string SerializeToJson()
    {
        ClearEmpty();
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}

