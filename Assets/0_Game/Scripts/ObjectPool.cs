using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Bullet Prefabs")]
    [SerializeField] private GameObject normalBulletPrefab;
    [SerializeField] private GameObject laserBulletPrefab;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 30;
    [SerializeField] private int maxPoolSize = 120;

    private readonly Dictionary<BulletType, Queue<GameObject>> pools = new Dictionary<BulletType, Queue<GameObject>>();
    private readonly Dictionary<BulletType, int> totalCounts = new Dictionary<BulletType, int>();

    private void Awake()
    {
        pools[BulletType.Normal] = new Queue<GameObject>();
        pools[BulletType.Laser] = new Queue<GameObject>();
        totalCounts[BulletType.Normal] = 0;
        totalCounts[BulletType.Laser] = 0;

        WarmPool(BulletType.Normal);
        WarmPool(BulletType.Laser);
    }

    public GameObject GetObject(BulletType bulletType)
    {
        Queue<GameObject> pool = pools[bulletType];

        if (pool.Count == 0 && totalCounts[bulletType] < maxPoolSize)
        {
            CreateNewBullet(bulletType);
        }

        while (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            if (obj == null)
            {
                continue;
            }

            obj.SetActive(true);
            return obj;
        }

        Debug.LogWarning($"Pool for {bulletType} is empty and reached max size.");
        return null;
    }

    public void ReturnObject(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        obj.SetActive(false);

        Bullet bullet = obj.GetComponent<Bullet>();
        if (bullet == null)
        {
            return;
        }

        Queue<GameObject> pool = pools[bullet.BulletType];
        if (!pool.Contains(obj))
        {
            pool.Enqueue(obj);
        }
    }

    private void WarmPool(BulletType bulletType)
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewBullet(bulletType);
        }
    }

    private void CreateNewBullet(BulletType bulletType)
    {
        GameObject prefab = bulletType == BulletType.Laser ? laserBulletPrefab : normalBulletPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"Missing prefab for bullet type {bulletType}");
            return;
        }

        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        pools[bulletType].Enqueue(obj);
        totalCounts[bulletType]++;
    }
}
