using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPoolManager : MonoBehaviour
{
    public static WeaponPoolManager Instance { get; private set; }

    [Header("Shared Prefabs")]
    public GameObject gunBullet;
    public GameObject turretBullet;
    public GameObject shellPrefab;
    public GameObject bloodParticlePrefab;

    [Header("Pool Sizes")]
    public int bulletPoolSize = 20;
    public int turretBulletPoolSize = 10;
    public int shellPoolSize = 20;
    public int bloodPoolSize = 20;

    private Queue<GameObject> bullets = new();
    private Queue<GameObject> turretBullets = new();
    private Queue<GameObject> shells = new();
    private Queue<GameObject> bloodParticles = new();

    [Header("Enable Pools")]
    public bool PoolSmallBullets = true;
    public bool PoolTurretBullets = true;
    public bool PoolShells = true;
    public bool PoolBloodParticles = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (PoolSmallBullets)
            PreloadPool(gunBullet, bulletPoolSize, bullets);

        if (PoolTurretBullets)
            PreloadPool(turretBullet, turretBulletPoolSize, turretBullets);

        if (PoolShells)
            PreloadPool(shellPrefab, shellPoolSize, shells);

        if (PoolBloodParticles)
            PreloadPool(bloodParticlePrefab, bloodPoolSize, bloodParticles);
    }

    private void OnEnable()
    {
        //OutGameManager.OnCurrentWeaponChanged += ChangeBulletPool;
    }

    private void ChangeBulletPool(GameObject currentBulletPrefab)
    {
        // 1. Clear old bullet pool
        while (bullets.Count > 0)
        {
            Destroy(bullets.Dequeue());
        }

        // 2. Assign the new bullet prefab
        gunBullet = currentBulletPrefab;

        // 3. Re-initialize the pool
        if (PoolSmallBullets && gunBullet != null)
            PreloadPool(gunBullet, bulletPoolSize, bullets);
    }

    private void PreloadPool(GameObject prefab, int count, Queue<GameObject> queue)
    {
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab);
            go.SetActive(false);
            queue.Enqueue(go);
        }
    }

    // -------- SMALL BULLETS --------
    public GameObject GetBullet()
    {
        if (PoolSmallBullets && bullets.Count > 0)
            return bullets.Dequeue();

        var b = Instantiate(gunBullet);
        b.SetActive(false);
        return b;
    }

    public void ReturnBullet(GameObject b)
    {
        if (PoolSmallBullets)
            CleanAndEnqueue(b, bullets, bulletPoolSize);
        else
            Destroy(b);
    }

    // -------- TURRET BULLETS --------
    public GameObject GetTurretBullet()
    {
        if (PoolTurretBullets && turretBullets.Count > 0)
            return turretBullets.Dequeue();

        var b = Instantiate(turretBullet);
        b.SetActive(false);
        return b;
    }

    public void ReturnTurretBullet(GameObject b)
    {
        if (PoolTurretBullets)
            CleanAndEnqueue(b, turretBullets, turretBulletPoolSize);
        else
            Destroy(b);
    }

    // -------- SHELLS --------
    public GameObject GetShell()
    {
        if (PoolShells && shells.Count > 0)
            return shells.Dequeue();

        var s = Instantiate(shellPrefab);
        s.SetActive(false);
        return s;
    }

    public void ReturnShell(GameObject s)
    {
        if (PoolShells)
            CleanAndEnqueue(s, shells, shellPoolSize);
        else
            Destroy(s);
    }

    // -------- BLOOD --------
    public GameObject GetBlood()
    {
        if (PoolBloodParticles && bloodParticles.Count > 0)
            return bloodParticles.Dequeue();

        var b = Instantiate(bloodParticlePrefab);
        b.SetActive(false);
        return b;
    }

    public void ReturnBlood(GameObject b)
    {
        if (PoolBloodParticles)
            CleanAndEnqueue(b, bloodParticles, bloodPoolSize);
        else
            Destroy(b);
    }

    // -------- UTIL --------
    private void CleanAndEnqueue(GameObject obj, Queue<GameObject> queue, int maxPoolSize)
    {
        obj.SetActive(false);
        if (queue.Count < maxPoolSize)
            queue.Enqueue(obj);
        else
            Destroy(obj);
    }
    private void OnDisable()
    {
        //OutGameManager.OnCurrentWeaponChanged -= ChangeBulletPool;
    }
}
