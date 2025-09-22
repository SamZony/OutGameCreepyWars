using DG.Tweening;
using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeleeController))]
public class OutShooterController : MonoBehaviour, IDamageable
{
    private Animator animator;
    public OutHolsterManager holster;
    private Rigidbody[] ragdollBodies;

    [Header("Camera")]
    public CinemachineCamera cineCam;
    private GameObject cameraMain;
    public Transform lookAtTarget;
    public Transform lookAtAim;

    [Header("Field Of View Settings")]
    public float aimFOV = 12f;
    public float normalFOV = 40f;

    public GameObject currentWeapon;

    [Header("Constraints")]
    public MultiAimConstraint upperBodyConstraint;
    public MultiAimConstraint rightHandConstraint;
    public Component leftHandConstraint;
    [Tooltip("Set Left Hand Constraint from Rig object. LeftHandIKGrip component will be needed if false.")]
    public bool setLeftHandConstraint = true;


    private InputSystem_Actions actions;
    private InputAction aim;

    [Header("Health")]
    public float Health { get; set; }
    public float maxHealth = 200f;

    [Header("References")]
    public Transform rightHandGrip;

    public Transform leftHandGrip;
    public Transform leftHandGrip_aiming;

    bool isDamagable = true;

    void Awake()
    {
        actions = new InputSystem_Actions();
        aim = actions.Player.Aim;

        ragdollBodies = GetComponentsInChildren<Rigidbody>();

        // Assume ragdoll is inactive at start (animated)
    }

    private void OnEnable()
    {
        aim.Enable();
        aim.performed += OnAimPerformed;
        aim.canceled += OnAimCanceled;

        OutPlayerWeaponManager.OnWeaponChanged += (weapon) =>
        {
            if (setLeftHandConstraint)
            {
                SetLeftHandConstraint(1);
                SetLeftHandTarget(weapon.GetComponent<OutWeapon>().leftHandGrip);
            }
            else
            {
                SetLeftHandConstraint(weapon.GetComponent<OutWeapon>().leftHandGrip, 1);
            }

        };
    }

    CinemachineRotationComposer cineCamRotation;
    OutCameraController outCamCtrl;


    void Start()
    {
        animator = GetComponent<Animator>();
        cineCamRotation = cineCam.GetComponent<CinemachineRotationComposer>();
        cameraMain = GameObject.FindWithTag("MainCamera");
        outCamCtrl = cameraMain.GetComponent<OutCameraController>();
        GetComponent<OutTPSystem>().SetRagdollState(animator.enabled);
        Health = maxHealth;

        Invoke(nameof(TryEnableHolster), 0.1f);
    }


    private void OnDisable()
    {
        aim.performed -= OnAimPerformed;
        aim.canceled -= OnAimCanceled;
        aim.Disable();
    }

    float angleBTWCamNP;
    void Update()
    {
        if (animator.GetBool("AimingRifle"))
        {
            angleBTWCamNP = Vector3.Angle(transform.forward, cameraMain.transform.forward);
            if (angleBTWCamNP > 87f)
            {
                animator.SetFloat("X_aim", 1);
            }
            else
                animator.SetFloat("X_aim", 0);
        }
    }

    private void OnAimPerformed(InputAction.CallbackContext ctx)
    {
        if (animator.GetBool("GunMode"))
        {
            animator.SetBool("AimingRifle", ctx.performed && animator.GetBool("GunMode"));
            TweenAimState(true, cineCam, rightHandConstraint, cineCamRotation, outCamCtrl.aimScreenPos);

            cineCam.Lens.FieldOfView = 20f;
            cineCam.LookAt = lookAtAim;
            ShooterUI.Instance.reticle.SetActive(true);
        }
    }

    public void TweenAimState(bool aiming, CinemachineCamera cineCam,
    MultiAimConstraint rightHandConstraint,
    CinemachineRotationComposer cineCamRotation,
    float aimScreenPos, float duration = 0.2f)
    {
        if (cineCam == null)
        {
            Debug.Log("cineCam is null");
        }
        if (cineCamRotation == null) Debug.Log("cineCamRotation is null");
        // Kill any existing tweens on these values
        DOTween.Kill(cineCam);
        DOTween.Kill(rightHandConstraint);
        DOTween.Kill(cineCamRotation);

        // Build a sequence to run these tweens simultaneously
        DG.Tweening.Sequence seq = DOTween.Sequence();

        float targetFOV = aiming ? aimFOV : normalFOV;
        float targetWeight = aiming ? 1f : 0f;
        float targetOffsetX = aiming ? aimScreenPos : 0f;

        seq.Join(DOTween.To(() => cineCam.Lens.FieldOfView,
            x => cineCam.Lens.FieldOfView = x,
            targetFOV, duration
            ));
        seq.Join(DOTween.To(
            () => rightHandConstraint.weight,
            x => rightHandConstraint.weight = x,
            targetWeight,
            duration
        ));
        seq.Join(DOTween.To(
            () => cineCamRotation.TargetOffset.x,
            v =>
            {
                var t = cineCamRotation.TargetOffset;
                t.x = v;
                cineCamRotation.TargetOffset = t;
            },
            targetOffsetX,
            duration
        ));
        // Append this one AFTER the others are done
        seq.AppendInterval(0.4f);
        seq.Append(DOTween.To(
            () => upperBodyConstraint.weight,
            w => upperBodyConstraint.weight = w,
            aiming ? 1f : 0f,
            duration * 0.8f // slightly faster/smoother finish
        ));

        seq.SetEase(Ease.InOutQuad);
        seq.Play();
    }

    private void OnAimCanceled(InputAction.CallbackContext ctx)
    {
        if (animator.GetBool("GunMode"))
        {
            // Optionally reset aiming on release
            animator.SetBool("AimingRifle", false);
            TweenAimState(false, cineCam, rightHandConstraint, cineCamRotation, outCamCtrl.aimScreenPos);
            cineCam.LookAt = lookAtTarget;
            ShooterUI.Instance.reticle.SetActive(false);
            upperBodyConstraint.weight = 0;
        }
    }

    void TryEnableHolster()
    {
        if (holster != null)
        {
            holster.enabled = true;
        }
        else
            Debug.LogError("Holster Manager not found in the holster field of TPShooterPlayer1");

        if (holster.enabled == false) Invoke(nameof(TryEnableHolster), 0.1f);
    }

    // Helper to safely stop coroutines
    void StopCoroutine(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }


    IEnumerator InterpolateFloat(float startValue, float endValue, float duration, System.Action<float> onUpdate)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentValue = Mathf.Lerp(startValue, endValue, t);
            onUpdate?.Invoke(currentValue);
            yield return null;
        }
        // Ensure final value is exact
        onUpdate?.Invoke(endValue);
    }

    public void TakeDamage(float amount)
    {
        if (isDamagable)
        {
            Health -= amount;
            float fillAmount = Health / maxHealth;
            ShooterUI.Instance.healthFill.fillAmount = fillAmount;


            if (Health <= 0)
            {
                animator.enabled = false;
                GetComponent<OutTPSystem>().enabled = false;
                TryGetComponent<FeetGrounder>(out FeetGrounder feetGrounder);
                if (feetGrounder) feetGrounder.enabled = false;
                upperBodyConstraint.weight = 0f;
                rightHandConstraint.weight = 0f;
                if (currentWeapon != null)
                {
                    Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                    currentWeapon.transform.SetParent(hand, false);
                    SetLeftHandConstraint(0);
                    currentWeapon.transform.SetLocalPositionAndRotation(rightHandGrip.localPosition, rightHandGrip.localRotation);
                    currentWeapon.transform.localScale = currentWeapon.GetComponent<OutWeapon>().scaleInHand;
                }
                GetComponent<OutTPSystem>().SetRagdollState(animator.enabled);

                OutGameManager.Instance.LevelFailed("Sylph Died!");
                isDamagable = false;
                GetComponent<OutShooterController>().enabled = false;
            }
            else ShooterUI.Instance.damagePanel.DORestart();
        }

    }

    void SetLeftHandConstraint(float weight)
    {
        if (setLeftHandConstraint)
        {
            if (leftHandConstraint.TryGetComponent<MultiParentConstraint>(out MultiParentConstraint mp))
            {
                mp.weight = weight;
            }
            else if (leftHandConstraint.TryGetComponent<TwoBoneIKConstraint>(out TwoBoneIKConstraint tb))
            {
                tb.weight = weight;
            }
            else
            {
                Debug.LogError("Unsupported constraint type for left hand. It should be MultiParent or TwoBoneIK");
            }
        }
        else
        {
            GetComponent<LeftHandIKGrip>().leftHandGrip = currentWeapon.GetComponent<OutWeapon>().leftHandGrip;
            GetComponent<LeftHandIKGrip>().positionWeight = weight;
            GetComponent<LeftHandIKGrip>().rotationWeight = weight;
        }
    }

    public void SetLeftHandConstraint(Transform target, float weight)
    {
        if (setLeftHandConstraint)
        {
            if (leftHandConstraint.TryGetComponent<MultiParentConstraint>(out MultiParentConstraint mp))
            {
                // Assign weight
                mp.weight = weight;

                // Assign target
                var sources = mp.data.sourceObjects;
                if (sources.Count == 0)
                {
                    sources.Add(new WeightedTransform(target, 1f));
                }
                else
                {
                    sources.SetTransform(0, target);
                    sources.SetWeight(0, 1f);
                }
                mp.data.sourceObjects = sources;
            }
            else if (leftHandConstraint.TryGetComponent<TwoBoneIKConstraint>(out TwoBoneIKConstraint tb))
            {
                // Assign weight
                tb.weight = weight;

                // Assign target
                tb.data.target = target;
            }
            else
            {
                Debug.LogError("Unsupported constraint type for left hand. It should be MultiParent or TwoBoneIK");
            }
        }
        else
        {
            // Fallback: use Animator IK (your LeftHandIKGrip script)
            TryGetComponent<LeftHandIKGrip>(out LeftHandIKGrip grip);
            grip.leftHandGrip = target;
            grip.positionWeight = weight;
            grip.rotationWeight = weight;
        }
    }


    public void SetLeftHandTarget(Transform target)
    {
        if (leftHandConstraint.TryGetComponent<MultiParentConstraint>(out MultiParentConstraint mp))
        {
            // MultiParentConstraint uses a WeightedTransformArray
            var sources = mp.data.sourceObjects;
            if (sources.Count == 0)
            {
                sources.Add(new WeightedTransform(target, 1f));
            }
            else
            {
                sources.SetTransform(0, target);
                sources.SetWeight(0, 1f);
            }
            mp.data.sourceObjects = sources;
        }
        else if (leftHandConstraint.TryGetComponent<TwoBoneIKConstraint>(out TwoBoneIKConstraint tb))
        {
            // TwoBoneIKConstraint has a dedicated target field
            tb.data.target = target;
        }
        else
        {
            Debug.LogError("Unsupported constraint type on left hand. It should be MultiParent or TwoBoneIK");
        }
    }


    //methods in this indent
}
//class ends with above bracket
