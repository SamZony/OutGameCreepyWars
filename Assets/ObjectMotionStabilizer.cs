using UnityEngine;

public class ObjectMotionStabilizer : MonoBehaviour
{
    private Vector3 defaultLocalPosition;
    private Quaternion defaultLocalRotation;

    [Header("Stabilization Settings")]
    public bool smooth = true;
    public float positionSmoothSpeed = 10f;
    public float rotationSmoothSpeed = 10f;

    void Start()
    {
        // Save the child's initial offset relative to parent
        defaultLocalPosition = transform.localPosition;
        defaultLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        // Target world pose based on original local pose
        Vector3 targetWorldPosition = transform.parent.TransformPoint(defaultLocalPosition);
        Quaternion targetWorldRotation = transform.parent.rotation * defaultLocalRotation;

        if (smooth)
        {
            transform.position = Vector3.Lerp(transform.position, targetWorldPosition, Time.deltaTime * positionSmoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetWorldRotation, Time.deltaTime * rotationSmoothSpeed);
        }
        else
        {
            transform.position = targetWorldPosition;
            transform.rotation = targetWorldRotation;
        }
    }
}
