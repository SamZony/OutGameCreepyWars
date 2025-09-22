using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class CCTVCamera : MonoBehaviour
{
    [Header("Camera Holder")]
    public Transform cameraHolder;
    public GameObject cameraBase;

    [Header("Vision Direction")]
    public Transform visionReference;

    [Header("Explosion Particle")]
    public ParticleSystem explosionParticle;


    [Header("Rotation Settings")]
    public bool rotateOnX = false;
    public bool rotateOnY = true;
    public float rotationAngle = 45f;
    public float rotationSpeed = 30f;

    [Header("View Settings")]
    public bool showViewCone = true;
    public float viewDistance = 15f;
    [Range(0, 180)] public float viewAngle = 45f;

    [Header("Detection Settings")]
    public string[] targetTags = new string[] { "Player", "Companion" };
    public float alertDuration = 5f;
    public LayerMask detectionMask; // Add this in inspector to ignore ground, walls, etc.

    [Header("On Detection Events")]
    public UnityEvent OnTargetDetected;

    private float currentRotation = 0f;
    private bool rotatingForward = true;

    private bool targetDetected = false;
    private float detectionResetTime = 0f;



    private void Update()
    {
        if (cameraHolder == null) return;

        RotateCamera();

        if (targetDetected && Time.time < detectionResetTime) return;

        foreach (string tag in targetTags)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject target in targets)
            {
                if (IsInView(target.transform))
                {
                    TriggerAlert(target.transform.position);
                    targetDetected = true;
                    detectionResetTime = Time.time + alertDuration;
                    return;
                }
            }
        }

        // Reset detection after cooldown
        if (targetDetected && Time.time >= detectionResetTime)
            targetDetected = false;
    }

    void RotateCamera()
    {
        float direction = rotatingForward ? 1f : -1f;
        float angleStep = rotationSpeed * Time.deltaTime * direction;

        if (rotateOnY)
        {
            cameraHolder.Rotate(0f, angleStep, 0f);
            currentRotation += angleStep;
        }
        else if (rotateOnX)
        {
            cameraHolder.Rotate(angleStep, 0f, 0f);
            currentRotation += angleStep;
        }

        if (Mathf.Abs(currentRotation) >= rotationAngle)
        {
            rotatingForward = !rotatingForward;
            currentRotation = Mathf.Clamp(currentRotation, -rotationAngle, rotationAngle);
        }
    }

    bool IsInView(Transform target)
    {
        if (visionReference == null) return false;

        Vector3 dirToTarget = target.position - visionReference.position;
        float distance = dirToTarget.magnitude;
        if (distance > viewDistance) return false;

        Vector3 viewDirection = rotateOnY ? visionReference.forward : visionReference.right;
        float angleToTarget = Vector3.Angle(viewDirection, dirToTarget.normalized);

        if (angleToTarget < viewAngle / 2f)
        {
            Ray ray = new Ray(visionReference.position, dirToTarget.normalized);
            if (Physics.Raycast(ray, out RaycastHit hit, viewDistance, detectionMask))
            {
                // Check if hit is the player or inside player's hierarchy
                return hit.transform == target || hit.transform.IsChildOf(target);
            }
        }

        return false;
    }


    void TriggerAlert(Vector3 position)
    {
        GameObject ping = new GameObject("AlertPing");
        ping.transform.position = position;

        NavMeshAgent[] enemies = FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
        foreach (var agent in enemies)
        {
            if (agent.CompareTag("Enemy"))
            {
                agent.SetDestination(ping.transform.position);
            }
        }

        OnTargetDetected?.Invoke();
        StartCoroutine(DestroyAfterTime(ping, alertDuration));
    }

    IEnumerator DestroyAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        if (obj != null) Destroy(obj);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (cameraHolder == null) return;

        if (collision.gameObject == cameraBase) return;

        if (cameraBase.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = cameraBase.gameObject.AddComponent<Rigidbody>();
            rb.mass = 5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        if (explosionParticle != null)
        {
            explosionParticle.gameObject.SetActive(true);
            explosionParticle.Play();
        }

        enabled = false;

    }


    private void OnDrawGizmosSelected()
    {
        if (!showViewCone || visionReference == null) return;

        Vector3 origin = visionReference.position;
        Vector3 forward = rotateOnY ? visionReference.forward : visionReference.right;
        Vector3 up = visionReference.up;

        Quaternion baseRotation = Quaternion.LookRotation(forward, up);

        int segments = 30;
        float angleStep = viewAngle / segments;

        Vector3 prevPoint = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float angle = -viewAngle / 2f + i * angleStep;
            Quaternion rotation = baseRotation * Quaternion.Euler(0, angle, 0);
            Vector3 direction = rotation * Vector3.forward;
            Vector3 point = origin + direction * viewDistance;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(origin, direction * viewDistance);

            if (i > 0)
            {
                Gizmos.DrawLine(prevPoint, point);
            }
            prevPoint = point;
        }

        Gizmos.DrawWireSphere(origin, viewDistance);

#if UNITY_EDITOR
        Handles.color = new Color(1f, 0f, 0f, 0.15f);
        Handles.DrawSolidArc(origin, up,
            baseRotation * Quaternion.Euler(0, -viewAngle / 2f, 0) * Vector3.forward,
            viewAngle, viewDistance);
#endif
    }

}
