using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    [SerializeField] private GameObject quadPrefab;
    private float yoffset = 0.625f;
    private float wallOffset = 0.225f;
    private List<GameObject> quads = new List<GameObject>();

    private void CreateMaskingTemp()
    {
        foreach (var quad in quads)
        {
            Destroy(quad);
        }
        quads.Clear();
        
        for (int i = -3; i <= boardWidth + 3; i++)
        {
            for (int j = -3; j <= boardHeight + 3; j++)
            {
                if (boardBlockDic.ContainsKey((i, j))) continue;

                float xValue = i;
                float zValue = j;
                if (i == -1 && j <= boardHeight) xValue -= wallOffset;
                if (i == boardWidth + 1 && j <= boardHeight + 1) xValue += wallOffset;
                
                if (j == -1 && i <= boardWidth) zValue -= wallOffset;
                if (j == boardHeight + 1 && i <= boardWidth + 1) zValue += wallOffset;
                
                GameObject quad = GameObject.Instantiate(quadPrefab, quadTr);
                quads.Add(quad);
                
                quad.transform.position = blockDistance * new Vector3(xValue, yoffset, zValue);
            }
        }
    }
}