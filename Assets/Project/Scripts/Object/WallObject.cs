
using UnityEngine;

public class WallObject : MonoBehaviour
{
    [SerializeField] private MeshRenderer wallRenderer;
    [SerializeField] private MeshFilter wallFilter;
    [SerializeField] private BoxCollider collider;
    [SerializeField] private GameObject stencil;
    [SerializeField] private GameObject arrow;
    [SerializeField] private Mesh[] wallMeshes;

    public void SetGoal(BoardGenerator.GoalGroup goalGroup)
    {
        SetWallMeshAndTransform(goalGroup.length, goalGroup.direction);

        MaterialPropertyBlock block = new();
        wallRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", goalGroup.color.ToWallColor());
        wallRenderer.SetPropertyBlock(block);

        arrow.SetActive(true);
    }

    public void Set(BoardGenerator.WallGroup wallGroup)
    {
        SetWallMeshAndTransform(wallGroup.length, wallGroup.direction);

        arrow.SetActive(false);
    }

    private void SetWallMeshAndTransform(int length, Direction direction)
    {
        if (length > wallMeshes.Length - 1)
        {
            Debug.LogError("Wall is longer then mesh");
            return;
        }

        //set collider size
        Vector3 size = collider.size;
        size.x *= length;
        collider.size = size;
        //set stencil size
        size = stencil.transform.localScale;
        size.x *= length;
        stencil.transform.localScale = size;

        //set mesh
        wallFilter.mesh = wallMeshes[length - 1];

        //set rotation
        transform.rotation = Quaternion.LookRotation(direction.ToVector().ToXZ());

        //set offset
        transform.position += direction.ToVector().ToXZ() * BoardGenerator.gridSize * 0.4f;
    }
}