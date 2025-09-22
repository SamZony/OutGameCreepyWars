using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System;

public enum InHand
{
    None,
    Weapon,
    Grenade,
    OtherThrowableObject,

}
public class OutPlayerWeaponManager : MonoBehaviour
{
    private Animator animator;
    public OutHolsterManager holster;

    public GameObject currentWeapon;
    private Vector3 weaponScaleInHolster;

    private Coroutine equipRoutine;
    private Coroutine holsterRoutine;

    bool setLeftHandConstraint;


    [Header("References")]
    public Transform rightHandGrip;
    public Component leftHandConstraint;

    public InHand inHand;
    private PlaceInHolster equippedFromHolsterSlot;

    public static Action<Transform> OnWeaponChanged;


    #region InputSystem
    private InputSystem_Actions actions;
    private InputAction takeWeapon1, takeWeapon2, takeWeapon5, holsterWeapon;
    #endregion

    private void Awake()
    {
        actions = new InputSystem_Actions();

        takeWeapon1 = actions.Player.TakeWeapon1;
        takeWeapon2 = actions.Player.TakeWeapon2;
        takeWeapon5 = actions.Player.TakeWeapon5;
        holsterWeapon = actions.Player.HolsterWeapon;
    }

    private void OnEnable()
    {
        takeWeapon1.Enable();
        takeWeapon2.Enable();
        takeWeapon5.Enable();
        holsterWeapon.Enable();

        takeWeapon1.performed += _ => TryEquip(PlaceInHolster.Primary);
        takeWeapon2.performed += _ => TryEquip(PlaceInHolster.Secondary);
        takeWeapon5.performed += _ => TryEquip(PlaceInHolster.BigGun);
        holsterWeapon.performed += _ => Holster();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        setLeftHandConstraint = GetComponent<OutShooterController>().setLeftHandConstraint;
    }

    private void OnDisable()
    {
        takeWeapon1.Disable();
        takeWeapon2.Disable();
        holsterWeapon.Disable();
    }

    private void OnDestroy()
    {
        takeWeapon1.Disable();
        takeWeapon2.Disable();
        holsterWeapon.Disable();
    }

    void TryEquip(PlaceInHolster slot)
    {
        if (inHand == InHand.Weapon)
        {
            equippedFromHolsterSlot = slot;
            Holster();
            Invoke(nameof(Equip), 2f);
        }
        else Equip(slot);
    }

    void Equip(PlaceInHolster slot)
    {
        if (slot == PlaceInHolster.None)
        {
            slot = equippedFromHolsterSlot;
        }
        var item = OutHolsterManager.Instance1.weapons
            .FirstOrDefault(w => w.placeInHolster == slot);

        if (currentWeapon != null) return;

        StopCoroutine(ref equipRoutine);
        StopCoroutine(ref holsterRoutine);

        currentWeapon = item.weapon;
        inHand = InHand.Weapon;
        OutGameManager.Instance.currentWeapon = currentWeapon;

        animator.SetLayerWeight(1, 1);
        animator.SetBool("GunMode", true);
        animator.SetTrigger("EquipBigGun");

        equipRoutine = StartCoroutine(EquipRoutine());
    }

    IEnumerator EquipRoutine()
    {
        weaponScaleInHolster = currentWeapon.transform.localScale;
        OutWeapon currentWeaponScript = currentWeapon.GetComponent<OutWeapon>();
        yield return new WaitForSeconds(0.8f);

        if (currentWeapon != null)
        {
            OnWeaponChanged?.Invoke(currentWeapon.transform);
            Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            currentWeapon.transform.SetParent(hand, false);
            switch (leftHandConstraint)
            {
                case MultiParentConstraint multiParent:
                    StartCoroutine(InterpolateFloat(0, 1, 0.2f, v => multiParent.weight = v));
                    break;
                case TwoBoneIKConstraint twoBone:
                    StartCoroutine(InterpolateFloat(0, 1, 0.2f, v => twoBone.weight = v));
                    break;
            }
            ;
            currentWeapon.transform.SetLocalPositionAndRotation(currentWeaponScript.handlingPoint.localPosition, currentWeaponScript.handlingPoint.localRotation);
            currentWeapon.transform.localScale = currentWeaponScript.scaleInHand;
        }

        yield return new WaitForSeconds(0.6f);

        currentWeapon.transform.SetLocalPositionAndRotation(currentWeaponScript.handlingPoint.localPosition, currentWeaponScript.handlingPoint.localRotation);
        animator.SetLayerWeight(1, 0);

        StartCoroutine(InterpolateFloat(0, 1, 0.2f, (v) =>
        {
            animator.SetLayerWeight(2, v);
            SetLeftHandConstraint(v);
        }));

    }

    public void Holster()
    {
        if (currentWeapon == null) return;

        StopCoroutine(ref equipRoutine);
        StopCoroutine(ref holsterRoutine);

        if (currentWeapon != null)
        {
            Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            currentWeapon.transform.SetParent(hand, false);
            switch (leftHandConstraint)
            {
                case MultiParentConstraint multiParent:
                    StartCoroutine(InterpolateFloat(1, 0, 0.2f, v => multiParent.weight = v));
                    break;
                case TwoBoneIKConstraint twoBone:
                    StartCoroutine(InterpolateFloat(1, 0, 0.2f, v => twoBone.weight = v));
                    break;
            }
            ;
            currentWeapon.transform.SetLocalPositionAndRotation(rightHandGrip.localPosition, rightHandGrip.localRotation);
            currentWeapon.transform.localScale = currentWeapon.GetComponent<OutWeapon>().scaleInHand;
        }


        animator.SetLayerWeight(2, 0);
        animator.SetLayerWeight(1, 1);
        animator.SetTrigger("HolsterBigGun");
        SetLeftHandConstraint(0, currentWeapon.GetComponent<OutWeapon>().leftHandGrip);

        holsterRoutine = StartCoroutine(HolsterRoutine());
    }

    IEnumerator HolsterRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        if (currentWeapon != null)
        {
            currentWeapon.transform.SetParent(OutHolsterManager.Instance1.transform, false);

            OutWeapon weaponScript = currentWeapon.GetComponent<OutWeapon>();
            bool isPlayer1 = OutGameManager.Instance.currentPlayer == CurrentPlayer.Player1;

            Transform holsterPlace = null;

            if (weaponScript.placeInHolster == PlaceInHolster.Primary)
            {
                holsterPlace = isPlayer1 ? OutHolsterManager.Instance1.primaryWeaponPlace
                                         : OutHolsterManager.Instance2.primaryWeaponPlace;
            }
            else if (weaponScript.placeInHolster == PlaceInHolster.Secondary)
            {
                holsterPlace = isPlayer1 ? OutHolsterManager.Instance1.secondaryWeaponPlace
                                         : OutHolsterManager.Instance2.secondaryWeaponPlace;
            }

            if (holsterPlace != null)
            {
                currentWeapon.transform.SetLocalPositionAndRotation(
                    holsterPlace.localPosition, holsterPlace.localRotation);
            }

            currentWeapon.transform.localScale = weaponScaleInHolster;
        }

        yield return new WaitForSeconds(0.6f);

        currentWeapon = null;
        inHand = InHand.None;
        OutGameManager.Instance.currentWeapon = null;
        animator.SetLayerWeight(1, 0);
        animator.SetLayerWeight(2, 0);
        animator.SetBool("GunMode", false);
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

    public void SetLeftHandConstraint(float weight, Transform target)
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
            var grip = GetComponent<LeftHandIKGrip>();
            grip.leftHandGrip = target;
            grip.positionWeight = weight;
            grip.rotationWeight = weight;
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

    void StopCoroutine(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
}
