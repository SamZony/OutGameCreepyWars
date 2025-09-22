using UnityEngine;

public class OutHitbox : MonoBehaviour, IDamageable
{
    [SerializeField] public MonoBehaviour damageableTarget; // Must implement IDamageable
    [SerializeField] public float damageMultiplier = 1f;

    public float Health { get; set; }

    private IDamageable target;

    private void Awake()
    {
        if (damageableTarget is IDamageable d)
        {
            target = d;
        }
        else
        {
            Debug.LogError($"[Hitbox] Assigned target on {gameObject.name} does not implement IDamageable.");
        }
    }

    public void TakeDamage(float damage)
    {
        if (target != null)
        {
            float finalDamage = damage * damageMultiplier;
            target.TakeDamage(finalDamage);
            Debug.Log($"[Hitbox] Passed {finalDamage} damage to {target}");
        }
    }
}
