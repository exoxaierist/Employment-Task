using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    public bool CheckCanDestroy(BoardBlockObject boardBlock, BlockObject block)
    {
        foreach (var checkGroupIdx in boardBlock.checkGroupIdx)
        {
            if (!boardBlock.isCheckBlock && !CheckBlockGroupDic.ContainsKey(checkGroupIdx)) return false;
        }

        //List<Vector2> checkCoordinates = new List<Vector2>();

        int pBlockminX = boardWidth;
        int pBlockmaxX = -1;
        int pBlockminY = boardHeight;
        int pBlockmaxY = -1;

        List<BlockObject> blocks = block.dragHandler.blocks;

        foreach (var playingBlock in blocks)
        {
            if (playingBlock.x <= pBlockminX) pBlockminX = (int)playingBlock.x;
            if (playingBlock.y <= pBlockminY) pBlockminY = (int)playingBlock.y;
            if (playingBlock.x >= pBlockmaxX) pBlockmaxX = (int)playingBlock.x;
            if (playingBlock.y >= pBlockmaxY) pBlockmaxY = (int)playingBlock.y;
        }

        List<BoardBlockObject> horizonBoardBlocks = new List<BoardBlockObject>();
        List<BoardBlockObject> verticalBoardBlocks = new List<BoardBlockObject>();

        foreach (var checkIndex in boardBlock.checkGroupIdx)
        {
            foreach (var boardBlockObj in CheckBlockGroupDic[checkIndex])
            {
                foreach (var horizon in boardBlockObj.isHorizon)
                {
                    if (horizon) horizonBoardBlocks.Add(boardBlockObj);
                    else verticalBoardBlocks.Add(boardBlockObj);
                }
                //checkCoordinates.Add(new Vector2(boardBlockObj.x, boardBlockObj.y));
            }
        }

        int matchingIndex = boardBlock.colorType.FindIndex(color => color == block.colorType);
        bool hor = boardBlock.isHorizon[matchingIndex];   
        //Horizon
        if (hor)
        {
            int minX = boardWidth;
            int maxX = -1;
            foreach (var coordinate in horizonBoardBlocks)
            {
                if (coordinate.x < minX) minX = (int)coordinate.x;

                if (coordinate.x > maxX) maxX = (int)coordinate.x;
            }

            // 개별 좌표가 나갔는지 여부를 판단.
            if (pBlockminX < minX - blockDistance / 2 || pBlockmaxX > maxX + blockDistance / 2)
            {
                return false;
            }

            (int, int)[] blockCheckCoors = new (int, int)[horizonBoardBlocks.Count];

            for (int i = 0; i < horizonBoardBlocks.Count; i++)
            {
                if (horizonBoardBlocks[i].y <= boardHeight / 2)
                {
                    int maxY = -1;

                    for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                    {
                        var currentBlock = block.dragHandler.blocks[k];

                        if (currentBlock.y == horizonBoardBlocks[i].y)
                        {
                            if (currentBlock.y > maxY)
                            {
                                maxY = (int)currentBlock.y;
                            }
                        }
                    }

                    blockCheckCoors[i] = ((int)horizonBoardBlocks[i].x, maxY);

                    for (int l = blockCheckCoors[i].Item2; l <= horizonBoardBlocks[i].y; l++)
                    {
                        if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                            continue;

                        (int, int) key = (blockCheckCoors[i].Item1, l);

                        if (boardBlockDic.ContainsKey(key) &&
                            boardBlockDic[key].playingBlock != null &&
                            boardBlockDic[key].playingBlock.colorType != boardBlock.horizonColorType)
                        {
                            return false;
                        }
                    }
                }
                // up to downside
                else
                {
                    int minY = 100;

                    for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                    {
                        var currentBlock = block.dragHandler.blocks[k];

                        if (currentBlock.y == horizonBoardBlocks[i].y)
                        {
                            if (currentBlock.y < minY)
                            {
                                minY = (int)currentBlock.y;
                            }
                        }
                    }

                    blockCheckCoors[i] = ((int)horizonBoardBlocks[i].x, minY);

                    for (int l = blockCheckCoors[i].Item2; l >= horizonBoardBlocks[i].y; l--)
                    {
                        if (blockCheckCoors[i].Item1 < pBlockminX || blockCheckCoors[i].Item1 > pBlockmaxX)
                            continue;
                        (int, int) key = (blockCheckCoors[i].Item1, l);

                        if (boardBlockDic.ContainsKey(key) &&
                            boardBlockDic[key].playingBlock != null &&
                            boardBlockDic[key].playingBlock.colorType != boardBlock.horizonColorType)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // Vertical
        else
        {
            int minY = boardHeight;
            int maxY = -1;

            foreach (var coordinate in verticalBoardBlocks)
            {
                if (coordinate.y < minY) minY = (int)coordinate.y;
                if (coordinate.y > maxY) maxY = (int)coordinate.y;
            }

            if (pBlockminY < minY - blockDistance / 2 || pBlockmaxY > maxY + blockDistance / 2)
            {
                return false;
            }

            (int, int)[] blockCheckCoors = new (int, int)[verticalBoardBlocks.Count];

            for (int i = 0; i < verticalBoardBlocks.Count; i++)
            {
                //x exist in left
                if (verticalBoardBlocks[i].x <= boardWidth / 2)
                {
                    int maxX = int.MinValue;

                    for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                    {
                        var currentBlock = block.dragHandler.blocks[k];

                        if (currentBlock.y == verticalBoardBlocks[i].y)
                        {
                            if (currentBlock.x > maxX)
                            {
                                maxX = (int)currentBlock.x;
                            }
                        }
                    }

                    // 튜플에 y와 maxX를 저장
                    blockCheckCoors[i] = (maxX, (int)verticalBoardBlocks[i].y);

                    for (int l = blockCheckCoors[i].Item1; l >= verticalBoardBlocks[i].x; l--)
                    {
                        if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                            continue;
                        (int, int) key = (l, blockCheckCoors[i].Item2);

                        if (boardBlockDic.ContainsKey(key) &&
                            boardBlockDic[key].playingBlock != null &&
                            boardBlockDic[key].playingBlock.colorType != boardBlock.verticalColorType)
                        {
                            return false;
                        }
                    }
                }
                //x exist in right
                else
                {
                    int minX = 100;

                    for (int k = 0; k < block.dragHandler.blocks.Count; k++)
                    {
                        var currentBlock = block.dragHandler.blocks[k];

                        if (currentBlock.y == verticalBoardBlocks[i].y)
                        {
                            if (currentBlock.x < minX)
                            {
                                minX = (int)currentBlock.x;
                            }
                        }
                    }

                    // 튜플에 y와 minX를 저장
                    blockCheckCoors[i] = (minX, (int)verticalBoardBlocks[i].y);

                    for (int l = blockCheckCoors[i].Item1; l <= verticalBoardBlocks[i].x; l++)
                    {
                        if (blockCheckCoors[i].Item2 < pBlockminY || blockCheckCoors[i].Item2 > pBlockmaxY)
                            continue;
                        (int, int) key = (l, blockCheckCoors[i].Item2);

                        if (boardBlockDic.ContainsKey(key) &&
                            boardBlockDic[key].playingBlock != null &&
                            boardBlockDic[key].playingBlock.colorType != boardBlock.verticalColorType)
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    public Material GetTargetMaterial(int index)
    {
        return wallMaterials[index];
    }
}