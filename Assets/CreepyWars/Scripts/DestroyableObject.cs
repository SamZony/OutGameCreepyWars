using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public interface IDisableOnExplosion { }

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class DestroyableObject : MonoBehaviour
{
    public enum DestructionResult { FallOff, Explode }
    public enum DestroyMethod { Collision, Trigger, Manual }

    [Header("General Settings")]
    [Tooltip("How is this object destroyed?")]
    public DestroyMethod destroyMethod = DestroyMethod.Collision;

    [Tooltip("What happens to the object when destroyed? Explode requires a trigger SphereCollider attached.")]
    public DestructionResult destructionResult = DestructionResult.FallOff;

    [Tooltip("Minimum mass of the attacker object required to break this object.")]
    public float minAttackerMass = 1f;

    [Tooltip("Minimum velocity magnitude of the attacker object required to break this object.")]
    public float minAttackerVelocity = 2f;

    [Tooltip("Minimum number of hits required before breaking.")]
    public int minAttackTimes = 1;

    [Tooltip("if using Fall Off")]
    public AudioClip fallOffSound;

    #region explosion settings
    [Header("Explosion Settings")]
    [Tooltip("Force of the explosion applied to this object.")]
    public float explosionForce = 500f;

    [Tooltip("Upward modifier for explosion force.")]
    public float upwardsModifier = 2f;

    [Tooltip("Prefab to spawn on hit objects (fire, smoke, etc.).")]
    public GameObject fireParticlePrefab;
    [Tooltip("Particle prefab on this object (explosion, etc.).")]
    public GameObject explosionParticlePrefab;

    public AudioClip explosionSound;

    [Tooltip("Maximum number of fire prefabs to spawn per explosion.")]
    public int maxFireSpawns = 5;

    [Header("Explosion Delay Settings")]
    [Tooltip("Delay before triggering explosion after valid trigger/collision.")]
    public float explosionDelay = 0f;

    [Tooltip("If true, explosion will still happen even if attacker leaves the trigger. If false, timer resets.")]
    public bool persistExplosionAfterExit = false;

    // CRATER SETTINGS
    [Header("Crater Settings")]
    [Tooltip("Radius of the crater in world units.")]
    public float craterRadius = 5f;

    [Tooltip("Depth of the crater deformation.")]
    public float craterDepth = 2f;

    private float[,] originalHeights;
    private int savedX, savedZ, savedSize;
    private bool savedHeights = false;
    // CRATER SETTINGS END


    public static Action<Transform> showDangerIndication;
    public static Action hideDangerIndication;

    #endregion


    [Header("Events")]
    [Tooltip("Events triggered after breaking this object.")]
    public UnityEvent afterBreaking;

    private Rigidbody rb;
    private bool isBroken = false;
    private int attackCount = 0;

    private AudioSource audioSource;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private Coroutine explosionDelayRoutine;
    private Collider currentTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (isBroken || destroyMethod != DestroyMethod.Trigger) return;

        if (other.CompareTag("Player"))
        {
            showDangerIndication?.Invoke(transform);
            if (other.attachedRigidbody != null &&
            other.attachedRigidbody.mass >= minAttackerMass)
            {
                currentTrigger = other;

                if (explosionDelay > 0)
                {
                    explosionDelayRoutine ??= StartCoroutine(ExplosionDelayRoutine());
                }
                else
                {
                    Break();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (destroyMethod != DestroyMethod.Trigger) return;

        if (other.CompareTag("Player"))
        {
            hideDangerIndication?.Invoke();
        }

        if (!persistExplosionAfterExit && other == currentTrigger)
        {
            if (explosionDelayRoutine != null)
            {
                StopCoroutine(explosionDelayRoutine);
                explosionDelayRoutine = null;
            }
        }
    }

    private IEnumerator ExplosionDelayRoutine()
    {
        yield return new WaitForSeconds(explosionDelay);
        Break();
    }


    public void ManualDestroy()
    {
        if (destroyMethod == DestroyMethod.Manual && !isBroken)
        {
            Break();
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken) return;

        if (collision.rigidbody == null)
        {
            Debug.LogWarning($"Collider {collision.gameObject.name} has no Rigidbody (required for {name}).");
            return;
        }

        bool validMass = collision.rigidbody.mass >= minAttackerMass;
        bool validVelocity = collision.rigidbody.linearVelocity.magnitude >= minAttackerVelocity;

        if (validMass && validVelocity)
        {
            attackCount++;
            if (attackCount >= minAttackTimes)
                Break();
        }
    }

    private void Break()
    {
        isBroken = true;
        rb.isKinematic = false;

        if (destructionResult == DestructionResult.Explode)
            HandleExplosion();

        else HandleFallOff();

        afterBreaking?.Invoke();
    }

    private void HandleFallOff()
    {
        if (fallOffSound)
            audioSource.PlayOneShot(fallOffSound);
    }

    private void HandleExplosion()
    {
        PlayExplosionSound();

        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere == null) return;

        if (explosionParticlePrefab)
        {
            explosionParticlePrefab.SetActive(true);
            explosionParticlePrefab.GetComponent<ParticleSystem>().Play();
        }

        rb.AddExplosionForce(explosionForce, transform.position, sphere.radius, upwardsModifier, ForceMode.Impulse);

        Collider[] hits = Physics.OverlapSphere(transform.position, sphere.radius);
        int fireSpawned = 0;

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            bool hasIDamagable = false;

            foreach (var script in hit.GetComponents<MonoBehaviour>())
            {
                if (script is IDamageable damageable)
                {
                    hasIDamagable = true;
                    damageable.TakeDamage(100);
                }
                else if (script is IDisableOnExplosion)
                    script.enabled = false;
            }

            if (!hasIDamagable)
            {
                if (hit.TryGetComponent<NavMeshAgent>(out var agent))
                    agent.enabled = false;

                if (hit.TryGetComponent<Animator>(out var animator))
                    animator.enabled = false;
            }

            // Spawn fire effect (respect limit & valid rigidbody)
            if (fireParticlePrefab != null && fireSpawned < maxFireSpawns)
            {
                if (hit.TryGetComponent<Rigidbody>(out var hitRb) && !hitRb.isKinematic)
                {
                    GameObject fireObj = Instantiate(fireParticlePrefab, hit.transform.position, Quaternion.identity, hit.transform);
                    Destroy(fireObj, 7f);
                    fireSpawned++;
                }
            }
        }

        CreateCrater();
    }

    private void CreateCrater()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null) return;

        TerrainData tData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int mapX = (int)(((transform.position.x - terrainPos.x) / tData.size.x) * tData.heightmapResolution);
        int mapZ = (int)(((transform.position.z - terrainPos.z) / tData.size.z) * tData.heightmapResolution);

        int radius = Mathf.RoundToInt((craterRadius / tData.size.x) * tData.heightmapResolution);

        // Save original heights once
        if (!savedHeights)
        {
            originalHeights = tData.GetHeights(mapX - radius, mapZ - radius, radius * 2, radius * 2);
            savedX = mapX - radius;
            savedZ = mapZ - radius;
            savedSize = radius * 2;
            savedHeights = true;
        }

        float[,] heights = tData.GetHeights(mapX - radius, mapZ - radius, radius * 2, radius * 2);

        for (int x = 0; x < radius * 2; x++)
        {
            for (int z = 0; z < radius * 2; z++)
            {
                float dx = (x - radius) / (float)radius;
                float dz = (z - radius) / (float)radius;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);

                if (dist < 1.0f)
                {
                    heights[z, x] -= (1 - dist) * (craterDepth / tData.size.y);
                }
            }
        }

        tData.SetHeights(mapX - radius, mapZ - radius, heights);
    }




    private void PlayExplosionSound()
    {
        if (explosionSound == null) return;

        Debug.Log("Playing explosion sound");
        // Create a temporary GameObject at this position
        GameObject soundGO = new GameObject("ExplosionSound");
        soundGO.transform.position = transform.position;

        // Add AudioSource and configure it
        AudioSource audioSource = soundGO.AddComponent<AudioSource>();
        audioSource.clip = explosionSound;
        audioSource.spatialBlend = 0.7f; // 3D sound
        audioSource.volume = 1.0f;
        audioSource.minDistance = 300f;
        audioSource.maxDistance = 1000f;
        audioSource.playOnAwake = false;

        // Play the sound
        audioSource.Play();

        // Destroy after the clip length (or 2-3 sec if unknown)
        float lifetime = explosionSound.length > 0 ? explosionSound.length : UnityEngine.Random.Range(2f, 3f);
        Destroy(soundGO, lifetime);
    }

    private void OnApplicationQuit()
    {
        ResetTerrain();
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) // Leaving Play Mode
            ResetTerrain();
#endif
    }

    private void ResetTerrain()
    {
        if (!savedHeights) return;

        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null) return;

        terrain.terrainData.SetHeights(savedX, savedZ, originalHeights);
        savedHeights = false;
    }

}
