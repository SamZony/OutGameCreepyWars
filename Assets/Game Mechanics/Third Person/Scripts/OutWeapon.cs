using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponFor
{
    Player1, Player2
}

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AudioSource))]
public class OutWeapon : MonoBehaviour
{
    [Header("Weapon Setup")]
    public GameObject bulletPrefab;
    public Transform shellPrefab;
    public List<ParticleSystem> muzzleFlash;
    public AudioClip multiShotSound;
    public AudioClip singleShotSound;
    public Light muzzleLight;
    [HideInInspector]
    public AudioSource audioSource;

    [Header("References")]
    public Transform muzzlePoint;
    public Transform shellPoint;
    public Transform handlingPoint;
    [Tooltip("The left hand grip references to the weapon's front grip")]
    public Transform leftHandGrip;

    [Header("Stats")]
    public float bulletTransversalSpeed = 300f;
    public float damage = 10f;
    public float soundRadius = 10f;

    [Tooltip("How far back the gun should move (positive Z)")]
    public float recoilDistance = 0.1f;

    [Tooltip("How long the recoil motion takes")]
    public float recoilDuration = 0.1f;

    [Tooltip("Easing style for the recoil punch")]
    public Ease recoilEase = Ease.OutQuint;

    private Vector3 originalPos;

    public Vector3 scaleInHand;

    public float shake = 1f;
    public float shotsPerSecond = 10f;
    public int totalMagazines = 3;
    public int clipsPerMagazine = 30;

    [Header("Runtime")]
    public bool isAutomatic = true;
    public bool isShooting;
    public bool showDebugRay = false;


    private float nextFireTime;
    private Coroutine fireCoroutine;
    private Coroutine fireAICoroutine;

    private InputSystem_Actions action;
    private InputAction shoot;

    [Space]
    public GameObject whoFires;

    public PlaceInHolster placeInHolster;

    public LayerMask enemyLayer;

    // 🔫 Runtime counters
    private int currentAmmo;
    private int currentMagazines;

    [Tooltip("Useful if this weapon is in enemy's hands. Bullet Mask usually sets to Default, Land, Wall etc...")]
    public LayerMask bulletMask;

    private void Awake()
    {
        action = new InputSystem_Actions();
        shoot = action.Player.Attack;

        originalPos = transform.localPosition;

        TryGetComponent<SphereCollider>(out SphereCollider sphere);
        sphere.radius = soundRadius;
        sphere.isTrigger = true;

    }
    private void OnEnable()
    {
        action.Enable();
        shoot.started += OnShootStarted;
        shoot.performed += OnShootPerformed;
        shoot.canceled += OnShootCanceled;

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = multiShotSound;
    }

    private void Start()
    {
        currentMagazines = totalMagazines;
        currentAmmo = clipsPerMagazine;
        UpdateUI();
        enemyLayer = OutGameManager.Instance.enemyLayer;

    }

    private void OnDisable()
    {
        shoot.started -= OnShootStarted;
        shoot.performed -= OnShootPerformed;
        shoot.canceled -= OnShootCanceled;
        action.Disable();
    }


    private void OnShootStarted(InputAction.CallbackContext context)
    {

    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        if (!OutGameManager.Instance.currentP.GetComponent<Animator>().GetBool("GunMode")) return;
        if (OutGameManager.Instance.currentWeapon != gameObject) return;

        if (OutGameManager.Instance.currentP.GetComponent<Animator>().GetBool("AimingRifle"))
        {
            PlaySound(multiShotSound, true);
            audioSource.loop = true;
            isShooting = true;
            AmmoOrMagIndicator.InstanceAmmo.Focus();
            AmmoOrMagIndicator.InstanceMag.Focus();
            if (fireCoroutine == null)

                fireCoroutine ??= StartCoroutine(AutoFire());
        }
    }



    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        if (!OutGameManager.Instance.currentP.GetComponent<Animator>().GetBool("GunMode")) return;
        if (!OutGameManager.Instance.currentP.GetComponent<Animator>().GetBool("AimingRifle")) return;
        if (OutGameManager.Instance.currentWeapon != gameObject) return;

        AmmoOrMagIndicator.InstanceAmmo.FadeOut();
        AmmoOrMagIndicator.InstanceMag.FadeOut();
        PlaySound(singleShotSound, false);
        isShooting = false;
        if (fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
            fireCoroutine = null;
        }
    }
    public void StartFireAI()
    {
        PlaySound(multiShotSound, true);
        fireByEnemy = true;
        isShooting = true;
        fireAICoroutine ??= StartCoroutine(AutoFire());
    }
    public void StopFireAI()
    {
        PlaySound(singleShotSound, false);
        isShooting = false;
        if (fireAICoroutine != null)
        {
            StopCoroutine(fireAICoroutine);
            fireAICoroutine = null;
        }
    }

    private IEnumerator ReturnShellToPool(GameObject shell, float delay)
    {
        yield return new WaitForSeconds(delay);
        shell.SetActive(false);
        WeaponPoolManager.Instance.ReturnShell(shell);
    }

    bool fireByEnemy;
    private IEnumerator AutoFire()
    {
        while (true)
        {
            if (fireByEnemy)
            {
                FireAI();
            }
            else
            {
                if (OutGameManager.Instance.currentWeapon == gameObject)
                {
                    Fire();
                }
                else Debug.LogError($"current weapon is not {this.name}");
            }

            yield return new WaitForSeconds(1f / shotsPerSecond);
        }
    }


    private void Fire()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("Clip empty!");
            return;
        }

        currentAmmo--;

        // Bullet
        GameObject bullet = WeaponPoolManager.Instance.GetBullet();
        bullet.transform.SetPositionAndRotation(muzzlePoint.position, muzzlePoint.rotation);
        bullet.SetActive(true);
        bullet.GetComponent<OutBullet>().Init(this, whoFires);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(muzzlePoint.forward * bulletTransversalSpeed, ForceMode.Impulse);


        EmitSound(transform.position, soundRadius);

        Debug.Log("Bullet sent");

        // Recoil
        ApplyRecoil();

        // Camera Shake
        if (!fireByEnemy)
            ApplyCameraShake();

        // Debug Ray (always draw even if no hit)
        Ray ray = new(muzzlePoint.position, muzzlePoint.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (showDebugRay)
            {
                Color rayColor = hit.collider.GetComponent<OutEnemyController>() ? Color.red : Color.green;
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, rayColor, 0.5f);
            }
        }
        else if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green, 0.5f);
        }

        // Effects
        if (muzzleFlash.Count > 0)
        {
            for (int i = 0; i < muzzleFlash.Count; i++) muzzleFlash[i].Play();
        }
        if (muzzleLight != null) StartCoroutine(MuzzleFlashLight());

        // Shell Ejection from Pool
        if (shellPoint != null)
        {
            var shell = WeaponPoolManager.Instance.GetShell();
            shell.transform.SetPositionAndRotation(shellPoint.position, shellPoint.rotation);
            shell.SetActive(true);
            shell.TryGetComponent<Rigidbody>(out Rigidbody shellRb);
            if (shellRb == null) shellRb = shell.AddComponent<Rigidbody>();
            shellRb.linearVelocity = Vector3.zero;
            shellRb.angularVelocity = Vector3.zero;
            shellRb.AddForce(shellPoint.right * Random.Range(1f, 2f), ForceMode.Impulse);
            StartCoroutine(ReturnShellToPool(shell, 2f));
        }

        if (!fireByEnemy)
            UpdateUI();

        // Reload logic when clip ends (auto reload simulation)
        if (currentAmmo <= 0 && currentMagazines > 0)
        {
            StartCoroutine(Reload());
        }

    }
    private void FireAI()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("Clip empty!");
            return;
        }

        currentAmmo--;

        // === Get LookAtTarget from current player ===
        Transform lookAtTarget = null;
        lookAtTarget = OutGameManager.Instance.currentP.GetComponent<OutShooterController>().lookAtTarget;


        if (lookAtTarget == null)
        {
            Debug.LogWarning("No LookAtTarget found for raycast shooting.");
            return;
        }

        // === Raycast Shooting ===
        Vector3 direction = (lookAtTarget.position - muzzlePoint.position).normalized;
        Ray ray = new(muzzlePoint.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, bulletMask)) // bulletMask is your LayerMask
        {
            Debug.Log($"AI Bullet hit {hit.collider.name}");

            // Apply damage if possible
            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // Blood / Impact FX
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            Texture hitTexture = renderer ? renderer.material.mainTexture : null;

            foreach (var impact in OutGameManager.Instance.bulletImpacts)
            {
                if (impact.surfaceTexture == hitTexture)
                {
                    // Particle
                    if (impact.impactEffect != null)
                    {
                        var fx = Instantiate(impact.impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                        fx.Play();
                        Destroy(fx.gameObject, fx.main.duration);
                    }

                    // Audio
                    if (impact.impactSound != null)
                    {
                        AudioSource.PlayClipAtPoint(impact.impactSound, hit.point);
                    }
                    break;
                }
            }

            // Debug Ray (hit)
            if (showDebugRay)
            {
                Color rayColor = hit.collider.GetComponent<OutEnemyController>() ? Color.red : Color.green;
                Debug.DrawRay(ray.origin, direction * hit.distance, rayColor, 0.5f);
            }
        }
        else if (showDebugRay)
        {
            // Debug Ray (miss)
            Debug.DrawRay(ray.origin, direction * 200f, Color.yellow, 0.5f);
        }

        // === Effects ===
        if (muzzleFlash.Count > 0)
        {
            for (int i = 0; i < muzzleFlash.Count; i++) muzzleFlash[i].Play();
        }
        if (muzzleLight != null) StartCoroutine(MuzzleFlashLight());

        // === Shell Ejection ===
        if (shellPoint != null)
        {
            var shell = WeaponPoolManager.Instance.GetShell();
            shell.transform.SetPositionAndRotation(shellPoint.position, shellPoint.rotation);
            shell.SetActive(true);
            shell.TryGetComponent<Rigidbody>(out Rigidbody shellRb);
            if (shellRb == null) shellRb = shell.AddComponent<Rigidbody>();
            shellRb.linearVelocity = Vector3.zero;
            shellRb.angularVelocity = Vector3.zero;
            shellRb.AddForce(shellPoint.right * Random.Range(1f, 2f), ForceMode.Impulse);
            StartCoroutine(ReturnShellToPool(shell, 2f));
        }

        // === Sound ===
        EmitSound(transform.position, soundRadius);

        // === Auto Reload ===
        if (currentAmmo <= 0 && currentMagazines > 0)
        {
            StartCoroutine(Reload());
        }
    }


    void PlaySound(AudioClip audioClip, bool loop)
    {
        audioSource.clip = audioClip;
        audioSource.loop = loop;
        audioSource.Play();
    }

    void EmitSound(Vector3 soundPos, float soundRadius)
    {
        Collider[] hits = Physics.OverlapSphere(soundPos, soundRadius, enemyLayer);
        foreach (var col in hits)
        {
            var enemy = col.GetComponent<OutEnemyController>();
            enemy?.OnHeardNoise(soundPos);
        }
    }

    private IEnumerator Reload()
    {
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(1f); // simulate reload time

        if (currentMagazines > 0)
        {
            currentMagazines--;
            currentAmmo = clipsPerMagazine;

            if (!fireByEnemy)
                UpdateUI();
        }
    }

    public void ApplyRecoil()
    {
        transform.DOKill();

        if (!fireByEnemy)
        {
            OutGameManager.Instance.currentP.GetComponent<OutTPSystem>().cameraTransform.DOPunchPosition(
                new Vector3(0, 0, -recoilDistance),
                recoilDuration,
                vibrato: 1,
                elasticity: 0.5f
            ).SetEase(recoilEase);
        }
    }

    private void UpdateUI()
    {
        if (ShooterUI.Instance.ammoFill != null)
            ShooterUI.Instance.ammoFill.fillAmount = (float)currentAmmo / clipsPerMagazine;

        if (ShooterUI.Instance.magFill != null)
            ShooterUI.Instance.magFill.fillAmount = (float)currentMagazines / totalMagazines;
    }

    private void ApplyCameraShake()
    {
        // A simple camera shake implementation (use Cinemachine or custom logic for better effects)
        GameObject.FindWithTag("MainCamera").TryGetComponent<OutCameraController>(out OutCameraController cameraController);
        if (cameraController != null)
        {
            cameraController.ShakeCamera(shake); // You must implement this method in OutCameraController
        }
    }


    private IEnumerator MuzzleFlashLight()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(1f / shotsPerSecond);
        muzzleLight.enabled = false;
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
}
