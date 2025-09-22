using UnityEngine;

public class LeftHandIKGrip : MonoBehaviour
{
    [Tooltip("Assign the Animator component (usually on the character root).")]
    public Animator animator;

    [Tooltip("The transform of the weapon's left-hand grip point.")]
    public Transform leftHandGrip;

    [Range(0f, 1f)]
    public float positionWeight = 1f;

    [Range(0f, 1f)]
    public float rotationWeight = 1f;

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || leftHandGrip == null)
            return;

        // Tell the animator we want to override IK for the left hand
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, positionWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, rotationWeight);

        // Assign the grip point as IK target
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandGrip.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandGrip.rotation);
    }
}
