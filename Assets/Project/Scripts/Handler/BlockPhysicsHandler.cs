using UnityEngine;

public class BlockPhysicsHandler : MonoBehaviour
{
    private Rigidbody rb;
    private bool isDragging = false;

    public void Initialize()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void SetDragging(bool inIsDragging)
    {
        isDragging = inIsDragging;
        //reset velocity
        if (!isDragging) rb.linearVelocity = Vector3.zero;
        //set kinematic
        rb.isKinematic = !inIsDragging;
    }

    public void SetPointerOffset(Vector3 pointerOffset)
    {
        rb.AddForce(pointerOffset * 2, ForceMode.VelocityChange);
    }
}
