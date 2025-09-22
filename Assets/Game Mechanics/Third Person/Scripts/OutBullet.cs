using System.Collections;
using System.Linq;
using UnityEngine;

public class OutBullet : MonoBehaviour
{
    public enum BulletHitMode
    {
        Raycast,
        Collision
    }

    [Header("Bullet Settings")]
    public OutWeapon weapon;
    public GameObject owner;
    [SerializeField] private float lifeTime = 1.3f;

    [Header("Hit Detection")]
    public BulletHitMode hitMode = BulletHitMode.Collision; // Choose in Inspector
    public float checkDistance = 0.3f;
    public LayerMask hitMask;

    private Vector3 lastPosition;

    public void Init(OutWeapon sourceWeapon, GameObject owner)
    {
        weapon = sourceWeapon;
        this.owner = owner;
    }

    private void OnEnable()
    {
        lastPosition = transform.position;
        StartCoroutine(Disable());
    }

    private void Update()
    {
        if (hitMode == BulletHitMode.Raycast)
        {
            Vector3 direction = transform.forward;
            float distance = Vector3.Distance(lastPosition, transform.position);

            if (Physics.Raycast(lastPosition, direction, out RaycastHit hit, distance + checkDistance, hitMask))
            {
                Debug.Log($"Bullet raycast hit {hit.collider.gameObject.name} (Tag: {hit.collider.tag})");
                HandleHit(hit.collider.gameObject, hit);
                HandleCleanup(hit);
            }

            lastPosition = transform.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hitMode != BulletHitMode.Collision) return;

        Debug.Log($"Bullet collided with {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        HandleHit(collision.gameObject, collision);
        HandleCleanup(collision);
    }

    // ===============================
    // Unified Hit Handling
    // ===============================
    private void HandleHit(GameObject hitGo, RaycastHit hit)
    {
        if (weapon == null)
        {
            Debug.LogError("Bullet has no weapon reference.");
            return;
        }

        var damageable = hitGo.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            if (hitGo == owner)
            {
                StartCoroutine(Disable());
                return;
            }

            Debug.Log($"Applying {weapon.damage} damage to {damageable} on {hitGo.name}");
            damageable.TakeDamage(weapon.damage);
            SpawnBloodAt(hit);
        }

        HandleImpactFX(hitGo, hit.point, hit.normal);
    }

    private void HandleHit(GameObject hitGo, Collision collision)
    {
        if (weapon == null)
        {
            Debug.LogError("Bullet has no weapon reference.");
            return;
        }

        var damageable = hitGo.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            if (hitGo == owner)
            {
                StartCoroutine(Disable());
                return;
            }

            Debug.Log($"Applying {weapon.damage} damage to {damageable} on {hitGo.name}");
            damageable.TakeDamage(weapon.damage);
            SpawnBloodAt(collision);
        }

        var contact = collision.GetContact(0);
        HandleImpactFX(hitGo, contact.point, contact.normal);
    }

    private void HandleImpactFX(GameObject hitGo, Vector3 point, Vector3 normal)
    {
        Renderer renderer = hitGo.GetComponent<Renderer>();
        Texture hitTexture = renderer ? renderer.material.mainTexture : null;

        foreach (var impact in OutGameManager.Instance.bulletImpacts)
        {
            if (impact.surfaceTexture == hitTexture)
            {
                if (impact.impactEffect != null)
                {
                    var fx = Instantiate(impact.impactEffect, point, Quaternion.LookRotation(normal));
                    fx.Play();
                    Destroy(fx.gameObject, fx.main.duration);
                }

                if (impact.impactSound != null)
                {
                    AudioSource.PlayClipAtPoint(impact.impactSound, point);
                }
                break;
            }
        }
    }

    // ===============================
    // Blood & Cleanup
    // ===============================
    private void SpawnBloodAt(RaycastHit hit)
    {
        var bloodGo = WeaponPoolManager.Instance.GetBlood();
        bloodGo.SetActive(true);
        bloodGo.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));

        bloodGo.GetComponent<ParticleSystem>().Play();
        ParticleSystem system = bloodGo.GetComponent<ParticleSystem>();
        //var systems = bloodGo.GetComponentsInChildren<ParticleSystem>();
        //foreach (var ps in systems)
        //{
        //    var main = ps.main;
        //    main.loop = false;
        //    var emission = ps.emission;
        //    emission.enabled = true;
        //    ps.Play();
        //}

        if (gameObject.activeSelf)
            StartCoroutine(ReturnBloodToPoolWhenStopped(bloodGo, system));
    }

    private void SpawnBloodAt(Collision collision)
    {
        var bloodGo = WeaponPoolManager.Instance.GetBlood();
        bloodGo.SetActive(true);
        var contact = collision.GetContact(0);
        bloodGo.transform.SetPositionAndRotation(contact.point, Quaternion.LookRotation(contact.normal));

        bloodGo.GetComponent<ParticleSystem>().Play();
        ParticleSystem system = bloodGo.GetComponent<ParticleSystem>();
        //var systems = bloodGo.GetComponentsInChildren<ParticleSystem>();
        //foreach (var ps in systems)
        //{
        //    var main = ps.main;
        //    main.loop = false;
        //    var emission = ps.emission;
        //    emission.enabled = true;
        //    ps.Play();
        //}

        if (gameObject.activeSelf)
            StartCoroutine(ReturnBloodToPoolWhenStopped(bloodGo, system));
    }

    private void HandleCleanup(RaycastHit hit)
    {
        WeaponPoolManager.Instance.ReturnBullet(gameObject);
        if (gameObject.activeSelf) StartCoroutine(Disable());
    }

    private void HandleCleanup(Collision collision)
    {
        WeaponPoolManager.Instance.ReturnBullet(gameObject);
        if (gameObject.activeSelf) StartCoroutine(Disable());
    }

    private IEnumerator ReturnBloodToPoolWhenStopped(GameObject bloodGo, ParticleSystem system)
    {
        //foreach (var ps in systems)
        //    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        yield return new WaitUntil(() => system.IsAlive(true));
        WeaponPoolManager.Instance.ReturnBlood(bloodGo);
    }

    private IEnumerator Disable()
    {
        yield return new WaitForSeconds(lifeTime);
        WeaponPoolManager.Instance.ReturnBullet(gameObject);
    }
}
