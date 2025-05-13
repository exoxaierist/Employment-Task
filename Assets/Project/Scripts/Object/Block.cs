using UnityEngine;

public class Block : MonoBehaviour
{
    [HideInInspector] public BlockGroup parentGroup;
    [SerializeField] private Collider blockCollider;

    public void Set(BlockGroup parent)
    {
        parentGroup = parent;
    }

    public void DisableCollision()
    {
        blockCollider.enabled = false;
    }

    public bool CheckBoardForClear(out Direction outDirection, out Vector3 outExitCenter)
    {
        outDirection = Direction.Up;
        outExitCenter = Vector3.zero;

        Ray ray = new(transform.position, Vector3.down);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return false;
        if (!hit.collider.TryGetComponent(out Board board)) return false;

        foreach (var condition in board.conditions)
        {
            //color match
            if (condition.color != parentGroup.color) continue;

            Vector3 boardCenterPos = board.transform.position;
            //check if block group center is within exit range
            if (condition.direction.IsVertical())
            {
                boardCenterPos.x -= condition.offsetFromCenter * BoardGenerator.gridSize;
                float minRange = boardCenterPos.x - condition.totalLength * 0.5f * BoardGenerator.gridSize + parentGroup.groupSize.x * 0.5f * BoardGenerator.gridSize;
                float maxRange = boardCenterPos.x + condition.totalLength * 0.5f * BoardGenerator.gridSize - parentGroup.groupSize.x * 0.5f * BoardGenerator.gridSize;
                if (parentGroup.GetCenter().x.IsBetween(minRange, maxRange, tolerance: 0.1f))
                {
                    outDirection = condition.direction;
                    outExitCenter = boardCenterPos;
                    return true;
                }
            }
            else
            {
                boardCenterPos.z -= condition.offsetFromCenter * BoardGenerator.gridSize;
                outExitCenter = boardCenterPos;
                float minRange = boardCenterPos.z - condition.totalLength * 0.5f * BoardGenerator.gridSize + parentGroup.groupSize.y * 0.5f * BoardGenerator.gridSize;
                float maxRange = boardCenterPos.z + condition.totalLength * 0.5f * BoardGenerator.gridSize - parentGroup.groupSize.y * 0.5f * BoardGenerator.gridSize;
                if (parentGroup.GetCenter().z.IsBetween(minRange, maxRange, tolerance: 0.1f))
                {
                    outDirection = condition.direction;
                    outExitCenter = boardCenterPos;
                    return true;
                }
            }
            break;
        }

        return false;
    }

    public BlockGroup CheckForOtherGroupInDirection(Direction direction)
    {
        Ray ray = new(transform.position, direction.ToVector().ToXZ());
        //distance need to be changed if stage is concave
        float distance = (direction.IsHorizontal() ? parentGroup.groupSize.x : parentGroup.groupSize.y);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, LayerMask.GetMask("PlayingBlock"));
        if (hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider.TryGetComponent(out Block block)) continue;
                if (block.parentGroup == parentGroup) continue;
                return block.parentGroup;
            }
        }
        return null;
    }
}
