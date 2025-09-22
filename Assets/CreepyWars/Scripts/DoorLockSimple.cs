using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HingeJoint))]
public class DoorLockSimple : MonoBehaviour
{
    [Header("Lock Settings")]
    public bool isDoorLocked = true;
    [Space]
    public Transform playerUnlockingPosition;

    private HingeJoint hinge;
    private JointLimits originalLimits;
    private bool originalUseLimits;

    void Awake()
    {
        hinge = GetComponent<HingeJoint>();
    }

    void Start()
    {
        // Store original hinge limits
        originalLimits = hinge.limits;
        originalUseLimits = hinge.useLimits;

        if (isDoorLocked)
            LockDoorAtZero();
    }

    private void LockDoorAtZero()
    {
        JointLimits lockedLimits = new()
        {
            min = 0f,
            max = 0f
        };

        hinge.limits = lockedLimits;
        hinge.useLimits = true;
    }

    public void TryUnlockDoor()
    {
        OutGameManager.Instance.currentP.GetComponent<Animator>().SetTrigger("OpenDoor");
        Invoke(nameof(UnlockDoor), 1f);
    }

    private void UnlockDoor()
    {
        hinge.limits = originalLimits;
        hinge.useLimits = originalUseLimits;
        isDoorLocked = false;
        PlayerPrefs.SetInt("PlayerHasKey", 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isDoorLocked)
        {
            // Check if player has the key in PlayerPrefs
            if (PlayerPrefs.GetInt("PlayerHasKey", 0) == 1)
            {
                // Move player to unlocking position, then unlock
                StartCoroutine(MovePlayerToUnlockPosition());
            }
            else
            {
                ShooterUI.ShowRequirement.Invoke(RequirementPromptType.Key);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isDoorLocked)
        {
            ShooterUI.HideRequirement.Invoke();
        }
    }

    private IEnumerator MovePlayerToUnlockPosition()
    {
        Transform player = OutGameManager.Instance.currentP.transform;
        Rigidbody rb = OutGameManager.Instance.currentP.GetComponent<Rigidbody>();

        float timeElapsed = 0f;
        Vector3 startingPosition = player.position;

        while (timeElapsed < 0.5f)
        {
            float t = timeElapsed / 0.5f;
            player.GetComponent<Rigidbody>().position = Vector3.Lerp(startingPosition, playerUnlockingPosition.position, t * 20);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Snap final
        rb.position = playerUnlockingPosition.position;

        rb.isKinematic = false;

        // Call door unlock
        TryUnlockDoor();
    }
}
