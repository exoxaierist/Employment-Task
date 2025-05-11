
using UnityEngine;

public class WallObject : MonoBehaviour
{
    [SerializeField] MeshRenderer wallRenderer;
    [SerializeField] private GameObject arrow;

    public void SetWall(Material material, bool isCuttingBox)
    {
        wallRenderer.material = material;
        arrow.SetActive(isCuttingBox);
    }
}