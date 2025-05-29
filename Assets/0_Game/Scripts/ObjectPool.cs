using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public  GameObject        bulletPrefab;
    public  int               initialPoolSize = 50;
    public  int               maxPoolSize     = 200;
    private Queue<GameObject> pool;

    void Awake()
    {
        pool = new Queue<GameObject>();
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewBullet();
        }
    }

    public GameObject GetObject()
    {
        if (pool.Count == 0 && pool.Count < maxPoolSize)
        {
            CreateNewBullet(); // Tạo mới nếu chưa đạt max
        }

        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        Debug.LogWarning("Pool is empty and reached max size!");
        return null;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        if (!pool.Contains(obj))
        {
            pool.Enqueue(obj);
        }
    }

    private void CreateNewBullet()
    {
        GameObject obj = Instantiate(bulletPrefab, this.transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}