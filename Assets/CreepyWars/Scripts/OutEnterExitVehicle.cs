using DG.Tweening;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class OutEnterExitVehicle : MonoBehaviour
{
    Rigidbody rb;
    Animator animator;

    public CinemachineCamera cineCamV;
    CinemachineCamera cineCamP;

    [Header("Enter/Exit Points")]
    public Transform enterPosition;
    public Transform drivingPosition;

    public OutVehicleController currentVehicle;

    [Header("Player Components to Disable When Driving")]
    public List<MonoBehaviour> playerComponents = new List<MonoBehaviour>();

    InputSystem_Actions inputActions;
    InputAction enterExit;

    public bool insideVehicle;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        enterExit = inputActions.Player.EnterExit;
    }

    private void OnEnable()
    {
        enterExit.performed += DoEnterExit;
        enterExit.Enable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        cineCamP = GetComponent<OutShooterController>().cineCam;
    }

    private void OnDisable()
    {
        enterExit.performed -= DoEnterExit;
        enterExit.Disable();
    }

    private void DoEnterExit(InputAction.CallbackContext context)
    {
        if (!insideVehicle && OutGameManager.Instance != null)
            EnterVehicle();
        else
            ExitVehicle();
    }

    private void FixedUpdate()
    {
        if (insideVehicle)
        {
            rb.position = drivingPosition.position;
            rb.rotation = drivingPosition.rotation;
        }
    }

    Vector3 playerScale;
    private void EnterVehicle()
    {
        Debug.Log("Enter method ran");

        animator.SetLayerWeight(animator.GetLayerIndex("Activities"), 1);
        animator.SetTrigger("EnterVehicle");
        // Ensure we have a valid vehicle from GameManager
        if (OutGameManager.Instance?.currentVehicle == null || insideVehicle)
            return;

        currentVehicle = OutGameManager.Instance.currentVehicle;
        currentVehicle.enabled = true;

        enterPosition = currentVehicle.enterPosition;
        drivingPosition = currentVehicle.drivingPosition;

        // Disable player movement/interaction scripts
        foreach (var comp in playerComponents)
        {
            if (comp != null) comp.enabled = false;
        }

        rb.isKinematic = true;
        animator.applyRootMotion = false;

        playerScale = transform.localScale;
        // Parent to vehicle for correct movement sync
        transform.SetParent(currentVehicle.transform, true);

        rb.DORotate(enterPosition.rotation.eulerAngles, 0.5f, RotateMode.Fast);
        // Smooth movement into vehicle
        rb.DOMove(enterPosition.position, 0.5f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                cineCamV.gameObject.SetActive(true);
                cineCamV.Priority = 20 ;

                rb.DORotate(drivingPosition.rotation.eulerAngles, 1f);
                rb.DOMove(drivingPosition.position, 1f)
                  .SetEase(Ease.Linear)
                  .OnComplete(() =>
                  {
                      insideVehicle = true;
                      SetTriggerType(OutVehicleTrigger.TriggerType.ExitTrigger);
                  });
            });
    }

    private void ExitVehicle()
    {
        Debug.Log("Exit method ran");

        if (currentVehicle == null || !insideVehicle)
            return;

        rb.DOMove(enterPosition.position, 1f)
          .OnComplete(() =>
          {
              rb.isKinematic = false;
              animator.applyRootMotion = true;

              foreach (var comp in playerComponents)
              {
                  if (comp != null) comp.enabled = true;
              }

              cineCamV.gameObject.SetActive(false);
              cineCamV.Priority = 14;

              transform.SetParent(null, true);
              insideVehicle = false;
              SetTriggerType(OutVehicleTrigger.TriggerType.EnterTrigger);
              currentVehicle.enabled = false;
              currentVehicle = null;
              animator.SetLayerWeight(animator.GetLayerIndex("Activities"), 0);

          });
    }

    private void SetTriggerType(OutVehicleTrigger.TriggerType type)
    {
        if (currentVehicle?.enterPosition != null)
        {
            var trigger = currentVehicle.enterPosition.GetComponent<OutVehicleTrigger>();
            if (trigger != null) trigger.triggerType = type;
        }
    }

    public void OpenDoor()
    {
        if (currentVehicle != null) currentVehicle.OpenDoor();
    }

    public void StopDoorMotor()
    {
        if (currentVehicle != null) currentVehicle.StopDoorMotor();
    }
}
