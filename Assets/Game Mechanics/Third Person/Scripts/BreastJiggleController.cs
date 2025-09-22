using UnityEngine;

public class BreastJiggleController : MonoBehaviour
{
    public Animator animator;
    [Header("Breast Bones")]
    public Transform leftBreastBone;
    public Transform rightBreastBone;

    [Header("Jiggle Settings")]
    [Tooltip("Vertical jiggle amplitude multiplier")]
    public float intensity = 0.1f;
    [Tooltip("Spring stiffness (higher = snappier)")]
    public float stiffness = 10f;
    [Tooltip("Damping (higher = less oscillation)")]
    public float damping = 5f;

    [Header("Activation")]
    [Tooltip("Name of Animator bool that enables jiggle when false")]
    public string idleBool = "Idle";

    Vector3 leftVelocity, rightVelocity;
    Vector3 leftRest, rightRest;

    void Start()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (leftBreastBone) leftRest = leftBreastBone.localPosition;
        if (rightBreastBone) rightRest = rightBreastBone.localPosition;
    }

    void LateUpdate()
    {
        bool idle = animator.GetBool(idleBool);

        if (!idle)
        {
            SpringBone(leftBreastBone, ref leftVelocity, leftRest);
            SpringBone(rightBreastBone, ref rightVelocity, rightRest);
        }
        else
        {
            RelaxBone(leftBreastBone, ref leftVelocity, leftRest);
            RelaxBone(rightBreastBone, ref rightVelocity, rightRest);
        }
    }

    void SpringBone(Transform bone, ref Vector3 velocity, Vector3 rest)
    {
        if (bone == null) return;
        var local = bone.localPosition;
        Vector3 force = (rest - local) * stiffness - velocity * damping;
        velocity += force * Time.deltaTime;
        // direction in local space upward
        Vector3 offset = bone.localRotation * Vector3.up * (velocity.y * intensity);
        bone.localPosition = rest + offset;
    }

    void RelaxBone(Transform bone, ref Vector3 velocity, Vector3 rest)
    {
        if (bone == null) return;
        bone.localPosition = Vector3.Lerp(bone.localPosition, rest, Time.deltaTime * 5f);
        velocity = Vector3.zero;
    }
}
