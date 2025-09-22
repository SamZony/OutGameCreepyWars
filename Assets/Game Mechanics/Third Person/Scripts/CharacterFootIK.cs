using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FeetGrounder : MonoBehaviour
{
    Animator _anim;

    [Header("Feet Grounder Settings")]
    [Tooltip("Enable or disable all foot IK logic")]
    public bool enableFeetIk = true;

    [Tooltip("Vertical offset applied above the foot before raycasting down")]
    [Range(0f, 2f)] public float heightFromGroundRaycast = 1.14f;

    [Tooltip("Maximum downward distance to cast when detecting ground")]
    [Range(0f, 2f)] public float raycastDownDistance = 1.5f;

    [Tooltip("Layers considered as ground detection")]
    public LayerMask environmentLayer;

    [Tooltip("Vertical offset added to IK foot position to prevent sinking")]
    public float pelvisOffset = 0f;

    [Tooltip("Speed at which pelvis moves up/down to follow foot positions")]
    [Range(0f, 1f)] public float pelvisUpAndDownSpeed = 0.28f;

    [Tooltip("Speed to blend foot animation FK position toward IK target")]
    [Range(0f, 1f)] public float feetToIkPositionSpeed = 0.5f;

    [Tooltip("Enable animation-curves to modulate IK weight per foot step")]
    public bool useProIkFeature = false;

    [Tooltip("Name of the float curve parameter for left foot planting")]
    public string leftFootAnimVariableName = "LeftFootCurve";

    [Tooltip("Name of the float curve parameter for right foot planting")]
    public string rightFootAnimVariableName = "RightFootCurve";

    [Tooltip("Show debug rays and lines in Scene view")]
    public bool showSolverDebug = true;

    Vector3 _leftFootPos, _rightFootPos;
    Vector3 _leftFootIkPos, _rightFootIkPos;
    Quaternion _leftFootIkRot, _rightFootIkRot;
    float _lastPelvisPositionY, _lastLeftFootY, _lastRightFootY;
    Quaternion _currentLeftFootIkRot, _currentRightFootIkRot;

    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (!enableFeetIk || !_anim) return;

        AdjustFeetTarget(ref _rightFootPos, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref _leftFootPos, HumanBodyBones.LeftFoot);

        FeetPositionSolver(_rightFootPos, ref _rightFootIkPos, ref _rightFootIkRot);
        FeetPositionSolver(_leftFootPos, ref _leftFootIkPos, ref _leftFootIkRot);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!enableFeetIk || !_anim) return;

        _currentRightFootIkRot = _anim.GetIKRotation(AvatarIKGoal.RightFoot);
        _currentLeftFootIkRot = _anim.GetIKRotation(AvatarIKGoal.LeftFoot);

        MovePelvisHeight();

        _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, useProIkFeature ? _anim.GetFloat(rightFootAnimVariableName) : 1f);
        MoveFeetToIkPoint(AvatarIKGoal.RightFoot, _rightFootIkPos, _rightFootIkRot, ref _lastRightFootY, _currentRightFootIkRot);

        _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, useProIkFeature ? _anim.GetFloat(leftFootAnimVariableName) : 1f);
        MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, _leftFootIkPos, _leftFootIkRot, ref _lastLeftFootY, _currentLeftFootIkRot);
    }

    void AdjustFeetTarget(ref Vector3 feetPos, HumanBodyBones foot)
    {
        feetPos = _anim.GetBoneTransform(foot).position;
        feetPos.y = transform.position.y + heightFromGroundRaycast;
    }

    void FeetPositionSolver(Vector3 fromPosition, ref Vector3 feetIkPos, ref Quaternion feetIkRot)
    {
        if (showSolverDebug)
        {
            Debug.DrawLine(fromPosition,
                fromPosition + Vector3.down * (heightFromGroundRaycast + raycastDownDistance),
                Color.yellow);
        }

        if (Physics.SphereCast(fromPosition, 0.1f, Vector3.down,
            out RaycastHit hit, heightFromGroundRaycast + raycastDownDistance, environmentLayer))
        {
            feetIkPos = fromPosition;
            feetIkPos.y = hit.point.y + pelvisOffset;

            Quaternion targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            feetIkRot = Quaternion.Slerp(feetIkRot, targetRot, Time.deltaTime * 8f);

            if (showSolverDebug)
            {
                Debug.DrawRay(hit.point, hit.normal, Color.magenta);
            }
        }
        else
        {
            feetIkPos = Vector3.zero;
        }
    }

    void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 ikPosition, Quaternion ikRotation,
        ref float lastFootY, Quaternion currentFootRot)
    {
        Vector3 targetIkPos = _anim.GetIKPosition(foot);

        if (ikPosition != Vector3.zero)
        {
            targetIkPos = transform.InverseTransformPoint(targetIkPos);
            Vector3 ikLocal = transform.InverseTransformPoint(ikPosition);

            float yOffset = Mathf.Lerp(lastFootY, ikLocal.y, feetToIkPositionSpeed);
            lastFootY = yOffset;

            targetIkPos.y += yOffset;
            targetIkPos = transform.TransformPoint(targetIkPos);

            Quaternion blendedRot = ikRotation * currentFootRot;
            _anim.SetIKRotation(foot, blendedRot);
        }

        _anim.SetIKPosition(foot, targetIkPos);
    }

    void MovePelvisHeight()
    {
        if (_leftFootIkPos == Vector3.zero || _rightFootIkPos == Vector3.zero) return;

        float lOffset = _leftFootIkPos.y - transform.position.y;
        float rOffset = _rightFootIkPos.y - transform.position.y;
        float totalOffset = Mathf.Min(lOffset, rOffset);

        Vector3 newPelvisPos = _anim.bodyPosition + Vector3.up * totalOffset;
        newPelvisPos.y = Mathf.Lerp(_lastPelvisPositionY, newPelvisPos.y, pelvisUpAndDownSpeed);

        _anim.bodyPosition = newPelvisPos;
        _lastPelvisPositionY = newPelvisPos.y;
    }
}
