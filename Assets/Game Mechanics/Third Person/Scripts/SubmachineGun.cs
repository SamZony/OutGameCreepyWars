using UnityEngine;
using System.Collections;
using TMPro;

public class SubmachineGun : MonoBehaviour
{
    public AudioSource fireSound;
    public ParticleSystem muzzleFlash;
    public ParticleSystem smokeParticles;
    public Animator playerAnimator;
    public TextMeshProUGUI ammoText;
    public GameObject bulletPrefab;
    public GameObject shellPrefab;
    public Vector3 ShellSize;
    public Transform bulletSpawnPoint;
    public Transform shellSpawnPoint;

    public int magazineSize = 30;
    public int currentAmmo = 30;
    public int magazinesLeft = 5;

    private bool isFiring = false;
    private float fireRate = 10f; // Bullets per second
    private float nextFireTime = 0f;
    private float cameraShakeDuration = 0.5f;
    private float cameraShakeIntensity = 0.2f;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            StartFiring();
        }

        if (Input.GetButtonUp("Fire1"))
        {
            StopFiring();
        }

        if (isFiring && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate;
        }

        // Update ammo text
        ammoText.text = currentAmmo.ToString() + " / " + (magazineSize * magazinesLeft).ToString();
    }

    void StartFiring()
    {
        isFiring = true;
        playerAnimator.SetLayerWeight(2, 1);
        playerAnimator.Play("Fire");
    }

    void StopFiring()
    {
        isFiring = false;
        playerAnimator.SetLayerWeight(2, 0);
        StartCoroutine(CameraShake(cameraShakeDuration, cameraShakeIntensity));
    }

    void Fire()
    {
        if (currentAmmo > 0)
        {
            fireSound.Play();
            muzzleFlash.Play();
            smokeParticles.Play();

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.AddComponent<Rigidbody>();
            bullet.GetComponent<Rigidbody>().AddForce(bulletSpawnPoint.forward * 1000f, ForceMode.Impulse);

            GameObject shell = Instantiate(shellPrefab, shellSpawnPoint.position, shellSpawnPoint.rotation);
            shell.transform.localScale = ShellSize;
            shell.AddComponent<Rigidbody>();
            shell.AddComponent<MeshCollider>().convex = true;
            shell.GetComponent<Rigidbody>().AddForce(Vector3.down * 1000f, ForceMode.Impulse);
            Destroy(shell, 2f);

            currentAmmo--;

            if (currentAmmo == 0)
            {
                StopFiring();
            }
        }
    }

    IEnumerator CameraShake(float duration, float intensity)
    {
        float elapsedTime = 0f;
        Vector3 originalPosition = Camera.main.transform.position;

        while (elapsedTime < duration)
        {
            float x = Random.Range(-intensity, intensity);
            float y = Random.Range(-intensity, intensity);
            float z = Random.Range(-intensity, intensity);

            Camera.main.transform.position = originalPosition + new Vector3(x, y, z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = originalPosition;
    }
}