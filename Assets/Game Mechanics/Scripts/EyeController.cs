using UnityEngine;
using System.Collections.Generic;

public class EyeController : MonoBehaviour
{
    [Header("Eye Transforms")]
    public Transform leftEye;
    public Transform rightEye;

    [Header("Eye Movement")]
    public float lookSpeed = 5f;
    public float maxRotationAngle = 30f;

    [Header("Wander")]
    public bool allowIdleWander = true;
    public float wanderSpeed = 1f;
    public float wanderAmount = 0.1f;

    [Header("Auto Target Switching")]
    public bool useConeTargeting = true;
    public string[] targetTags = { "Enemy", "Companion", "Object" };
    public float viewAngle = 60f;
    public float viewDistance = 10f;
    public float targetSwitchInterval = 3f;

    [Header("Debug")]
    public bool visualizeCone = true;

    private Quaternion leftEyeInitialRotation;
    private Quaternion rightEyeInitialRotation;

    private Transform currentTarget;
    private float targetSwitchTimer;
    private Vector3 wanderOffset;

    void Start()
    {
        leftEyeInitialRotation = leftEye.localRotation;
        rightEyeInitialRotation = rightEye.localRotation;

        wanderOffset = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0f
        );
    }

    void Update()
    {
        if (useConeTargeting)
        {
            targetSwitchTimer -= Time.deltaTime;
            if (targetSwitchTimer <= 0f)
            {
                currentTarget = FindTargetInViewCone();
                targetSwitchTimer = targetSwitchInterval;
            }
        }

        if (currentTarget != null)
        {
            LookAtTarget(currentTarget);
        }
        else if (allowIdleWander)
        {
            ApplyIdleWander();
        }
    }

    void LookAtTarget(Transform target)
    {
        Vector3 direction = target.position - leftEye.position;

        Quaternion desiredLeftRot = Quaternion.LookRotation(direction, transform.up);
        Quaternion desiredRightRot = Quaternion.LookRotation(direction, transform.up);

        desiredLeftRot = ClampRotation(desiredLeftRot, leftEyeInitialRotation);
        desiredRightRot = ClampRotation(desiredRightRot, rightEyeInitialRotation);

        leftEye.rotation = Quaternion.Slerp(leftEye.rotation, desiredLeftRot, Time.deltaTime * lookSpeed);
        rightEye.rotation = Quaternion.Slerp(rightEye.rotation, desiredRightRot, Time.deltaTime * lookSpeed);
    }

    void ApplyIdleWander()
    {
        float offsetX = Mathf.Sin(Time.time * wanderSpeed) * wanderAmount;
        float offsetY = Mathf.Cos(Time.time * wanderSpeed * 0.8f) * wanderAmount;

        Quaternion idleRot = Quaternion.Euler(offsetY, offsetX, 0f);

        leftEye.localRotation = Quaternion.Slerp(leftEye.localRotation, leftEyeInitialRotation * idleRot, Time.deltaTime * lookSpeed);
        rightEye.localRotation = Quaternion.Slerp(rightEye.localRotation, rightEyeInitialRotation * idleRot, Time.deltaTime * lookSpeed);
    }

    Quaternion ClampRotation(Quaternion targetRotation, Quaternion initial)
    {
        Quaternion relative = Quaternion.Inverse(initial) * targetRotation;
        relative.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > maxRotationAngle)
            relative = Quaternion.AngleAxis(maxRotationAngle, axis);

        return initial * relative;
    }

    Transform FindTargetInViewCone()
    {
        List<Transform> visibleTargets = new List<Transform>();

        foreach (string tag in targetTags)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objs)
            {
                Vector3 dirToTarget = obj.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, dirToTarget);

                if (angle < viewAngle / 2f && dirToTarget.magnitude <= viewDistance)
                {
                    Ray ray = new Ray(transform.position, dirToTarget.normalized);
                    if (Physics.Raycast(ray, out RaycastHit hit, viewDistance))
                    {
                        if (hit.transform == obj.transform)
                        {
                            visibleTargets.Add(obj.transform);
                        }
                    }
                }
            }
        }

        if (visibleTargets.Count > 0)
        {
            return visibleTargets[Random.Range(0, visibleTargets.Count)];
        }

        return null;
    }

    void OnDrawGizmosSelected()
    {
        if (!visualizeCone) return;

        Gizmos.color = Color.cyan;
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        // Cone edges
        Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle / 2, transform.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle / 2, transform.up);

        Vector3 leftRay = leftRayRotation * forward * viewDistance;
        Vector3 rightRay = rightRayRotation * forward * viewDistance;

        Gizmos.DrawRay(origin, leftRay);
        Gizmos.DrawRay(origin, rightRay);

        Gizmos.DrawWireSphere(origin, viewDistance);
    }
}
