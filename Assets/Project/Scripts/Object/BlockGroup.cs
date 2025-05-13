using DG.Tweening;
using Monotone.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Watermelon.JellyMerge;

[RequireComponent(typeof(BlockPhysicsHandler))]
public class BlockGroup : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isDragging = false;
    private Vector3 mouseOffset;

    //group info
    public ColorType color;
    public Vector2 groupSize;
    public Vector3 offsetFromCenter;
    public Vector3 GetCenter() => transform.position - offsetFromCenter;
    public List<Block> blocks = new();

    //components
    private BlockPhysicsHandler physicsHandler;
    private Outline outline;

    //state
    private bool destroyPending = false;

    public void Set(PlayingBlockData data)
    {
        destroyPending = false;
        transform.position = new(data.x * BoardGenerator.gridSize, 0.2f, data.y * BoardGenerator.gridSize);

        color = data.colorType;

        int minX = 9999, minY = 9999, maxX = -9999, maxY = -9999;
        foreach (var pos in data.shapes)
        {
            //create child blocks
            Block instance = Instantiate(PrefabLibrary.Get("BlockComponent")).GetComponent<Block>();
            instance.transform.SetParent(transform);
            Vector3 position = (Vector2)pos.offset * BoardGenerator.gridSize;
            position.z = position.y;
            position.y = 0;
            instance.transform.localPosition = position;
            instance.Set(this);
            blocks.Add(instance);

            //center and size
            minX = Mathf.Min(minX, pos.offset.x);
            maxX = Mathf.Max(maxX, pos.offset.x);
            minY = Mathf.Min(minY, pos.offset.y);
            maxY = Mathf.Max(maxY, pos.offset.y);
        }
        offsetFromCenter = -((new Vector2(minX, minY) + new Vector2(maxX - minX, maxY - minY) * 0.5f) * BoardGenerator.gridSize).ToXZ();
        groupSize = new(maxX - minX + 1, maxY - minY + 1);

        //assign material color
        SkinnedMeshRenderer[] renderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        foreach (var rend in renderer)
        {
            rend.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color.ToBlockColor().WithAlpha(0.6f));
            rend.SetPropertyBlock(block);
        }

        //initialize physics handler
        physicsHandler = GetComponent<BlockPhysicsHandler>();
        physicsHandler.Initialize();

        //add outline
        outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 2f;
        outline.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (destroyPending) return;
        isDragging = true;
        outline.enabled = true;

        physicsHandler.SetDragging(isDragging);

        mouseOffset = transform.position - CommonUtility.GetPointerWorldPos();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (destroyPending) return;
        isDragging = false;
        physicsHandler.SetDragging(isDragging);

        outline.enabled = false;
        SnapToGrid();
        CheckForDestroy();
    }

    void FixedUpdate()
    {
        if (!isDragging) return;
        if (destroyPending) return;

        physicsHandler.SetPointerOffset(Vector3.ProjectOnPlane(CommonUtility.GetPointerWorldPos() + mouseOffset - transform.position, Vector3.up));
    }

    private void SnapToGrid()
    {
        Vector3 snapPosition = transform.position;
        snapPosition.x = Mathf.Round(snapPosition.x / BoardGenerator.gridSize) * BoardGenerator.gridSize;
        snapPosition.y = 0.2f;
        snapPosition.z = Mathf.Round(snapPosition.z / BoardGenerator.gridSize) * BoardGenerator.gridSize;
        transform.position = snapPosition;
    }

    //can this block be destroyed(cleared)
    private void CheckForDestroy()
    {
        Direction goalDirection = Direction.Up;
        Vector3 goalCenter = new();

        //check if child block is in clearable position
        bool onGoalBlock = false;
        foreach (var block in blocks)
        {
            if (block.CheckBoardForClear(out Direction direction, out Vector3 exitCenter))
            {
                goalDirection = direction;
                goalCenter = exitCenter;
                onGoalBlock = true;
                break;
            }
        }
        if (!onGoalBlock) return;

        //check if there are no other blocks blocking
        bool isBlocked = false;
        foreach (var subBlock in blocks)
        {
            BlockGroup other = subBlock.CheckForOtherGroupInDirection(goalDirection);
            if (other != null)
            {
                isBlocked = true;
                break;
            }
        }
        if (isBlocked) return;

        DestroyMove(goalDirection, goalCenter);
    }

    public void DestroyMove(Direction direction, Vector3 exitCenter)
    {
        //set particle transform
        ParticleSetuper particle = Instantiate(PrefabLibrary.Get("Particle")).GetComponent<ParticleSetuper>();
        particle.transform.position = exitCenter + direction.ToVector().ToXZ() * 0.5f * BoardGenerator.gridSize + Vector3.up * 0.5f;
        particle.transform.rotation = direction.ToRotation();
        Vector3 particleScale = Vector3.one * 0.5f;
        if (direction.IsHorizontal()) particleScale = Vector3.one * groupSize.y * 0.5f;
        else particleScale = Vector3.one * groupSize.x * 0.5f;
        particleScale.y = 0.5f;
        particle.transform.localScale = particleScale;

        //set particle material
        particle.SetColor(color.ToBlockColor());

        Vector3 dir = direction.ToVector();
        dir = new(dir.x * groupSize.x, 0, dir.y * groupSize.y);
        DestroyMove(transform.position + dir, particle.gameObject);
    }

    public void DestroyMove(Vector3 pos, GameObject particle)
    {
        OnDestroyInitiated();
        transform.DOMove(pos, 1f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Destroy(particle.gameObject);
                Destroy(gameObject);
                //block.GetComponent<BlockShatter>().Shatter();
            });
    }

    private void OnDestroyInitiated()
    {
        destroyPending = true;
        foreach (var block in blocks)
        {
            block.DisableCollision();
        }
    }

    private void OnDisable()
    {
        transform.DOKill(true);
    }

    private void OnDestroy()
    {
        transform.DOKill(true);
    }

}