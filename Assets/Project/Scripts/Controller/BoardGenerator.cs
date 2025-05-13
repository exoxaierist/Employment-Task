using Monotone.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class BoardGenerator
{
    public const float gridSize = 0.79f;

    public List<Board> boardObjects = new();
    public List<BlockGroup> blockGroupObjects = new();
    public List<WallObject> wallObjects = new();

    public void Generate(StageData stageData, Action onCompletedCallback = null)
    {
        DestroyAll();

        List<BoardBlockData> boards = new();
        List<PlayingBlockData> blocks = new();
        List<WallGroup> walls = new();
        List<GoalGroup> goals = new();
        ParseTileData(stageData, boards, blocks, walls, goals);

        GenerateBoard(boards);
        GenerateBlocks(blocks);
        GenerateWalls(walls);
        GenerateGoals(goals);

        onCompletedCallback?.Invoke();
    }

    public void DestroyAll()
    {
        foreach (var board in boardObjects)
        {
            if (board != null) Object.Destroy(board.gameObject);
        }
        foreach (var block in blockGroupObjects)
        {
            if (block != null) Object.Destroy(block.gameObject);
        }
        foreach (var wall in wallObjects)
        {
            if (wall != null) Object.Destroy(wall.gameObject);
        }
        boardObjects.Clear();
        blockGroupObjects.Clear();
        wallObjects.Clear();
    }

    public Bounds GetStageBounds()
    {
        Bounds bound = new();
        foreach (var board in boardObjects)
        {
            bound.Encapsulate(board.transform.position);
        }
        return bound;
    }

    //used for checking if wall is parsed
    private struct TempWallData
    {
        public int x;
        public int y;
        public Direction direction;
        public ColorType color;
    }
    //wall/goal groups for grouping wall sections
    public struct GoalGroup
    {
        public HashSet<Vector2Int> positions;
        public Direction direction;
        public ColorType color;
        public int length;
        public float GetOffsetFromCenter(Vector2 position)
        {
            Vector2 center = GetCenter();
            float offset = 0;
            if (direction.IsHorizontal())
            {
                offset = position.y - center.y;
            }
            else
            {
                offset = position.x - center.x;
            }
            return offset;
        }
        public Vector2 GetCenter()
        {
            Vector2 center = Vector2.zero;
            foreach (Vector2Int position in positions)
            {
                center += (Vector2)position;
            }
            return center / positions.Count;
        }
    }
    public struct WallGroup
    {
        public HashSet<Vector2Int> positions;
        public Direction direction;
        public int length;
        public float GetOffsetFromCenter(Vector2 position)
        {
            Vector2 center = GetCenter();
            float offset = 0;
            if (direction.IsHorizontal())
            {
                offset = position.y - center.y;
            }
            else
            {
                offset = position.x - center.x;
            }
            return offset;
        }
        public Vector2 GetCenter()
        {
            Vector2 center = Vector2.zero;
            foreach (Vector2Int position in positions)
            {
                center += (Vector2)position;
            }
            return center / positions.Count;
        }
    }
    private void ParseTileData(StageData stageData, List<BoardBlockData> boards, List<PlayingBlockData> blocks, List<WallGroup> walls, List<GoalGroup> goals)
    {
        stageData.ClearEmpty();
        stageData.CacheTiles();
        List<TileContainer> tiles = stageData.tiles;

        //parse goal groups
        HashSet<TempWallData> parsedGoals = new();
        foreach (TileContainer tile in tiles)
        {
            foreach (var goal in tile.goals)
            {
                //skip if already parsed
                if (parsedGoals.Contains(new() { x = tile.x, y = tile.y, color = goal.colorType, direction = goal.direction })) continue;

                GoalGroup group = new() { direction = goal.direction, color = goal.colorType, positions = new() };
                Vector2Int startPos = new(tile.x, tile.y);
                if (goal.direction.IsVertical())
                {
                    // check right side
                    while (stageData.TryGetTile(startPos + Vector2Int.right, out TileContainer outTile))
                    {
                        startPos = startPos + Vector2Int.right;
                        if (outTile.goals.Count == 0) break;
                        if (!outTile.TryGetGoalInfo(goal.direction, out var outGoal)) break;
                        if (outGoal.colorType != goal.colorType) break;
                        parsedGoals.Add(new() { x = startPos.x, y = startPos.y, color = goal.colorType, direction = goal.direction });
                        group.positions.Add(startPos);
                    }
                    //check left side
                    startPos = new(tile.x + 1, tile.y);
                    while (stageData.TryGetTile(startPos + Vector2Int.left, out TileContainer outTile))
                    {
                        startPos = startPos + Vector2Int.left;
                        if (outTile.goals.Count == 0) break;
                        if (!outTile.TryGetGoalInfo(goal.direction, out var outGoal)) break;
                        if (outGoal.colorType != goal.colorType) break;
                        parsedGoals.Add(new() { x = startPos.x, y = startPos.y, color = goal.colorType, direction = goal.direction });
                        group.positions.Add(startPos);
                    }
                    //add group
                    group.length = group.positions.Count;
                    goals.Add(group);
                }
                else
                {
                    //check top side
                    while (stageData.TryGetTile(startPos + Vector2Int.up, out TileContainer outTile))
                    {
                        startPos = startPos + Vector2Int.up;
                        if (outTile.goals.Count == 0) break;
                        if (!outTile.TryGetGoalInfo(goal.direction, out var outGoal)) break;
                        if (outGoal.colorType != goal.colorType) break;
                        parsedGoals.Add(new() { x = startPos.x, y = startPos.y, color = goal.colorType, direction = goal.direction });
                        group.positions.Add(startPos);
                    }
                    //check bottom side
                    startPos = new(tile.x, tile.y + 1);
                    while (stageData.TryGetTile(startPos + Vector2Int.down, out TileContainer outTile))
                    {
                        startPos = startPos + Vector2Int.down;
                        if (outTile.goals.Count == 0) break;
                        if (!outTile.TryGetGoalInfo(goal.direction, out var outGoal)) break;
                        if (outGoal.colorType != goal.colorType) break;
                        parsedGoals.Add(new() { x = startPos.x, y = startPos.y, color = goal.colorType, direction = goal.direction });
                        group.positions.Add(startPos);
                    }
                    //add group
                    group.length = group.positions.Count;
                    goals.Add(group);
                }
            }
        }

        //parse board
        foreach (TileContainer tile in tiles)
        {
            if (tile.HasFlag(TileContainer.TileFlags.Board))
            {
                BoardBlockData board = new();
                board.x = tile.x;
                board.y = tile.y;
                board.conditions = new();
                //check if goal is neighboring
                foreach (var goal in goals)
                {
                    //right
                    if (goal.positions.Contains(new(tile.x + 1, tile.y)) &&
                        goal.direction == Direction.Left)
                    {
                        board.conditions.Add(new()
                        {
                            direction = goal.direction.Invert(),
                            color = goal.color,
                            totalLength = goal.length,
                            offsetFromCenter = goal.GetOffsetFromCenter(new(tile.x, tile.y))
                        });
                    }
                    //left
                    if (goal.positions.Contains(new(tile.x - 1, tile.y)) &&
                        goal.direction == Direction.Right)
                    {
                        board.conditions.Add(new()
                        {
                            direction = goal.direction.Invert(),
                            color = goal.color,
                            totalLength = goal.length,
                            offsetFromCenter = goal.GetOffsetFromCenter(new(tile.x, tile.y))
                        });
                    }
                    //up
                    if (goal.positions.Contains(new(tile.x, tile.y + 1)) &&
                        goal.direction == Direction.Down)
                    {
                        board.conditions.Add(new()
                        {
                            direction = goal.direction.Invert(),
                            color = goal.color,
                            totalLength = goal.length,
                            offsetFromCenter = goal.GetOffsetFromCenter(new(tile.x, tile.y))
                        });
                    }
                    //down
                    if (goal.positions.Contains(new(tile.x, tile.y - 1)) &&
                        goal.direction == Direction.Up)
                    {
                        board.conditions.Add(new()
                        {
                            direction = goal.direction.Invert(),
                            color = goal.color,
                            totalLength = goal.length,
                            offsetFromCenter = goal.GetOffsetFromCenter(new(tile.x, tile.y))
                        });
                    }
                }
                boards.Add(board);
            }
        }

        //parse blocks
        foreach (var tile in tiles)
        {
            if (!tile.HasFlag(TileContainer.TileFlags.Block)) continue;

            //check if color already exist in blocks
            PlayingBlockData data = blocks.Find(x => x.colorType == tile.colorType);
            if (data == null)
            {
                //add new
                data = new() { x = tile.x, y = tile.y };
                data.colorType = tile.colorType;
                data.shapes = new();
                data.shapes.Add(new() { offset = Vector2Int.zero });
                blocks.Add(data);
            }
            else
            {
                //add shape offset
                data.shapes.Add(new() { offset = new Vector2Int(tile.x - data.x, tile.y - data.y) });
            }
        }

        //parse walls
        HashSet<TempWallData> visitedWalls = new();
        foreach (var tile in tiles)
        {
            Vector2Int tilePos = new(tile.x, tile.y);
            CheckWall(stageData, visitedWalls, walls, tilePos, Direction.Right);
            CheckWall(stageData, visitedWalls, walls, tilePos, Direction.Left);
            CheckWall(stageData, visitedWalls, walls, tilePos, Direction.Up);
            CheckWall(stageData, visitedWalls, walls, tilePos, Direction.Down);
        }

    }

    private void GenerateBoard(List<BoardBlockData> blocks)
    {
        Transform boardParent = new GameObject("Board").transform;
        foreach (var block in blocks)
        {
            Board instance = Object.Instantiate(PrefabLibrary.Get("Board")).GetComponent<Board>();
            instance.transform.SetParent(boardParent);
            instance.Set(block);
            boardObjects.Add(instance);
        }
    }

    private void GenerateBlocks(List<PlayingBlockData> blocks)
    {
        Transform blockParent = new GameObject("Blocks").transform;
        foreach (var block in blocks)
        {
            BlockGroup group = Object.Instantiate(PrefabLibrary.Get("BlockGroup")).GetComponent<BlockGroup>();
            group.transform.SetParent(blockParent);
            group.Set(block);
            blockGroupObjects.Add(group);
        }
    }

    Transform wallParent;
    private void GenerateWalls(List<WallGroup> walls)
    {
        if (wallParent == null) wallParent = new GameObject("Walls").transform;
        foreach (var wall in walls)
        {
            WallObject instance = Object.Instantiate(PrefabLibrary.Get("Wall")).GetComponent<WallObject>();
            instance.transform.SetParent(wallParent);
            instance.transform.position = wall.GetCenter().ToXZ() * BoardGenerator.gridSize;
            instance.Set(wall);
            wallObjects.Add(instance);
        }
    }

    private void GenerateGoals(List<GoalGroup> goals)
    {
        if (wallParent == null) wallParent = new GameObject("Walls").transform;
        foreach (var goal in goals)
        {
            WallObject instance = Object.Instantiate(PrefabLibrary.Get("Wall")).GetComponent<WallObject>();
            instance.transform.SetParent(wallParent);
            instance.transform.position = goal.GetCenter().ToXZ() * BoardGenerator.gridSize;
            instance.SetGoal(goal);
            wallObjects.Add(instance);
        }
    }

    private void CheckWall(StageData stageData, HashSet<TempWallData> visitedWalls, List<WallGroup> walls, Vector2Int tilePos, Direction checkDirection)
    {
        const int maxWallLength = 5;
        Direction wallDirection = checkDirection.Invert();
        Vector2Int checkOffset = Vector2Int.RoundToInt(checkDirection.ToVector());
        Vector2Int ortho = new(checkOffset.y, -checkOffset.x);

        //right empty
        if (!stageData.TryGetTile(tilePos + checkOffset, out TileContainer _))
        {
            WallGroup wall = new();
            wall.direction = wallDirection;
            wall.positions = new();
            Vector2Int testPos = tilePos + checkOffset;
            TempWallData wallData = new() { x = testPos.x, y = testPos.y, direction = wallDirection };
            //check orthogonal direction
            while (!visitedWalls.Contains(wallData))
            {
                //if it has tile with board, or goal in same direction, break
                if (stageData.TryGetTile(testPos, out TileContainer _temp))
                {
                    if (_temp.HasFlag(TileContainer.TileFlags.Board)) break;
                    if (_temp.TryGetGoalInfo(wallDirection, out TileContainer.GoalInfo _tempGoal)) break;
                }
                //if it has no board connect, break
                if (!stageData.TryGetTile(testPos - checkOffset, out _temp) ||
                    !_temp.HasFlag(TileContainer.TileFlags.Board)) break;
                //if already visited, break
                if (visitedWalls.Contains(wallData)) break;
                //if wall is longer then mesh (max 6), break
                if (wall.positions.Count >= maxWallLength) break;
                visitedWalls.Add(wallData);
                wall.positions.Add(testPos);

                testPos -= ortho;
                wallData.x = testPos.x;
                wallData.y = testPos.y;
            }

            //check other direction
            testPos = tilePos + checkOffset + ortho;
            wallData.x = testPos.x;
            wallData.y = testPos.y;
            while (!visitedWalls.Contains(wallData))
            {
                //if it has tile with board, or goal in same direction, break
                if (stageData.TryGetTile(testPos, out TileContainer _temp))
                {
                    if (_temp.HasFlag(TileContainer.TileFlags.Board)) break;
                    if (_temp.TryGetGoalInfo(wallDirection, out TileContainer.GoalInfo _tempGoal)) break;
                }
                //if it has no board connect, break
                if (!stageData.TryGetTile(testPos - checkOffset, out _temp) ||
                    !_temp.HasFlag(TileContainer.TileFlags.Board)) break;
                //if already visited, break
                if (visitedWalls.Contains(wallData)) break;
                //if wall is longer then mesh (max 6), break
                if (wall.positions.Count >= maxWallLength) break;
                visitedWalls.Add(wallData);
                wall.positions.Add(testPos);

                testPos += ortho;
                wallData.x = testPos.x;
                wallData.y = testPos.y;
            }

            //add wall
            wall.length = wall.positions.Count;
            if (wall.length >= 1) walls.Add(wall);
        }
    }
}
