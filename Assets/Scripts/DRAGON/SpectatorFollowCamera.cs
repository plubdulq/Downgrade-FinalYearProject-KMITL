using UnityEngine;

public class SpectatorFollowCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Offset (relative to target)")]
    [SerializeField] private Vector3 localOffset = new Vector3(0.45f, 0.15f, -1.8f);

    [Header("Smoothing")]
    [SerializeField] private float positionSmooth = 5f;
    [SerializeField] private float rotationSmooth = 5f;

    [Header("Look Settings")]
    [SerializeField] private bool lookAtTarget = true;
    [SerializeField] private Vector3 lookOffset = new Vector3(0f, 0f, 0f);

    [Header("Auto Find")]
    [SerializeField] private bool autoFindMainCameraIfMissing = true;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Awake()
    {
        if (target == null && autoFindMainCameraIfMissing && Camera.main != null)
        {
            target = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition =
            target.position +
            target.right * localOffset.x +
            Vector3.up * localOffset.y +
            target.forward * localOffset.z;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Time.deltaTime * positionSmooth
        );

        Quaternion desiredRotation;

        if (lookAtTarget)
        {
            Vector3 lookPoint = target.position + lookOffset;
            Vector3 direction = lookPoint - transform.position;

            if (direction.sqrMagnitude < 0.0001f) return;

            desiredRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
        else
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(target.forward, Vector3.up).normalized;
            if (flatForward.sqrMagnitude < 0.0001f)
            {
                flatForward = transform.forward;
            }

            desiredRotation = Quaternion.LookRotation(flatForward, Vector3.up);
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            Time.deltaTime * rotationSmooth
        );
    }
}