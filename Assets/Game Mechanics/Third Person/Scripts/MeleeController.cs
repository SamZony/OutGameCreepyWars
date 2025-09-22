using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeController : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;
    public Transform playerCenterPoint;
    public float maxRayDistance;
    public LayerMask layerMask;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (playerCenterPoint == null) Debug.Log("Center point is not assigned in MeleeController in " + this.name);
    }
    public void MeleeHard()
    {
        animator.SetTrigger("MeleeHard");

        if (Physics.SphereCast(playerCenterPoint.position, 0.5f, playerCenterPoint.forward, out RaycastHit hit, maxRayDistance, layerMask)
)
        {
            if (hit.collider.TryGetComponent<OutEnemyController>(out OutEnemyController enemyController))
            {
                enemyController.TakeDamage(enemyController.Health);
            }
        }
    }

}
