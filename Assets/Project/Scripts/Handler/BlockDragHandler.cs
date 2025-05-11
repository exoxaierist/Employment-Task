
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BlockDragHandler : MonoBehaviour
{
    public int horizon = 1;
    public int vertical = 1;
    public int uniqueIndex;
    public List<ObjectPropertiesEnum.BlockGimmickType> gimmickType;
    public List<BlockObject> blocks = new List<BlockObject>();
    public List<Vector2> blockOffsets = new List<Vector2>();
    public bool Enabled = true;
    
    private Vector2 centerPos;
    private Camera mainCamera;
    private Rigidbody rb;
    private bool isDragging = false;
    private Vector3 offset;
    private float zDistanceToCamera;
    private float maxSpeed = 20f;
    private Outline outline;
    private Vector2 previousXY;

    // 충돌 감지 변수
    public Collider col { get; set; }
    private bool isColliding = false;
    private Vector3 lastCollisionNormal;
    private float collisionResetTime = 0.1f; // 충돌 상태 자동 해제 시간
    private float lastCollisionTime;  
    private float moveSpeed = 25f;           
    private float followSpeed = 30f;        

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // 충돌 감지 모드 향상
        
        outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 2f;
        outline.enabled = false;
    }

    void OnMouseDown()
    {
        if (!Enabled) return;
        
        isDragging = true;
        rb.isKinematic = false;
        outline.enabled = true;
        
        // 카메라와의 z축 거리 계산
        zDistanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
        
        // 마우스와 오브젝트 간의 오프셋 저장
        offset = transform.position - GetMouseWorldPosition();
        
        // 충돌 상태 초기화
        ResetCollisionState();
    }

    void OnMouseUp()
    {
        isDragging = false;
        outline.enabled = false;
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        SetBlockPosition();
        ResetCollisionState();
    }

    void Update()
    {
        // 충돌 상태 자동 해제 검사
        if (isColliding && Time.time - lastCollisionTime > collisionResetTime)
        {
            // 일정 시간 동안 충돌 갱신이 없으면 충돌 상태 해제
            ResetCollisionState();
        }
    }

    void FixedUpdate()
    {
        if (!Enabled || !isDragging) return;
        
        SetBlockPosition(false);
        
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 targetPosition = mouseWorldPos + offset;
        Vector3 moveVector = targetPosition - transform.position;
    
        // 충돌 상태에서 마우스가 충분히 멀어지면 충돌 상태 해제
        float distanceToMouse = Vector3.Distance(transform.position, targetPosition);
        if (isColliding && distanceToMouse > 0.5f)
        {
            if (Vector3.Dot(moveVector.normalized, lastCollisionNormal) > 0.1f)
            {
                ResetCollisionState();
            }
        }

        // 속도 계산 개선
        Vector3 velocity;
        if (isColliding)
        {
            // 충돌면에 대해 속도 투영 (실제 이동)
            Vector3 projectedMove = Vector3.ProjectOnPlane(moveVector, lastCollisionNormal);
            
            velocity = projectedMove * moveSpeed;
        }
        else
        {
            velocity = moveVector * followSpeed;
        }
    
        // 속도 제한
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }
        
        if(!rb.isKinematic) rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, velocity, Time.fixedDeltaTime * 10f);
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = zDistanceToCamera;
        return mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    }
    
    private void SetBlockPosition(bool mouseUp = true)
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 coordinate = hit.transform.position;
            
            Vector3 targetPos = new Vector3(coordinate.x, transform.position.y, coordinate.z);
            if(mouseUp) transform.position = targetPos;
            
            centerPos.x = Mathf.Round(transform.position.x / 0.79f);
            centerPos.y = Mathf.Round(transform.position.z / 0.79f);
            
            if (hit.collider.TryGetComponent(out BoardBlockObject boardBlockObject))
            {
                foreach (var blockObject in blocks)
                {
                    blockObject.SetCoordinate(centerPos);
                }
                foreach (var blockObject in blocks)
                {
                    boardBlockObject.CheckAdjacentBlock(blockObject, targetPos);
                    blockObject.CheckBelowBoardBlock(targetPos);
                }
            }
        }
        else
        {
            Debug.LogWarning("Nothing Detected");
        }
    }
    
    public void ReleaseInput()
    {
        if (col != null) col.enabled = false;
        isDragging = false;
        outline.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }
    
    // 충돌 상태 초기화
    private void ResetCollisionState()
    {
        isColliding = false;
        lastCollisionNormal = Vector3.zero;
    }
    
    // 충돌 감지
    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }
    
    void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);
    }
    
    private void HandleCollision(Collision collision)
    {
        if (!isDragging) return;
        
        if (collision.contactCount > 0 && collision.gameObject.layer != LayerMask.NameToLayer("Board"))
        {
            Vector3 normal = collision.contacts[0].normal;
            
            // 수직 충돌(바닥과의 충돌)은 무시
            if (Vector3.Dot(normal, Vector3.up) < 0.8f)
            {
                isColliding = true;
                lastCollisionNormal = normal;
                lastCollisionTime = Time.time; // 충돌 시간 갱신
            }
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        // 현재 충돌 중인 오브젝트가 떨어질 때만 충돌 상태 해제
        if (collision.contactCount > 0)
        {
            Vector3 normal = collision.contacts[0].normal;
            
            // 현재 저장된 충돌 normal과 유사한 경우에만 해제
            if (Vector3.Dot(normal, lastCollisionNormal) > 0.8f)
            {
                ResetCollisionState();
            }
        }
    }

    public Vector3 GetCenterX()
    {
        if (blocks == null || blocks.Count == 0)
        {
            return Vector3.zero; // Return default value if list is empty
        }

        float minX = float.MaxValue;
        float maxX = float.MinValue;

        foreach (var block in blocks)
        {
            float blockX = block.transform.position.x;
        
            if (blockX < minX)
            {
                minX = blockX;
            }
        
            if (blockX > maxX)
            {
                maxX = blockX;
            }
        }
    
        // Calculate the middle value between min and max
        return new Vector3((minX + maxX) / 2f, transform.position.y, 0);
    }

    public Vector3 GetCenterZ()
    {
        if (blocks == null || blocks.Count == 0)
        {
            return Vector3.zero; // Return default value if list is empty
        }

        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        foreach (var block in blocks)
        {
            float blockZ = block.transform.position.z;
        
            if (blockZ < minZ)
            {
                minZ = blockZ;
            }
        
            if (blockZ > maxZ)
            {
                maxZ = blockZ;
            }
        }
    
        return new Vector3(transform.position.x, transform.position.y, (minZ + maxZ) / 2f);
    }

    private void ClearPreboardBlockObjects()
    {
        foreach (var b in blocks)
        {
            if (b.preBoardBlockObject != null)
            {
                b.preBoardBlockObject.playingBlock = null;
            }
        }
    }
    public void DestroyMove(Vector3 pos, ParticleSystem particle)
    {
        ClearPreboardBlockObjects();
        
        transform.DOMove(pos, 1f).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Destroy(particle.gameObject);
                Destroy(gameObject);
                //block.GetComponent<BlockShatter>().Shatter();
            });
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