using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public ObjectPool bulletPool;
    public Transform  firePoint;
    public float      bulletSpeed = 20f;
    public BulletType bulletType = BulletType.Normal;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject bullet = bulletPool.GetObject(bulletType);
        if (bullet == null)
        {
            return;
        }

        bullet.transform.position = firePoint.position;
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = firePoint.right * bulletSpeed;
        }
    }
}
