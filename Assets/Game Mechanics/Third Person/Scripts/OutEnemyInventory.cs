using UnityEngine;
using UnityEngine.InputSystem;
using static OutEnemyController;

public class OutEnemyInventory : MonoBehaviour
{

    [Header("Environmental needs")]
    private bool allowedToInteract;
    public bool hasKey;
    public bool hasCard;

    public bool isDead;


    public enum CurrentInteraction { None, StealthKill, Loot }

    public CurrentInteraction currentInteraction;



    #region InputAction
    InputSystem_Actions action;
    #endregion

    public int health = 100;
    public int Health = 100;


    private void Awake()
    {
        action = new InputSystem_Actions();
    }

    #region EnableDisable
    void OnEnable()
    {
        action.Player.Enable();

        action.Player.Interact.performed += PlayerInteractions;
        action.Player.Interact.canceled += PlayerInteractions;
    }

    private void OnDisable()
    {
        action.Player.Interact.performed -= PlayerInteractions;
        action.Player.Interact.canceled -= PlayerInteractions;
    }

    #endregion



    private void PlayerInteractions(InputAction.CallbackContext context)
    {
        Debug.Log("Entered the context method");
        if (allowedToInteract)
        {
            switch (currentInteraction)
            {
                case CurrentInteraction.StealthKill:
                    break;
                case CurrentInteraction.Loot:
                    Loot();
                    break;
            }
        }
    }

    private void Loot()
    {
        if (hasKey || hasCard)
        {
            Debug.Log("Enemy has key, looting...");
            Animator playerAnimator = OutGameManager.Instance.currentP.GetComponent<Animator>();
            playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex("Activities"), 1);
            playerAnimator.SetTrigger("PickObject");

            if (hasKey)
            {
                hasKey = false;
                PlayerPrefs.SetInt("PlayerHasKey", 1);
            }
            if (hasCard)
            {
                hasCard = false;
                PlayerPrefs.SetInt("PlayerHasCard", 1);
            }

            ShooterUI.HideLootPrompt();
            Debug.Log("Looted key");

            Invoke(nameof(SetAnimLayerWeightBack), 1);
        }
    }

    private void SetAnimLayerWeightBack()
    {
        Animator playerAnimator = OutGameManager.Instance.currentP.GetComponent<Animator>();
        playerAnimator.SetLayerWeight(playerAnimator.GetLayerIndex("Activities"), 0);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            allowedToInteract = true;

            if (isDead)
            {
                ShooterUI.ShowLootPrompt.Invoke();
                currentInteraction = CurrentInteraction.Loot;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            allowedToInteract = false;

            if (isDead)
            {
                ShooterUI.HideLootPrompt.Invoke();
                currentInteraction = CurrentInteraction.None;
            }
        }
    }



}
