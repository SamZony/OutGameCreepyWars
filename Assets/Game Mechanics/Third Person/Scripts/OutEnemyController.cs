using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(NavMeshAgent))]
public class OutEnemyController : MonoBehaviour, IDamageable, IDisableOnExplosion
{
    public enum EnemyType
    {
        Sniper,
        Patrol,
        Driver,
        Convoy
    }

    
    [Header("Enemy type")]
    public EnemyType enemyType;

    [Header("References")]
    [SerializeField] Animator animator;
    private NavMeshAgent agent;
    private Transform player;

    [Header("Multi Aim Constraint")]
    public List<MultiAimConstraint> multiAimConstraints = new List<MultiAimConstraint>();
    // Internal storage for their source arrays
    private List<WeightedTransformArray> sources = new List<WeightedTransformArray>();

    [Header("Health & Damage")]
    public float health = 100f;
    [SerializeField] float hurtDuration = 2f;
    [SerializeField] float waitAfterAlert = 1f;
    [Tooltip("Minimum collision force magnitude to trigger damage.")]
    public float impactThreshold = 10f;
    public float Health { get; set; }

    private Coroutine hurtRoutine;

    [Header("Sounds")]
    public List<AudioClip> engageSounds;

    [Header("Vision Settings")]
    public float viewDistance = 10f;
    public float viewAngle = 45f;
    public LayerMask viewMask;

    [Header("Combat Distance")]
    public float minCombatDist = 4f;
    public float maxCombatDist = 6f;
    private bool isEngaging = false;

    [Header("AI Firing")]
    public float minFireDuration = 1f;
    public float maxFireDuration = 3f;
    public float minReloadTime = 1f;
    public float maxReloadTime = 2f;
    public OutWeapon currentWeapon;

    private bool doingFireCycle = false;

    [Header("Movement Settings")]
    public float chaseDistance = 5f;
    public float coverRadius = 3f;
    public float turnSpeed = 5f;

    [Header("Wandering Settings")]
    [SerializeField] bool wanderFromStart;
    [SerializeField] float standDurationMin = 1f;
    [SerializeField] float standDurationMax = 3f;
    [SerializeField] float wanderDurationMin = 2f;
    [SerializeField] float wanderDurationMax = 5f;
    [SerializeField] float wanderRadius = 5f;
    [SerializeField] float wanderMoveSpeed = 1.5f;
    [SerializeField] float rotationSpeed = 5f;

    [SerializeField] List<string> idleAnimatorTriggers;

    private Coroutine wanderRoutine;

    [SerializeField] private GameObject hitboxPrefab; // A basic empty GameObject with Collider + Hitbox
    [SerializeField] private bool autoGenerateHitboxes = true;

    private AudioSource audioSource;

    



    [System.Serializable]
    public struct HitboxInfo
    {
        public HumanBodyBones bone;
        public float damageMultiplier;
    }

    [SerializeField]
    private HitboxInfo[] hitboxes = new HitboxInfo[]
    {
        new HitboxInfo { bone = HumanBodyBones.Head, damageMultiplier = 2.0f },
        new HitboxInfo { bone = HumanBodyBones.Chest, damageMultiplier = 1.0f },
        new HitboxInfo { bone = HumanBodyBones.LeftLowerArm, damageMultiplier = 0.7f },
        new HitboxInfo { bone = HumanBodyBones.RightLowerArm, damageMultiplier = 0.7f },
        new HitboxInfo { bone = HumanBodyBones.LeftLowerLeg, damageMultiplier = 0.5f },
        new HitboxInfo { bone = HumanBodyBones.RightLowerLeg, damageMultiplier = 0.5f }
    };

   

    private void Awake()
    {
        if (autoGenerateHitboxes && animator != null)
        {
            GenerateHitboxes();
        }

        agent = GetComponent<NavMeshAgent>();
        if (!animator)
            Debug.LogError("[OutEnemy] Animator not assigned!");

        Health = health;
    }

    private void GenerateHitboxes()
    {
        foreach (var info in hitboxes)
        {
            Transform bone = animator.GetBoneTransform(info.bone);
            if (bone == null)
            {
                Debug.LogWarning($"[HitboxGen] Bone not found: {info.bone}");
                continue;
            }

            GameObject hitboxGO = Instantiate(hitboxPrefab, bone);
            hitboxGO.name = $"{info.bone}_Hitbox";
            hitboxGO.transform.localPosition = Vector3.zero;
            hitboxGO.transform.localRotation = Quaternion.identity;

            // Setup Hitbox component
            OutHitbox hitbox = hitboxGO.GetComponent<OutHitbox>();
            if (hitbox == null)
                hitbox = hitboxGO.AddComponent<OutHitbox>();

            hitbox.damageableTarget = this;
            hitbox.damageMultiplier = info.damageMultiplier;
            hitboxGO.GetComponent<Collider>().isTrigger = false;
        }

        Debug.Log("[HitboxGen] Hitboxes generated successfully.");
    }


    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p) player = p.transform;
        else Debug.LogError("[OutEnemy] No GameObject tagged 'Player' found!");

        agent.updateRotation = false;  // We'll manage rotation manually
        agent.updatePosition = true;   // Agent moves by displacement

        currentWeapon = animator.GetBoneTransform(HumanBodyBones.RightHand).GetComponentInChildren<OutWeapon>();

        if (wanderFromStart)
            StartWandering();


        // Initialize source arrays for each constraint
        sources.Clear();
        foreach (var constraint in multiAimConstraints)
        {
            WeightedTransformArray src = constraint.data.sourceObjects;
            src.Clear();
            src.Add(new WeightedTransform(player.GetComponent<OutShooterController>().lookAtTarget, 1f));

            // assign back
            constraint.data.sourceObjects = src;

            // store for later if you need to reference them
            sources.Add(src);

        }
    }

    void Update()
    {
        if (player == null || agent == null) return;

        if (!isEngaging)
        {
            Vector3 velocity = agent.velocity;
            velocity.y = 0;

            if (velocity.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            }
        }

        player = OutGameManager.Instance.currentP;


        if (CanSeePlayer())
        {
            // Ensure only one instance runs
            if (!IsInvoking(nameof(StartEngage)))
                Invoke(nameof(StartEngage), 0f);


        }


        foreach (var constraint in multiAimConstraints)
        {
            constraint.weight = CanSeePlayer() ? 1f : 0f;
        }

        if (health <= 0f && animator.enabled)
        {
            Die();
        }
    }

    #region Vision
    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        if (dist > viewDistance)
        {
            isEngaging = false;
            return false;
        }

        toPlayer.Normalize();
        if (Vector3.Angle(transform.forward, toPlayer) > viewAngle)
        {
            isEngaging = false;
            return false;
        }


        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, toPlayer, dist, viewMask))
        {
            isEngaging = false;
            return false;
        }

        if (player.GetComponent<OutShooterController>().Health <= 0)
        {
            isEngaging = false;
            return false;
        }

        isEngaging = true;
        return true;

    }

    #endregion

    private IEnumerator WanderLoop()
    {
        while (true)
        {
            // 1) Stand still and play random idle animation
            float standTime = Random.Range(standDurationMin, standDurationMax);
            agent.ResetPath();
            animator.SetFloat("X", 0f);
            animator.SetFloat("Y", 0f);

            // Trigger random idle animation if available
            if (idleAnimatorTriggers.Count > 0)
            {
                string trigger = idleAnimatorTriggers[Random.Range(0, idleAnimatorTriggers.Count)];
                animator.SetTrigger(trigger);
            }

            yield return new WaitForSeconds(standTime);

            // 2) Wander to a new position
            float wanderTime = Random.Range(wanderDurationMin, wanderDurationMax);
            Vector3 origin = transform.position;
            Vector3 randomPoint = origin + Random.insideUnitSphere * wanderRadius;
            randomPoint.y = origin.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                Vector3 destination = hit.position;

                // Rotate to face target
                Quaternion targetRotation = Quaternion.LookRotation(destination - transform.position);
                float rotateTimer = 0f;
                while (Quaternion.Angle(transform.rotation, targetRotation) > 1f && rotateTimer < 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                    rotateTimer += Time.deltaTime;
                    yield return null;
                }

                agent.speed = wanderMoveSpeed;
                agent.SetDestination(destination);
                float timer = 0f;

                // Wait until path is ready
                while (agent.pathPending) yield return null;

                // Move toward destination
                while (timer < wanderTime && agent.remainingDistance > agent.stoppingDistance)
                {
                    if (health <= 0f) yield break; // Exit if dead
                    timer += Time.deltaTime;
                    UpdateAnimatorXY(); // make sure this updates movement anim
                    yield return null;
                }

                // Reset animator
                animator.SetFloat("X", 0f);
                animator.SetFloat("Y", 0f);
            }

            yield return null; // Continue to next loop
        }
    }

    void StartWandering()
    {
        if (wanderRoutine != null)
            StopCoroutine(wanderRoutine);
        wanderRoutine = StartCoroutine(WanderLoop());
    }

    void StopWandering()
    {
        if (wanderRoutine != null)
            StopCoroutine(wanderRoutine);
        wanderRoutine = null;
    }


    void StartEngage()
    {
        StopWandering();
        StopAllCoroutines();
        if (engageSounds.Count > 0)
        {
            int random = Random.Range(0, engageSounds.Count);
            PlaySound(engageSounds[random]);
        }
        StartCoroutine(EngagePlayer());
    }

    private void PlaySound(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    IEnumerator EngagePlayer()
    {
        doingFireCycle = false;
        Debug.Log("[OutEnemy] EngagePlayer started");
        while (CanSeePlayer() && health > 0f)
        {
            animator.SetLayerWeight(2, 1);
            FacePlayer();
            animator?.SetBool("AimingRifle", true);
            animator?.SetBool("GunMode", true);


            // Inside your EngagePlayer loop:
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > maxCombatDist)
                MoveToPosition(player.position);
            else if (dist < minCombatDist)
                MoveToPosition(transform.position + (transform.position - player.position).normalized * (minCombatDist));
            else
                agent.ResetPath(); // stay put
            if (dist > chaseDistance)
            {
                MoveToPosition(player.position);
            }
            else
            {
                Vector3 randomPoint = transform.position + Random.insideUnitSphere * coverRadius;
                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, coverRadius, NavMesh.AllAreas))
                    MoveToPosition(hit.position);
                else Debug.LogWarning("[OutEnemy] Failed to find valid cover NavMesh position");

                animator.SetFloat("X", Vector3.Dot(agent.velocity.normalized, transform.right));
                animator.SetFloat("Y", Vector3.Dot(agent.velocity.normalized, transform.forward));
            }
            TryStartFiring();
            yield return new WaitForSeconds(Random.Range(1f, 2f));
        }
        animator.SetLayerWeight(2, 0);

        Debug.Log("[OutEnemy] EngagePlayer ended");
        agent.ResetPath();
        animator?.SetBool("AimingRifle", false);
        animator?.SetBool("GunMode", false);
        StartWandering();
    }

    void UpdateAnimatorXY()
    {
        Vector3 vel = agent.velocity;
        Vector3 localVel = transform.InverseTransformDirection(vel);
        animator.SetFloat("X", localVel.x);
        animator.SetFloat("Y", localVel.z);
    }

    #region FireCycle
    IEnumerator AIFireCycle()
    {
        doingFireCycle = true;
        float fireTime = Random.Range(minFireDuration, maxFireDuration);
        float timer = 0f;
        while (timer < fireTime && CanSeePlayer())
        {
            Fire(); // call your OutWeapon.Fire()
            if (currentWeapon) yield return new WaitForSeconds(1f / currentWeapon.shotsPerSecond);
            timer += Time.deltaTime;
        }
        StopFire();
        float reload = Random.Range(minReloadTime, maxReloadTime);
        yield return new WaitForSeconds(reload);
        doingFireCycle = false;
    }

    void TryStartFiring()
    {
        if (!doingFireCycle && CanSeePlayer())
            StartCoroutine(AIFireCycle());
    }

    void Fire()
    {
        if (currentWeapon) currentWeapon.StartFireAI();
    }

    void StopFire()
    {
        if (currentWeapon) currentWeapon.StopFireAI();
    }

    #endregion

    void MoveToPosition(Vector3 target)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 1f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogError($"[OutEnemy] MoveToPosition: target {target} is off NavMesh!");
        }
    }

    void FacePlayer()
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0f) return;

        Quaternion target = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * turnSpeed);
    }

    public void TakeDamage(float amount)
    {
        if (health <= 0f)
        {
            Debug.LogWarning("[Enemy] dead");
            Die();
            return;
        }

        health -= amount;
        Debug.Log($"[Enemy] Took damage: {amount}, health now {health}");

        // Restart the hurt routine on each hit
        if (hurtRoutine != null)
        {
            StopCoroutine(hurtRoutine);
            Debug.Log("[Enemy] Restarting ReactToDamage coroutine.");
        }
        hurtRoutine = StartCoroutine(ReactToDamage());
    }

    public void OnHeardNoise(Vector3 noisePos)
    {
        if (health <= 0f) return;
        StopWandering();
        StopAllCoroutines();
        StartCoroutine(ApproachNoise(noisePos));
    }

    IEnumerator ApproachNoise(Vector3 pos)
    {
        agent.SetDestination(pos);
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance);
        yield return new WaitForSeconds(waitAfterAlert);
        if (CanSeePlayer()) StartEngage();
        else StartWandering();
    }

    private IEnumerator ReactToDamage()
    {
        Debug.Log("[Enemy] ReactToDamage started");
        animator.SetBool("hurt", true);

        float timer = 0f;
        float lastHealth = health;

        while (timer < hurtDuration)
        {
            if (health < lastHealth)
            {
                timer = 0f;  // reset on additional damage
                lastHealth = health;
                Debug.Log("[Enemy] ReactToDamage timer reset due to new damage.");
            }
            timer += Time.deltaTime;
            yield return null;
        }

        animator.SetBool("hurt", false);
        yield return new WaitForSeconds(waitAfterAlert);

        Debug.Log("[Enemy] ReactToDamage ended");
        hurtRoutine = null;

        if (CanSeePlayer())
        {
            StartEngage();  // Immediately engage if player in sight
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Grab the collision impact velocity (relative speed at impact)
        float impact = collision.relativeVelocity.magnitude;
        float mass = collision.rigidbody.mass;

        // Only trigger TakeDamage if impact is strong enough
        if (impact >= impactThreshold && mass >= 49)
        {
            if (health >= 0)
                TakeDamage(100);
        }
    }

    public void Die()
    {

        Debug.Log("health");
        Debug.Log("[OutEnemy] Died, disabling animator & agent");
        StopAllCoroutines();
        currentWeapon.audioSource.loop = false;
        currentWeapon.StopFireAI();
        agent.isStopped = true;
        animator.enabled = false;
        agent.enabled = false;
        currentWeapon.AddComponent<Rigidbody>();
        currentWeapon.GetComponent<BoxCollider>().enabled = true;
        SoundManager.Instance.PlaySylphGameplayDialogue(SylphDialogueContext.stealth);
        TryGetComponent<OutEnemyInventory>(out OutEnemyInventory inventory);
        if (inventory) inventory.isDead = true;
        this.enabled = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 left = Quaternion.Euler(0, -viewAngle, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, left * viewDistance);
        Gizmos.DrawRay(transform.position, right * viewDistance);
    }
}
