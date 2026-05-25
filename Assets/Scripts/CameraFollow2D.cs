using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] [Min(0f)] private float smoothSpeed = 0.2f;

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothSpeed
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
