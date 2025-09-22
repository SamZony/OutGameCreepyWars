using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class OutTPSystem : MonoBehaviour
{
    public Animator animator;
    public Transform cameraTransform;
    private Rigidbody rb;

    [Header("Speeds")]
    public float moveSpeed = 5f;
    public float rotationSmoothness = 5f;
    public float jumpHeight = 2f; // how high to go
    public float jumpDuration = 0.4f; // time to reach peak
    public float fallMultiplier = 2f; // gravity multiplier

    [Header("Configs")]
    public float downforce = 50f;
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayers;


    [Header("Rigidbody Movement Smoothing")]
    public float acceleration = 10f;  // how fast to ramp up to target speed
    public float deceleration = 10f;  // how fast to slow down


    [Header("Movement Mode")]
    public MovementMode movementMode = MovementMode.Rigidbody;
    public enum MovementMode { RootMotion, Rigidbody }

    InputSystem_Actions action;
    InputAction move, look, interact, jump, crouch;
    Coroutine jumpRoutine;

    [HideInInspector] public bool jumping;
    private Vector2 cachedInput = Vector2.zero;

    public static Action<ThrowableObject> OnObjectPicked;
    public static Action<ThrowableObject> OnObjectThrown;
    Coroutine pickingObject;
    Coroutine throwingObject;

    public GameObject objHanded;

    private void Awake()
    {
        action = new InputSystem_Actions();
        move = action.Player.Move;
        look = action.Player.Look;
        interact = action.Player.Interact;
        jump = action.Player.Jump;
        crouch = action.Player.Crouch;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        animator.applyRootMotion = movementMode == MovementMode.RootMotion;
    }

    private void OnEnable()
    {
        action.Player.Enable();
        move.performed += OnMove;
        move.canceled += OnMove;
        look.performed += OnLook;
        look.canceled += OnLook;
        action.Player.Sprint.performed += Sprint;
        action.Player.Sprint.canceled += Sprint;
        jump.performed += Jump;
        jump.canceled += Jump;
        crouch.performed += Crouch;
        crouch.canceled += Crouch;
        interact.performed += OnInteract;
        interact.canceled += OnInteract;

        OnObjectPicked += PickObject;
        OnObjectThrown += ThrowObject;
    }

    private void Crouch(InputAction.CallbackContext context)
    {
        bool status = context.performed;
        animator.SetBool("Crouched", status);
    }

    private void OnDisable()
    {
        move.performed -= OnMove;
        move.canceled -= OnMove;
        look.performed -= OnLook;
        look.canceled -= OnLook;
        action.Player.Sprint.performed -= Sprint;
        action.Player.Sprint.canceled -= Sprint;
        action.Player.Disable();
        jump.performed -= Jump;
        jump.canceled -= Jump;
        crouch.performed -= Crouch;
        crouch.canceled -= Crouch;
        interact.performed -= OnInteract;
        interact.canceled -= OnInteract;

        OnObjectPicked -= PickObject;
        OnObjectThrown -= ThrowObject;
    }

    // ================= INTERACT =================
    private void OnInteract(InputAction.CallbackContext context)
    {
        if (OutGameManager.Instance.currentPickableObject != null)
            OnObjectPicked?.Invoke(OutGameManager.Instance.currentPickableObject);

        if (objHanded != null)
            OnObjectThrown?.Invoke(objHanded.GetComponent<ThrowableObject>());
    }

    private void PickObject(ThrowableObject currentObject)
    {
        if (pickingObject == null)
        {
            if (OutGameManager.Instance.currentWeapon != null)
                GetComponent<OutPlayerWeaponManager>().Holster();

            pickingObject = StartCoroutine(PickingObject(currentObject));
        }
    }

    IEnumerator PickingObject(ThrowableObject currentObject)
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetLayerWeight(animator.GetLayerIndex("Activities"), 1);
        animator.SetTrigger("PickObject");
        Transform handBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
        currentObject.transform.parent = handBone;
        currentObject.transform.SetLocalPositionAndRotation(
            handBone.Find("otherObjGrip").localPosition,
            handBone.Find("otherObjGrip").localRotation);

        yield return new WaitForSeconds(1);
        objHanded = currentObject.gameObject;
        animator.SetLayerWeight(animator.GetLayerIndex("Activities"), 0);
    }

    private void ThrowObject(ThrowableObject currentObject)
    {
        animator.SetLayerWeight(animator.GetLayerIndex("Activities"), 1);
        animator.SetTrigger("ThrowObject");
        throwingObject ??= StartCoroutine(ThrowingObject(currentObject));
    }

    IEnumerator ThrowingObject(ThrowableObject currentObject)
    {
        yield return new WaitForSeconds(1);
        currentObject.transform.SetParent(null);
        objHanded = null;

        if (!currentObject.GetComponent<Rigidbody>())
            currentObject.AddComponent<Rigidbody>();

        currentObject.TryGetComponent<Rigidbody>(out Rigidbody objRbdy);
        objRbdy.isKinematic = false;
        objRbdy.mass = 10;
        animator.SetLayerWeight(animator.GetLayerIndex("Activities"), 0);
    }

    // ================= JUMP =================
    private void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && !jumping)
        {
            animator.SetTrigger("Jump");
            jumpRoutine = StartCoroutine(JumpRoutine());
        }
    }

    IEnumerator JumpRoutine()
    {
        jumping = true;
        animator.SetBool("Airborne", true);
        float startY = transform.position.y;
        float targetY = startY + jumpHeight;

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;
            float newY = Mathf.Lerp(startY, targetY, t);
            rb.position = new Vector3(rb.position.x, newY, rb.position.z);
            yield return null;
        }

        // Falling down naturally with gravity
        while (!IsGrounded())
        {
            yield return null;
        }

        animator.SetBool("Airborne", false);
        jumping = false;
        jumpRoutine = null;
    }

    bool IsGrounded()
    {
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red);
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayers);

    }

    // ================= INPUT =================
    private void OnMove(InputAction.CallbackContext ctx)
    {
        cachedInput = ctx.ReadValue<Vector2>();

        if (animator.GetBool("AimingRifle"))
        {
            animator.SetFloat("Y", cachedInput.y);
            animator.SetFloat("X", cachedInput.x);
        }
        else
            animator.SetFloat("Y", cachedInput.sqrMagnitude > 0.01f ? 1 : 0);
    }

    private void Sprint(InputAction.CallbackContext ctx)
    {
        bool status = ctx.performed;
        animator.SetBool("Sprint", status);
    }

    void OnLook(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        if (animator.GetBool("AimingRifle"))
            animator.SetFloat("X_turn", input.x);
    }

    // ================= UPDATE =================
private void Update()
    {
        if (!IsGrounded()) animator.SetBool("Airborne", true);
        else animator.SetBool("Airborne", false);

        if (animator.GetBool("AimingRifle"))
        {
            Vector3 cameraEuler = cameraTransform.rotation.eulerAngles;
            Quaternion targetRotation = Quaternion.Euler(0, cameraEuler.y, 0);
            float lerpFactor = Time.deltaTime * 10f;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lerpFactor);
        }
        if (cachedInput.sqrMagnitude > 0.01f)
        {
            animator.SetBool("Idle", false);
            if (!animator.GetBool("AimingRifle"))
            {
                // Get camera-relative direction
                Vector3 camF = cameraTransform.forward;
                camF.y = 0;
                Vector3 camR = cameraTransform.right;
                camR.y = 0;

                Vector3 moveDir = (camF * cachedInput.y + camR * cachedInput.x).normalized;

                // Rotate character smoothly toward move direction
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                float lerpFactor = Time.deltaTime * 5f;  // adjust speed (5 → ~0.2s turn)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, lerpFactor);

                // Move character (Rigidbody)
                rb.linearVelocity = moveDir * moveSpeed + Vector3.up * rb.linearVelocity.y;

                // Check for elevating surface
                //CheckForElevatingSurface(moveDir);
            }

        }
        else
        {
            // Optionally zero horizontal movement when stopping
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            animator.SetBool("Idle", true);

        }

        if (ascendingStairs)
        {
            rb.AddForce(Vector3.up * 5f, ForceMode.Acceleration);
        }
    }

    [Header("Ascending Configs")]
    public float ascendCheckDistance = 1.5f;
    public float ascendCheckHeight = 0.5f;
    private bool ascendingStairs;

    void CheckForElevatingSurface(Vector3 moveDir)
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Vector3 direction = moveDir;
        RaycastHit hit;

        Debug.DrawRay(origin, direction * ascendCheckDistance, Color.green);

        if (Physics.Raycast(origin, direction, out hit, ascendCheckDistance))
        {
            // Check the angle of the surface
            float angle = Vector3.Angle(Vector3.up, hit.normal);

            // Adjust these angle thresholds as needed
            if (angle > 15f && angle < 45f) // Example: Stairs or sloped terrain
            {
                AscendStairs(true);
            }
            else
            {
                AscendStairs(false);
            }
        }
        else
        {
            AscendStairs(false);
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

    public void AscendStairs(bool status)
    {
        ascendingStairs = status;
        StartCoroutine(InterpolateFloat(status ? 0 : 1, status ? 1 : 0, 0.5f, v
            => animator.SetLayerWeight(animator.GetLayerIndex("Activities_Bottom"), v)));
        animator.SetBool("ascendStairs", status);
    }

    /// <summary>
    /// Enables or disables the ragdoll state for the character by toggling the kinematic and collision settings of its
    /// humanoid bones.
    /// </summary>
    /// <param name="isAnimated">A value indicating whether the character is animated or needs ragdoll.  Pass <see langword="true"/> to disable ragdoll
    ///, or <see langword="false"/> to tell the method that character is not animated and needs the ragdoll physics.</param>
    public void SetRagdollState(bool isAnimated)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator not found!");
            return;
        }

        // Loop through all humanoid bones
        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (bone == HumanBodyBones.LastBone)
                continue; // skip sentinel


            Transform boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform == null)
                continue;

            Rigidbody rb = boneTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = isAnimated;
                rb.detectCollisions = !isAnimated;
            }
        }
    }
}
